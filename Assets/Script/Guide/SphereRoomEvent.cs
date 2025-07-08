using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Collections;

public class SphereRoomEvent : MonoBehaviour
{
    public ExhibitDescriptionUI descriptionUI; // Inspector에서 할당
    
    public int num = 0;

    void OnTriggerStay(Collider col)
    {
        if(col.gameObject.tag == "Player")
        {
            if(num == 1)
            {
                StartCoroutine(DelayedPlaySphereGuideScene("GuideScene4.json", "GuideScene4_v.wav"));
                num = 0;
            }
            if(num == 2)
            {
                StartCoroutine(DelayedPlaySphereGuideScene("GuideScene5.json", "GuideScene5_v.wav"));
                num = 0;
            }
        }
    }


    void PlaySphereGuideScene(string jsonFile, string mp3File)
    {
        // GuideFile 폴더를 경로에 추가
        string jsonPath = Path.Combine(Application.dataPath, "Audio", "GuideFile", jsonFile);
        string mp3Path = Path.Combine(Application.dataPath, "Audio", "GuideFile", mp3File);

        if (descriptionUI != null)
        {
            descriptionUI.ShowExhibitDescription(jsonPath);
            descriptionUI.PlayExhibitAudio(mp3Path);
        }
    }

    System.Collections.IEnumerator DelayedPlaySphereGuideScene(string json, string wav)
    {
        yield return new WaitForSeconds(1f);
        PlaySphereGuideScene(json, wav);
    }
}
