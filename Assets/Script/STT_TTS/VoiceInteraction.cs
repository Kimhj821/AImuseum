using UnityEngine;
using UnityEngine.InputSystem;  // Input System 네임스페이스 추가
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

public class VoiceInteraction : MonoBehaviour
{
    private AudioClip recordedClip;
    private string folderPath;
    private string sttPath;
    private string ttsPath;
    private string textPath;
    private string instructionPath;

    private volatile bool isRecording = false;

    [SerializeField]
    private string elevenApiKey = "";
    
    [SerializeField]
    private string openAiApiKey = "";
    
    [SerializeField]
    private string elevenVoiceId = "";

    public bool Voice = false;
    public Text gptLegacyText;

    public InputActionAsset inputAsset; // 인스펙터 연결
    private InputAction bButtonAction;  // B 버튼 액션
    public InputActionReference bButtonActionReference;  // 인스펙터에 연결

    void Start()
    {
        folderPath = Path.Combine(Application.streamingAssetsPath);
        sttPath = Path.Combine(folderPath, "STT.wav");
        ttsPath = Path.Combine(folderPath, "TTS.mp3");
        textPath = Path.Combine(folderPath, "answer.txt");
        instructionPath = Path.Combine(folderPath, "instruction_guide.txt");
        var rightMap = inputAsset.FindActionMap("XRI Right Interaction");
        bButtonAction = rightMap.FindAction("BbuttonAction");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        if (bButtonAction != null)
        {
            bButtonAction.Enable();
            bButtonAction.performed += OnBButtonPressed;
            bButtonAction.canceled += OnBButtonReleased;
        }
        else
        {
            Debug.LogError("BbuttonAction을 찾지 못했습니다!");
        }
    }

    void Update()
    {
        // PC 키보드 입력 처리 (T키)
        if (!isRecording && Input.GetKeyDown(KeyCode.T))
        {
            Voice = true;
            Debug.Log("PC: 녹음 시작");
            StartRecording();
        }
        else if (isRecording && Input.GetKeyUp(KeyCode.T))
        {
            Voice = false;
            Debug.Log("PC: 녹음 종료");
            StopRecording();
            StartCoroutine(ProcessAudioFlow());
        }
    }

    private void OnBButtonPressed(InputAction.CallbackContext context)
    {
        if (!isRecording)
        {
            Voice = true;
            Debug.Log("VR: 녹음 시작 (B 버튼)");
            StartRecording();
        }
    }

    private void OnBButtonReleased(InputAction.CallbackContext context)
    {
        if (isRecording)
        {
            Voice = false;
            Debug.Log("VR: 녹음 종료 (B 버튼)");
            StopRecording();
            StartCoroutine(ProcessAudioFlow());
        }
    }

    private void OnDisable()
    {
        if (bButtonAction != null)
        {
            bButtonAction.performed -= OnBButtonPressed;
            bButtonAction.canceled -= OnBButtonReleased;
            bButtonAction.Disable();
        }
    }

    void StartRecording()
    {
        recordedClip = Microphone.Start(null, false, 10, 44100);
        isRecording = true;
    }

    void StopRecording()
    {
        if (!isRecording) return;
        Microphone.End(null);
        SaveRecording();
        isRecording = false;
    }

    void SaveRecording()
    {
        if (recordedClip == null) return;

        var samples = new float[recordedClip.samples];
        recordedClip.GetData(samples, 0);

        WavUtility.FromAudioClip(recordedClip, sttPath, true);
        Debug.Log("녹음 저장 경로: " + sttPath);
    }

    IEnumerator ProcessAudioFlow()
    {
        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(SendToSTT());

        if (!File.Exists(textPath))
        {
            Debug.LogError("STT 실패로 인해 'answer.txt' 파일이 생성되지 않았습니다. 흐름을 중단합니다.");
            yield break;
        }

        yield return new WaitForSeconds(0.2f);

        string prompt = File.ReadAllText(textPath);

        // 🔥 여기에서 키워드 감지 및 DALL-E 호출 분기 추가
        if (prompt.Contains("방을 꾸며줘"))  // 또는 prompt.ToLower().Contains("방 꾸며")
        {
            Debug.Log("DALL-E 이미지 생성 흐름으로 전환");
            var dalleScript = GameObject.Find("DalleEGeneratorObject").GetComponent<DalleEImageGenerator>();
        }
        else
        {
            Debug.Log("GPT 설명 흐름 시작");
            yield return StartCoroutine(SendToGPT(prompt));

            yield return new WaitForSeconds(0.2f);

            string gptResponse = File.ReadAllText(textPath);
            yield return StartCoroutine(SendToTTS(gptResponse));

            float timeout = 2f;
            float elapsed = 0f;
            while (!IsFileReady(ttsPath) && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }

        PlayAudio(ttsPath);
        }
    }
    IEnumerator SendToSTT()
    {
        byte[] audioData = File.ReadAllBytes(sttPath);

        var formData = new List<IMultipartFormSection>
        {
            new MultipartFormFileSection("file", audioData, "STT.wav", "audio/wav"),
            new MultipartFormDataSection("model", "whisper-1")
        };

        UnityWebRequest request = UnityWebRequest.Post("https://api.openai.com/v1/audio/transcriptions", formData);
        request.SetRequestHeader("Authorization", $"Bearer {openAiApiKey}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            JObject result = JObject.Parse(request.downloadHandler.text);
            string transcript = result["text"]?.ToString();
            File.WriteAllText(textPath, transcript);
            Debug.Log("STT 결과: " + transcript);
        }
        else
        {
            Debug.LogError($"STT Error: {request.responseCode} - {request.error} - {request.downloadHandler.text}");
        }
    }

    IEnumerator SendToGPT(string userText)
    {
        string instruction = File.Exists(instructionPath) ? File.ReadAllText(instructionPath) : "";

        var payload = new
        {
            model = "gpt-3.5-turbo",
            messages = new object[]
            {
                new { role = "system", content = instruction },
                new { role = "user", content = userText }
            }
        };

        string jsonPayload = JsonConvert.SerializeObject(payload);
        UnityWebRequest request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonPayload));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", $"Bearer {openAiApiKey}");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            JObject result = JObject.Parse(request.downloadHandler.text);
            string reply = result["choices"]?[0]?["message"]?["content"]?.ToString();
            string finalText = "GPT의 답변: " + reply;

            File.WriteAllText(textPath, finalText);
            if (gptLegacyText != null)
            {
                gptLegacyText.text = finalText;
            }
        }
    }

    IEnumerator SendToTTS(string replyText)
    {
        if (string.IsNullOrEmpty(replyText))
        {
            Debug.LogWarning("TTS 입력 텍스트가 비어 있습니다.");
            yield break;
        }

        var payload = new
        {
            text = replyText,
            voice_settings = new { stability = 0.75, similarity_boost = 0.75 }
        };

        string jsonPayload = JsonConvert.SerializeObject(payload);

        UnityWebRequest request = new UnityWebRequest($"https://api.elevenlabs.io/v1/text-to-speech/{elevenVoiceId}", "POST");


        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonPayload));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("xi-api-key", elevenApiKey);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            File.WriteAllBytes(ttsPath, request.downloadHandler.data);
            Debug.Log("TTS 저장 완료");
            yield return new WaitForSeconds(3f); // 파일 저장 후 대기
        }
        else
        {
            Debug.LogError($"TTS Error: {request.responseCode} - {request.error} - {request.downloadHandler.text}");
        }
    }

    void PlayAudio(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"오디오 파일이 존재하지 않습니다: {path}");
            return;
        }
        StartCoroutine(PlayClip(path));
    }

    IEnumerator PlayClip(string filePath)
    {
        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file:///" + filePath.Replace("\\", "/"), AudioType.MPEG);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            AudioSource tempAudioSource = gameObject.AddComponent<AudioSource>();
            tempAudioSource.clip = clip;
            tempAudioSource.Play();
            Destroy(tempAudioSource, clip.length + 0.5f);
        }
        else
        {
            Debug.LogError($"Audio play error for {filePath}: {www.error}");
        }
    }

    bool IsFileReady(string filePath)
    {
        try
        {
            using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                return stream.Length > 44;
            }
        }
        catch
        {
            return false;
        }
    }
}
