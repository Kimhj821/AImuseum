using UnityEngine;
using UnityEngine.Video;

public class CameraControll : MonoBehaviour
{
    public GameObject WebCamera; 
    private VideoPlayer videoPlayer;

    public GameObject leftCon;
    public GameObject rightCon;

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    void Update()
    {
        if (videoPlayer != null)
        {
            bool isPlaying = videoPlayer.isPlaying;

            
                if (WebCamera != null)
                {
                    leftCon.SetActive(!isPlaying);
                    rightCon.SetActive(!isPlaying);
                    WebCamera.SetActive(isPlaying);
                }
            
            
        }
        
        
    }
}
