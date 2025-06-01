using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq;

public class ApiKeyLoader : MonoBehaviour
{
   public static string OpenAiApiKey { get; private set; }
    public static string ElevenApiKey { get; private set; }
    public static string ElevenVoiceId { get; private set; }

    void Awake()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath,"API","API.json");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            JObject data = JObject.Parse(json);
            OpenAiApiKey = data["Open AI API"]?.ToString();
            ElevenApiKey = data["Eleven Labs API"]?.ToString();
            ElevenVoiceId = data["Eleven Labs Model ID"]?.ToString();
            Debug.Log("API 키 로드 완료");
        }
        else
        {
            Debug.LogError("API.json 파일을 찾을 수 없습니다.");
        }
    }
}
