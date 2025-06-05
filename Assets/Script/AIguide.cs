using UnityEngine;
using UnityEngine.UI;
public class AIguide : MonoBehaviour
{ 
    public Transform targetObject;             // AI 가이드 오브젝트
    public Camera mainCamera;                  // VR 카메라
    public RectTransform uiLabel;              // World Space Canvas UI
    public float forwardOffset = 0.5f;         // 앞쪽 거리
    public float heightOffset = 1.5f;          // 위쪽 높이 (예: 얼굴 앞)
    public float smoothSpeed = 10f;

    private Vector3 currentPos;

    void Start()
    {
        currentPos = uiLabel.position;
    }

    void Update()
    {
        // 가이드 앞에 배치할 위치 계산 (로컬 z방향 기준)
        Vector3 offsetPos = targetObject.position 
                          + targetObject.forward * forwardOffset 
                          + Vector3.up * heightOffset;

        // UI를 부드럽게 이동
        currentPos = Vector3.Lerp(currentPos, offsetPos, Time.deltaTime * smoothSpeed);
        uiLabel.position = currentPos;

        // 카메라를 바라보게 회전 (UI가 사용자 쪽을 향함)
        uiLabel.rotation = Quaternion.LookRotation(uiLabel.position - mainCamera.transform.position);

        uiLabel.gameObject.SetActive(true);
    }

}
