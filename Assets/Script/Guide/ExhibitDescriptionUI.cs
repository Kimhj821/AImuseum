using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class ExhibitDescriptionUI : MonoBehaviour
{
    public TMP_Text singleText;
    public AudioSource audioSource;
    public Transform robotTransform;
    public Robot_Control robotControl; // Inspector에서 할당

    public void ShowExhibitDescription(string jsonPath)
    {
        if (!File.Exists(jsonPath))
        {
            //Debug.LogWarning($"[설명UI] JSON 파일 없음: {jsonPath}");
            singleText.text = "설명 파일이 없습니다.";
            return;
        }

        string jsonString = File.ReadAllText(jsonPath);

        // Unity 내장 JsonUtility는 사용 불가, Dictionary 파싱 사용
        Dictionary<string, string> data = null;
        try
        {
            data = MiniJsonParser.Parse(jsonString);
        }
        catch
        {
            //Debug.LogWarning("[설명UI] JSON 파싱 실패");
            singleText.text = "설명을 불러올 수 없습니다.";
            return;
        }

        if (data == null || data.Count == 0)
        {
            //Debug.LogWarning("[설명UI] JSON 데이터 없음");
            singleText.text = "설명을 찾을 수 없습니다.";
            return;
        }

        // 첫 번째 value만 출력
        singleText.text = data.Values.First();
    }

    public void PlayExhibitAudio(string mp3Path)
    {
        if (audioSource == null)
        {
            //Debug.LogWarning("[설명UI] AudioSource가 할당되지 않았습니다.");
            return;
        }
        if (!File.Exists(mp3Path))
        {
            //Debug.LogWarning($"[설명UI] MP3 파일 없음: {mp3Path}");
            return;
        }
        StartCoroutine(LoadAndPlayAudio(mp3Path));
    }

    private System.Collections.IEnumerator LoadAndPlayAudio(string mp3Path)
    {
        string url = "file://" + mp3Path;
        using (var www = new WWW(url))
        {
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                //Debug.LogWarning($"[설명UI] 오디오 로드 실패: {www.error}");
                yield break;
            }
            audioSource.clip = www.GetAudioClip(false, false);
            audioSource.spatialBlend = 1.0f;
            audioSource.Play();
            // 오디오가 끝날 때까지 대기 후 설명모드 종료
            yield return WaitForAudioEnd();
        }
    }

    private System.Collections.IEnumerator WaitForAudioEnd()
    {
        if (audioSource == null) yield break;
        while (audioSource.isPlaying)
            yield return null;
        // 오디오가 끝나면 설명모드 종료
        if (robotControl != null)
            robotControl.EndExplainMode();
    }

    // 설명 텍스트를 지우는 메서드
    public void ClearDescription()
    {
        if (singleText != null)
            singleText.text = "";
    }
}

// 아주 간단한 JSON 파서 (쉼표, 쌍따옴표, 콜론만 단순 처리, 한 쌍만 지원)
public static class MiniJsonParser
{
    public static Dictionary<string, string> Parse(string json)
    {
        var dict = new Dictionary<string, string>();
        json = json.Trim().TrimStart('{').TrimEnd('}');
        // 한 쌍만 있다고 가정
        var kv = json.Split(new[] { ':' }, 2);
        if (kv.Length == 2)
        {
            string key = kv[0].Trim().Trim('"');
            string value = kv[1].Trim().Trim('"');
            // value 끝에 쉼표가 있으면 제거
            if (value.EndsWith(",")) value = value.Substring(0, value.Length - 1);
            dict[key] = value;
        }
        return dict;
    }
}
