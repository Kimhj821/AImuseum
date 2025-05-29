using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class DalleEImageGenerator : MonoBehaviour
{
    [SerializeField] private string openAiApiKey = "YOUR_API_KEY";// open API 키값을 받는 변수

    void Start()
    {
        string prompt = "A seamless panoramic texture for an Imjin War scene, with vivid historical Korean architecture, dramatic lighting, and high detail";  
        StartCoroutine(GenerateImages(prompt));
    }

    public IEnumerator GenerateImages(string prompt)
    {
        var payload = new
        {
            model = "dall-e-3",  // 또는 "gpt-image-1"
            prompt = prompt,
            n = 6,  // 6장 요청
            size = "512x512"
        };

        string jsonPayload = JsonConvert.SerializeObject(payload);

        UnityWebRequest request = new UnityWebRequest("https://api.openai.com/v1/images/generations", "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonPayload));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", $"Bearer {openAiApiKey}");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            JObject result = JObject.Parse(request.downloadHandler.text);
            JArray images = (JArray)result["data"];

            for (int i = 0; i < images.Count; i++)
            {
                string imageUrl = images[i]["url"]?.ToString();
                Debug.Log($"Image {i+1} URL: {imageUrl}");
                StartCoroutine(DownloadAndApplyTexture(imageUrl, i));
            }
        }
        else
        {
            Debug.LogError($"DALL-E Error: {request.responseCode} - {request.error} - {request.downloadHandler.text}");
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
        switch(index)
        {
            case 0: return "Floor";
            case 1: return "Ceiling";
            case 2: return "FrontWall";
            case 3: return "BackWall";
            case 4: return "LeftWall";
            case 5: return "RightWall";
            default: return "Unknown";
        }
    }
}