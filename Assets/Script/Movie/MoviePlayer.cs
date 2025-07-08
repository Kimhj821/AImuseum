using UnityEngine;
using UnityEngine.Video;
using System.IO;
using Unity.VisualScripting;
public class MoviePlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    private bool hasPlayedOnce = false;
    double currentTime;
    bool event_Play = false;
    private bool isVideoEnded = false; // 영상 종료 플래그 추가

    public double video_time = 119f;
    public ExhibitDescriptionUI descriptionUI;

    public void LateUpdate()
    {
        currentTime = videoPlayer.time;
        
        // 영상이 끝났는지 체크하고 wav 재생 (한 번만)
        if (videoPlayer.frame > 0 && videoPlayer.frame >= (long)videoPlayer.frameCount - 1 && event_Play == false)
        {
            isVideoEnded = true; // 영상이 끝났음을 표시
            event_Play = true;
            PlayVideoGuideScene("GuideScene6.json","GuideScene6_v.wav");
        }
        else if (videoPlayer.frame < (long)videoPlayer.frameCount - 1)
        {
            event_Play = false;
        }
    }
    public void PlayMovie()
    {
        if (isVideoEnded) // 영상이 끝났으면 아무 동작도 하지 않음
            return;

        Debug.Log("[MoviePlayer] PlayMovie 호출");

        if (!hasPlayedOnce)
        {
            Debug.Log("[MoviePlayer] 최초 자동재생 - 2초 대기 후 Play");
            hasPlayedOnce = true;
            StartCoroutine(AutoPlayCoroutine());
        }
        else
        {
            if (!videoPlayer.isPlaying)
            {
                videoPlayer.Play();
                Debug.Log("[MoviePlayer] 영상 이어서 Play");
            }
            else
            {
                Debug.Log("[MoviePlayer] 영상 이미 재생 중");
            }
        }
    }

    System.Collections.IEnumerator AutoPlayCoroutine()
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("[MoviePlayer] 2초 경과, 영상 Play");
        if (videoPlayer != null)
            videoPlayer.Play();
    }

    public void VideoPause()
    {
        Debug.Log("[MoviePlayer] 영상 Pause");
        if (videoPlayer != null)
            videoPlayer.Pause();
    }

    void PlayVideoGuideScene(string jsonFile, string mp3File)
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
