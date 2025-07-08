using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ExhibitDescriptionUI : MonoBehaviour
{
    public TMP_Text singleText;
    public AudioSource audioSource;
    public Transform robotTransform;
    public Robot_Control robotControl; // Inspector에서 할당

    private Queue<AudioClip> audioQueue = new Queue<AudioClip>();
    private bool isPlaying = false;

    // ========== 설명 텍스트 출력: Resources에서 TextAsset 로드 ==========
    // jsonResourcePath: "GuideFile/GuideScene1_Lobby" (확장자X)
    public void ShowExhibitDescription(string jsonResourcePath)
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>(jsonResourcePath);
        if (jsonAsset == null)
        {
            singleText.text = "설명 파일이 없습니다.";
            return;
        }
        ShowExhibitDescriptionFromText(jsonAsset.text);
    }

    // ========== 설명 텍스트 출력: 문자열 직접 전달 ==========
    public void ShowExhibitDescriptionFromText(string jsonString)
    {
        Dictionary<string, string> data = null;
        try
        {
            data = MiniJsonParser.Parse(jsonString);
        }
        catch
        {
            singleText.text = "설명을 불러올 수 없습니다.";
            return;
        }

        if (data == null || data.Count == 0)
        {
            singleText.text = "설명을 찾을 수 없습니다.";
            return;
        }
        // 첫 번째 value만 출력
        singleText.text = data.Values.First();
    }

    // ========== 오디오 재생(Queue 구조) ==========
    // clipName: "GuideFile/GuideScene1_Lobby_v" (확장자X)
    public void PlayExhibitAudio(string clipName, bool forceSkip = false)
    {
        AudioClip clip = Resources.Load<AudioClip>(clipName);
        if (clip == null)
        {
            Debug.LogWarning($"[설명UI] AudioClip 로드 실패: {clipName}");
            return;
        }

        if (forceSkip)
        {
            audioSource.Stop();
            audioQueue.Clear();
            isPlaying = false;
        }

        audioQueue.Enqueue(clip);
        TryPlayNext();
    }

    private void TryPlayNext()
    {
        if (isPlaying || audioQueue.Count == 0) return;

        AudioClip nextClip = audioQueue.Dequeue();
        audioSource.clip = nextClip;
        audioSource.spatialBlend = 1.0f; // 3D 오디오 (필요에 따라 0f로 변경)
        audioSource.Play();
        isPlaying = true;
        StartCoroutine(WaitForAudioEnd(nextClip.length));
    }

    private System.Collections.IEnumerator WaitForAudioEnd(float wait)
    {
        yield return new WaitForSeconds(wait + 0.1f);
        isPlaying = false;

        // 오디오가 끝나면 설명모드 종료
        if (robotControl != null)
            robotControl.EndExplainMode();

        TryPlayNext(); // 다음 오디오가 있으면 자동 재생
    }

    public void ClearDescription()
    {
        if (singleText != null)
            singleText.text = "";
        audioSource.Stop();
        audioQueue.Clear();
        isPlaying = false;
    }
}

// 아주 간단한 JSON 파서 (쉼표, 쌍따옴표, 콜론만 단순 처리, 한 쌍만 지원)
public static class MiniJsonParser
{
    public static Dictionary<string, string> Parse(string json)
    {
        var dict = new Dictionary<string, string>();
        json = json.Trim().TrimStart('{').TrimEnd('}');

        var kv = json.Split(new[] { ':' }, 2);
        if (kv.Length == 2)
        {
            string key = kv[0].Trim().Trim('"');
            string value = kv[1].Trim().Trim('"');
            if (value.EndsWith(",")) value = value.Substring(0, value.Length - 1);
            dict[key] = value;
        }
        return dict;
    }
}
