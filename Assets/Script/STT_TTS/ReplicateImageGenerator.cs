using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System;

public class ReplicateImageGenerator : MonoBehaviour
{
    private const string generateApiUrl = "https://api.replicate.com/v1/predictions";
    private const string generateModel = "d26037255a2b298408505e2fbd0bf7703521daca8f07e8c8f335ba874b4aa11a"; // flux (igorriti/flux-360)
    private const string upscaleModel = "d0ee3d708c9b911f122a4ad90046c5d26a0293b99476d697f6bb7f2e251ce2d4"; // realEsrgan (nightmareai/real-esrgan)

    // flux 이미지 생성 요청
    public IEnumerator GenerateImages(string prompt)
    {
        var payload = new
        {
            version = generateModel,
            input = new
            {
                prompt = prompt,
                aspect_ratio = "custom",
                height = 512,
                width = 1024,
                output_format = "png"
            }
        };

        string jsonPayload = JsonConvert.SerializeObject(payload);

        UnityWebRequest request = new UnityWebRequest(generateApiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonPayload));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", $"Token {ApiKeyLoader.ReplicateApiKey}");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            JObject result = JObject.Parse(request.downloadHandler.text);
            string getUrl = result["urls"]?["get"]?.ToString();

            if (!string.IsNullOrEmpty(getUrl))
                yield return StartCoroutine(PollGenerateResult(getUrl));
            else
                Debug.LogError("결과 URL을 찾을 수 없습니다.");
        }
        else
        {
            Debug.LogError($"Replicate 호출 실패: {request.responseCode} - {request.error} - {request.downloadHandler.text}");
        }
    }

    private IEnumerator PollGenerateResult(string url)
    {
        while (true)
        {
            UnityWebRequest poll = UnityWebRequest.Get(url);
            poll.SetRequestHeader("Authorization", $"Token {ApiKeyLoader.ReplicateApiKey}");
            yield return poll.SendWebRequest();

            if (poll.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(poll.downloadHandler.text);
                string status = result["status"]?.ToString();

                if (status == "succeeded")
                {
                    string imageUrl = result["output"]?.First?.ToString();
                    Debug.Log("✅ 이미지 생성 완료: " + imageUrl);

                    string folderPath = Path.Combine(Application.persistentDataPath, "PanoramaImages");
                    Directory.CreateDirectory(folderPath);
                    string savePath = Path.Combine(folderPath, "generated.png");
                    yield return StartCoroutine(DownloadImage(imageUrl, savePath));
                    yield return StartCoroutine(UpscaleWithReplicate(savePath));
                    break;
                }
                else if (status == "failed")
                {
                    Debug.LogError("❌ 이미지 생성 실패");
                    break;
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator DownloadImage(string url, string savePath)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(url);
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.Success)
        {
            File.WriteAllBytes(savePath, uwr.downloadHandler.data);
            Debug.Log("📁 이미지 저장 완료: " + savePath);
        }
        else
        {
            Debug.LogError("❌ 이미지 다운로드 실패: " + uwr.error);
        }
    }

    // realEsrgan 업스케일링 요청
    private IEnumerator UpscaleWithReplicate(string inputPath)
    {
        byte[] imageBytes = File.ReadAllBytes(inputPath);
        string base64Image = Convert.ToBase64String(imageBytes);

        // 유효성 검사
        if (string.IsNullOrEmpty(base64Image) || base64Image.Length < 1000)
        {
            Debug.LogError("⚠️ base64 인코딩 실패 또는 이미지가 너무 작음!");
            yield break;
        }

        var upscalePayload = new
        {
            version = upscaleModel,
            input = new
            {
                image = "data:image/png;base64," + base64Image,
                upscale = 4
            }
        };

        string json = JsonConvert.SerializeObject(upscalePayload);

        UnityWebRequest request = new UnityWebRequest(generateApiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", $"Token {ApiKeyLoader.ReplicateApiKey}");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            JObject result = JObject.Parse(request.downloadHandler.text);
            string getUrl = result["urls"]?["get"]?.ToString();
            if (!string.IsNullOrEmpty(getUrl))
                yield return StartCoroutine(PollUpscaleResult(getUrl));
        }
        else
        {
            Debug.LogError("❌ realEsrGan 업스케일링 요청 실패: " + request.error);
        }
    }

    private IEnumerator PollUpscaleResult(string url)
    {
        while (true)
        {
            UnityWebRequest poll = UnityWebRequest.Get(url);
            poll.SetRequestHeader("Authorization", $"Token {ApiKeyLoader.ReplicateApiKey}");
            yield return poll.SendWebRequest();

            if (poll.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(poll.downloadHandler.text);
                string status = result["status"]?.ToString();
                if (status == "succeeded")
                {
                    string upscaleUrl = result["output"]?.ToString();
                    Debug.Log("✅ 업스케일 완료: " + upscaleUrl);

                    string upscaleFolder = Path.Combine(Application.persistentDataPath, "PanoramaImages", "4k");
                    Directory.CreateDirectory(upscaleFolder);
                    string upscalePath = Path.Combine(upscaleFolder, "generated_4k.png");

                    UnityWebRequest img = UnityWebRequest.Get(upscaleUrl);
                    yield return img.SendWebRequest();

                    if (img.result == UnityWebRequest.Result.Success)
                    {
                        File.WriteAllBytes(upscalePath, img.downloadHandler.data);
                        Debug.Log("📁 4K 이미지 저장 완료: " + upscalePath);

                        // ★ 업스케일 이미지가 저장된 후 텍스처 적용!
                        yield return StartCoroutine(ApplyTexture(upscalePath));
                    }
                    break;
                }
                else if (status == "failed")
                {
                    Debug.LogError("❌ 업스케일링 실패");
                    break;
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    // 머티리얼에 4K 텍스처 적용
    private IEnumerator ApplyTexture(string texturePath)
    {
        byte[] pngBytes = File.ReadAllBytes(texturePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(pngBytes);
        texture.Apply();

        string currentRoomName = "SphereRoom" + RoomTeleport.CurrentRoomNumber;
        GameObject targetObject = GameObject.Find(currentRoomName);
        if (targetObject != null)
        {
            Renderer rend = targetObject.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.mainTexture = texture;
                Debug.Log("🎨 머티리얼에 텍스처 적용 완료 (4K)");
            }
            else
            {
                Debug.LogWarning("🎨 렌더러가 없습니다: " + currentRoomName);
            }
        }
        else
        {
            Debug.LogWarning("🛑 타겟 오브젝트를 찾지 못함: " + currentRoomName);
        }
        yield return null;
    }
}
