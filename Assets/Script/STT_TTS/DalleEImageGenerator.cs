using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class DalleEImageGenerator : MonoBehaviour
{
    public IEnumerator GenerateImages(string prompt)
    {
        // DALL·E 요청 페이로드 구성
        var payload = new
        {
            prompt = prompt,
            n = 1, // 한 장만 생성
            size = "1024x1024"  // 또는 "512x512"도 가능
        };

        string jsonPayload = JsonConvert.SerializeObject(payload);

        UnityWebRequest request = new UnityWebRequest("https://api.openai.com/v1/images/generations", "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonPayload));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", $"Bearer {ApiKeyLoader.OpenAiApiKey}");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            JObject result = JObject.Parse(request.downloadHandler.text);
            JArray images = (JArray)result["data"];

            if (images.Count > 0)
            {
                string imageUrl = images[0]["url"]?.ToString();
                Debug.Log($"🎨 Image URL: {imageUrl}");
                yield return StartCoroutine(DownloadAndApplyTexture(imageUrl));
            }
        }
        else
        {
            Debug.LogError($"DALL-E Error: {request.responseCode} - {request.error} - {request.downloadHandler.text}");
        }
    }

    IEnumerator DownloadAndApplyTexture(string url)
    {
        UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url);
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(uwr);

            // 현재 방 번호에 해당하는 SphereRoom 이름 찾기
            string currentRoomName = "SphereRoom" + RoomTeleport.CurrentRoomNumber;
            GameObject targetObject = GameObject.Find(currentRoomName);

            if (targetObject != null)
            {
                Renderer rend = targetObject.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material.mainTexture = texture;
                    Debug.Log($"✅ {currentRoomName}에 텍스처가 적용되었습니다.");
                }
                else
                {
                    Debug.LogError($"{currentRoomName}의 Renderer를 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogError($"{currentRoomName} 오브젝트를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogError("이미지 다운로드 실패: " + uwr.error);
        }
    }
}