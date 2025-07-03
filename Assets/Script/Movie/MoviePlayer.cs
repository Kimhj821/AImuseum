using UnityEngine;
using UnityEngine.Video;

public class MoviePlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    private bool hasPlayedOnce = false;

    public void PlayMovie()
    {
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
}
