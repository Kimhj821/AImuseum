using UnityEngine;
using UnityEngine.Video;

public class MoviePlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public Vector3 viewLockPosition;
    public Vector3 viewLockEuler; // Inspector에서 지정

    private bool hasPlayedOnce = false;

    public void LockAndPlay()
    {   
        Debug.Log("[MoviePlayer] LockAndPlay 호출");
        // 시점 고정
        ViewLock.Instance.LockView(viewLockPosition, Quaternion.Euler(viewLockEuler));

        // 2초 뒤에 자동재생 (딱 한번만)
        if (!hasPlayedOnce)
        {
            Debug.Log("[MoviePlayer] 최초 자동재생 - 2초 대기 후 Play");
            hasPlayedOnce = true;
            StartCoroutine(AutoPlayCoroutine());
        }
        else
        {
           
            // 이미 재생한 적 있다면 바로 이어서 재생
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
        videoPlayer.Play();
    }

    public void VideoPause()
    {
        Debug.Log("[MoviePlayer] 영상 Pause + 시점 해제");
        videoPlayer.Pause();
    }
}
