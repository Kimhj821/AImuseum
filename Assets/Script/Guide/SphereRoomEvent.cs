using UnityEngine;
using System.IO;
public class SphereRoomEvent : MonoBehaviour
{
    public ExhibitDescriptionUI descriptionUI; // Inspector에서 할당
    
    public int num = 0;

    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.tag == "Player")
        {
            if(num == 1)
            {
                PlaySphereGuideScene("GuideScene4.json","GuideScene4_v.mp3");
                num -= 1;
            }
        }
    }
    void PlaySphereGuideScene(string jsonFile, string mp3File)
    {
        // GuideFile 폴더를 경로에 추가
        string jsonPath = Path.Combine(Application.streamingAssetsPath, "GuideFile", jsonFile);
        string mp3Path = Path.Combine(Application.streamingAssetsPath, "GuideFile", mp3File);

        if (descriptionUI != null)
        {
            descriptionUI.ShowExhibitDescription(jsonPath);
            descriptionUI.PlayExhibitAudio(mp3Path);
        }
    }
}
