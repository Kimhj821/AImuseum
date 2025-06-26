using UnityEngine;
using UnityEngine.Video;

public class Sound_Range : MonoBehaviour
{
    public GameObject objectA;
    public GameObject objectB;
    public float maxDistance = 10f; // 최대 거리 (이 거리 이상에서는 볼륨이 0)
    public float minDistance = 1f;  // 최소 거리 (이 거리 이하에서는 볼륨이 1)

    private VideoPlayer videoPlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer component not found on this GameObject!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (objectA == null || objectB == null || videoPlayer == null) return;

        // 두 오브젝트 간의 거리 계산
        float distance = Vector3.Distance(objectA.transform.position, objectB.transform.position);

        // 거리에 따른 볼륨 계산 (0~1 사이의 값)
        float volume = Mathf.Clamp01(1f - Mathf.InverseLerp(minDistance, maxDistance, distance));
        
        // VideoPlayer의 볼륨 설정
        videoPlayer.SetDirectAudioVolume(0, volume);
    }
}
