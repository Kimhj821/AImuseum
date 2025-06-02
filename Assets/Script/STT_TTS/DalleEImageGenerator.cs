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
        string[] locationSuffixes = {"천장", "정면", "후면", "좌측", "우측" };

        for (int i = 0; i < 5; i++)
        {
            string specificPrompt = $"{prompt} - {locationSuffixes[i]}";

            var payload = new
            {
                prompt = specificPrompt,
                n = 1,  // 각 요청당 1장 생성 (DALL-E 2는 여러 장 요청 가능하나, 위치별로 1장씩 처리)
                size = "512x512"  // DALL-E 2는 512x512 지원
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
                    Debug.Log($"Image {i + 1} URL: {imageUrl}");
                    yield return StartCoroutine(DownloadAndApplyTexture(imageUrl, i));
                }
            }
            else
            {
                Debug.LogError($"DALL-E 2 Error: {request.responseCode} - {request.error} - {request.downloadHandler.text}");
            }
        }
    }

    IEnumerator DownloadAndApplyTexture(string url, int index)
    {
        UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url);
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
            GameObject targetObject = GameObject.Find(GetObjectNameForIndex(index));
            if (targetObject != null)
            {
                targetObject.GetComponent<Renderer>().material.mainTexture = texture;
            }
        }
        else
        {
            Debug.LogError("Image download failed: " + uwr.error);
        }
    }

    string GetObjectNameForIndex(int index)
    {
        switch (index)
        {
            case 0: return "Ceiling";
            case 1: return "FrontWall";
            case 2: return "BackWall";
            case 3: return "LeftWall";
            case 4: return "RightWall";
            default: return "Unknown";
        }
    }
}
// using UnityEngine;
// // using UnityEngine.Networking;
// using System.Collections.Generic;
// // using System.Text;
// // using Newtonsoft.Json.Linq;
// // using Newtonsoft.Json;

// public class DalleEImageGenerator : MonoBehaviour
// {
//     public List<GameObject> Walls;
//     public void SetWallsToRed()
//     {
//         // // Wall 이름 배열
//         // string[] wallNames = { "FloorWall", "Ceiling", "FrontWall", "BackWall", "LeftWall", "RightWall" };
        
//         // // 모든 Wall 오브젝트를 찾아서 Material 색상을 빨간색으로 변경
//         // foreach (string wallName in wallNames)
//         // {

//         //     if (wallObject != null)
//         //     {
//         //         //Renderer mat = wallObject.GetComponent<Material>();
//         //         if (GetComponent<Renderer>() != null)
//         //         {
//         //             //mat.material.color = Color.red;
//         //         }
//         //     }
//         //     else
//         //     {
//         //         Debug.LogWarning($"{wallName} 오브젝트를 찾을 수 없습니다.");
//         //     }
//         // }

//         for (int i = 0; i < Walls.Count; i++)
//         {
//             var WallRenderer = Walls[i].GetComponent<Renderer>();
//             WallRenderer.material.color = Color.red;
//         }
//     }
// }
 