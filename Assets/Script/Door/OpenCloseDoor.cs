using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using System.IO;
public class OpenCloseDoor : MonoBehaviour
{
    public GameObject doorCube;
    public GameObject doorCloseCube;
    public Animator animator;


    public ExhibitDescriptionUI descriptionUI; // Inspector에서 할당

    public int num = 0;
    void Start()
    {
        animator = doorCube.GetComponent<Animator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if(num == 1)
                animator.SetBool("curtain_opcl1", true);
                PlayCurtainGuideScene("GuideScene3.json","GuideScene3_v.mp3");
            if(num == 2)
                animator.SetBool("curtain_opcl2", true);
                PlayCurtainGuideScene("GuideScene3.json","GuideScene3_v.mp3");
            if(num == 3)
                animator.SetBool("curtain_opcl3", true);
                PlayCurtainGuideScene("GuideScene3.json","GuideScene3_v.mp3");
            doorCloseCube.SetActive(false);
        }
        else
        {
            if(num == 1)
                animator.SetBool("curtain_opcl1", false);
            if(num == 2)
                animator.SetBool("curtain_opcl2", false);
            if(num == 3)
                animator.SetBool("curtain_opcl3", false);
            doorCloseCube.SetActive(true);
        }
    }

    void PlayCurtainGuideScene(string jsonFile, string mp3File)
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
