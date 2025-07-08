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
            if (num == 1)
            {
                animator.SetBool("curtain_opcl1", true);
                PlayCurtainGuideScene("GuideScene3.json", "GuideFile/GuideScene3_v");
            }
            if (num == 2)
            {
                animator.SetBool("curtain_opcl2", true);
                PlayCurtainGuideScene("GuideScene3.json", "GuideFile/GuideScene3_v");
            }
            if (num == 3)
            {
                animator.SetBool("curtain_opcl3", true);
                PlayCurtainGuideScene("GuideScene3.json", "GuideFile/GuideScene3_v");
            }
            doorCloseCube.SetActive(false);
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            if (num == 1)
                animator.SetBool("curtain_opcl1", false);
            if (num == 2)
                animator.SetBool("curtain_opcl2", false);
            if (num == 3)
                animator.SetBool("curtain_opcl3", false);
        }
    }

    void PlayCurtainGuideScene(string jsonFile, string clipName)
    {
        string jsonPath = Path.Combine(Application.dataPath, "Audio", "GuideFile", jsonFile);

        if (descriptionUI != null)
        {
            descriptionUI.ShowExhibitDescription(jsonPath);
            // 오디오: clipName만 넘김 (확장자X, Resources/GuideFile/GuideScene3_v.wav 구조)
            descriptionUI.PlayExhibitAudio(clipName);
        }
    }
}
