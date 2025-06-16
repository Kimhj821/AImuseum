using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Linq;

public class VoiceInteraction : MonoBehaviour
{
    private string folderPath;
    private string sttPath, ttsPath, textPath, instructionPath;
    private AudioClip recordedClip;
    private volatile bool isRecording = false;

    public bool Voice = false;
    public Text gptLegacyText;

    public InputActionAsset inputAsset;
    private InputAction bButtonAction;

    public GameObject D;
    ReplicateImageGenerator repliScript;

    void Start()
    {
        D = GameObject.Find("API_Management");
        repliScript = D.transform.GetComponent<ReplicateImageGenerator>();

        folderPath = Path.Combine(Application.streamingAssetsPath);
        sttPath = Path.Combine(folderPath, "STT.wav");
        ttsPath = Path.Combine(folderPath, "TTS.mp3");
        textPath = Path.Combine(folderPath, "answer.txt");
        instructionPath = Path.Combine(folderPath, "instruction_guide.txt");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var rightMap = inputAsset.FindActionMap("XRI Right Interaction");
        bButtonAction = rightMap.FindAction("BbuttonAction");

        if (bButtonAction != null)
        {
            bButtonAction.Enable();
            bButtonAction.performed += OnBButtonPressed;
            bButtonAction.canceled += OnBButtonReleased;
        }
    }

    void Update()
    {
        if (!isRecording && Input.GetKeyDown(KeyCode.T))
        {
            Voice = true;
            StartRecording();
        }
        else if (isRecording && Input.GetKeyUp(KeyCode.T))
        {
            Voice = false;
            StopRecording();
            StartCoroutine(ProcessAudioFlow());
        }
    }

    private void OnBButtonPressed(InputAction.CallbackContext context)
    {
        Voice = true;
        StartRecording();
        Debug.Log("VR 녹음 시작");
    } 
    private void OnBButtonReleased(InputAction.CallbackContext context)
    {
        Voice = false;
        StopRecording();
        StartCoroutine(ProcessAudioFlow());
        Debug.Log("VR 녹음 종료");
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

    void StartRecording() { recordedClip = Microphone.Start(null, false, 10, 44100); isRecording = true; }
    void StopRecording() { if (isRecording) { Microphone.End(null); SaveRecording(); isRecording = false; } }

    void SaveRecording()
    {
        if (recordedClip == null) return;
        var samples = new float[recordedClip.samples];
        recordedClip.GetData(samples, 0);
        WavUtility.FromAudioClip(recordedClip, sttPath, true);
    }
    IEnumerator ProcessAudioFlow()
    {
        // 1️⃣ STT 변환
        yield return StartCoroutine(SendToSTT());
        if (!File.Exists(textPath)) yield break;
        string sttPrompt = File.ReadAllText(textPath).Trim();
        Debug.Log("STT 결과: " + sttPrompt);

        // 2️⃣ GPT 호출 (한국어 응답 생성)
        yield return StartCoroutine(SendToGPT(sttPrompt));
        if (!File.Exists(textPath)) yield break;
        string gptResponseKorean = File.ReadAllText(textPath).Trim();
        Debug.Log("GPT 응답(한글): " + gptResponseKorean);

       if (gptResponseKorean.Contains("(Call_Dall-E)"))
        {

            GameObject currentRoom = GameObject.Find("SphereRoom" + RoomTeleport.CurrentRoomNumber);

            RoomInfo roomInfo = currentRoom?.GetComponent<RoomInfo>();
        if (roomInfo != null && roomInfo.PlayerNum == RoomTeleport.CurrentRoomNumber)
            {
                Debug.Log("🎨 DALL-E 호출 플로우로 전환 (방 번호 확인 완료)");

                string replicKorean = gptResponseKorean.Replace("(Call_Dall-E)", "").Trim();
                string translationPrompt = $"다음을 영어로 번역해줘:\n{replicKorean}";

                yield return StartCoroutine(SendToGPT(translationPrompt));
                if (!File.Exists(textPath)) yield break;
                string gptResponseEnglish = File.ReadAllText(textPath).Trim();
                Debug.Log("GPT 응답(영어): " + gptResponseEnglish);

                var repliScript = FindFirstObjectByType<ReplicateImageGenerator>();
                if (repliScript != null)
                {
                    StartCoroutine(repliScript.GenerateImages(gptResponseEnglish));
                }
                else
                {
                    Debug.LogError("DalleEImageGenerator 스크립트를 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.Log($"⚠️ 현재 방 번호({RoomTeleport.CurrentRoomNumber})에서는 DALL-E 호출이 비활성화됨");
            }
        }
        else
        {
            Debug.Log("🎤 TTS 및 UI 텍스트 출력 플로우 진행");

            // TTS 출력 및 UI 업데이트
            // ✅ 여기서만 gptLegacyText 업데이트
            if (gptLegacyText != null) gptLegacyText.text = gptResponseKorean;

            yield return StartCoroutine(SendToTTS(gptResponseKorean));
            PlayAudio(ttsPath);
        }
    }


    // IEnumerator ProcessAudioFlow()
    // {
    //     yield return StartCoroutine(SendToSTT());

    //     if (!File.Exists(textPath)) yield break;
    //     string prompt = File.ReadAllText(textPath);

    //     if (prompt.Contains("(code_1)"))  // (code_1) 검출 시
    //     {
    //         Debug.Log("DALL-E 이미지 생성 흐름으로 전환");
    //         var dalleScript = FindObjectOfType<DalleEImageGenerator>(); 
    //         if (dalleScript != null)
    //         {
    //             StartCoroutine(dalleScript.SetWallsToRed(prompt));
    //         }
    //     }
    //     else
    //     {
    //         Debug.Log("GPT → TTS 흐름 진행");
    //         yield return StartCoroutine(SendToGPT(prompt));
    //         string gptResponse = File.ReadAllText(textPath);
    //         yield return StartCoroutine(SendToTTS(gptResponse));
    //         PlayAudio(ttsPath);
    //     }
    // }

    IEnumerator SendToSTT()
    {
        byte[] audioData = File.ReadAllBytes(sttPath);
        var formData = new List<IMultipartFormSection>
        {
            new MultipartFormFileSection("file", audioData, "STT.wav", "audio/wav"),
            new MultipartFormDataSection("model", "whisper-1")
        };
        UnityWebRequest request = UnityWebRequest.Post("https://api.openai.com/v1/audio/transcriptions", formData);
        request.SetRequestHeader("Authorization", $"Bearer {ApiKeyLoader.OpenAiApiKey}");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string transcript = JObject.Parse(request.downloadHandler.text)["text"]?.ToString();
            Debug.Log("STT 결과 (transcript): "+ transcript);
            File.WriteAllText(textPath, transcript);
        }
        else
        {
            Debug.LogError("STT 실패" + request.error);
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

        UnityWebRequest request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload)));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", $"Bearer {ApiKeyLoader.OpenAiApiKey}");
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string reply = JObject.Parse(request.downloadHandler.text)["choices"]?[0]?["message"]?["content"]?.ToString();
            File.WriteAllText(textPath, reply);
        }
    }

    IEnumerator SendToTTS(string replyText)
    {
        if (string.IsNullOrEmpty(replyText)) yield break;
        var payload = new { text = replyText, voice_settings = new { stability = 0.75, similarity_boost = 0.75 } };
        UnityWebRequest request = new UnityWebRequest($"https://api.elevenlabs.io/v1/text-to-speech/{ApiKeyLoader.ElevenVoiceId}", "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload)));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("xi-api-key", ApiKeyLoader.ElevenApiKey);
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            File.WriteAllBytes(ttsPath, request.downloadHandler.data);
    }

    void PlayAudio(string path)
    {
        if (File.Exists(path)) StartCoroutine(PlayClip(path));
    }

    IEnumerator PlayClip(string filePath)
    {
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file:///" + filePath.Replace("\\", "/"), AudioType.MPEG);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            AudioSource audio = gameObject.AddComponent<AudioSource>();
            audio.clip = DownloadHandlerAudioClip.GetContent(www);
            audio.Play();
            Destroy(audio, audio.clip.length + 0.5f);
        }
    }
}