using UnityEngine;
using System.IO;

public class DoorManager : MonoBehaviour
{
    public GameObject door2;
    public GameObject door3;
    public Animator door2Anim;
    public Animator door3Anim;

    public ExhibitDescriptionUI descriptionUI; // Inspector에서 할당

 

    void Start()
    {
        door2Anim = door2.GetComponent<Animator>();
        door3Anim = door3.GetComponent<Animator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {

            var inputValue = other.GetComponent<InputValue>();

            // 우선순위: 3 > 2 > 1 > 기본
            if (inputValue.isDoor3Open == true && inputValue.isLastLook == true)
            {
                PlayLobbyGuideScene("GuideScene9_Lobby.json", "GuideScene9_Lobby_v.mp3");
            }
            if (inputValue.isDoor3Open == true && inputValue.isLastLook == false)
            {
                PlayLobbyGuideScene("GuideScene8_Lobby.json", "GuideScene8_Lobby_v.mp3");
                door3Anim.SetBool("room3open", true);
                
            }
            if (inputValue.isDoor2Open == true && inputValue.isDoor3Open == false)
            {
                PlayLobbyGuideScene("GuideScene7_Lobby.json", "GuideScene7_Lobby_v.mp3");
                door2Anim.SetBool("room2open", true);
            }
            
            if(inputValue.isDoor3Open == false && inputValue.isDoor2Open == false)
                PlayLobbyGuideScene("GuideScene1_Lobby.json", "GuideScene1_Lobby_v.mp3");

        }
    }

    void PlayLobbyGuideScene(string jsonFile, string mp3File)
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
