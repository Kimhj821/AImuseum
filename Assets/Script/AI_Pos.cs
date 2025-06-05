using UnityEngine;

public class AI_Pos : MonoBehaviour
{
    public GameObject targetA;             // 가이드 로봇
    public RectTransform targetB;          // UI (World Space Canvas)
    public Camera mainCamera;              // 카메라

    public float yOffset = 1.5f;           // UI Y 오프셋
    public float moveSmoothSpeed = 5f;     // UI 이동 보간 속도
    public float rotateSmoothSpeed = 5f;   // UI 회전 보간 속도
    public float maxDistance = 10f;        // 허용 거리 초과 시 조치

    private Vector3 targetPos;
    private Quaternion targetRot;

    void Update()
    {
        if (targetA != null && targetB != null && mainCamera != null)
        {
            float distance = Vector3.Distance(mainCamera.transform.position, targetA.transform.position);

            // 가이드 로봇이 멀어졌는지 확인
            if (distance > maxDistance)
            {
                // 로봇이 카메라 시야에 안 보이는 경우
                Vector3 viewPos = mainCamera.WorldToViewportPoint(targetA.transform.position);
                bool isInView = viewPos.z > 0 && viewPos.x > 0 && viewPos.x < 1 && viewPos.y > 0 && viewPos.y < 1;

                if (!isInView)
                {
                    // 카메라 기준 1m 거리 내에서 랜덤 위치로 텔레포트
                    Vector3 randomOffset = Random.onUnitSphere; // 구 표면상 랜덤 방향
                    randomOffset.y = Mathf.Abs(randomOffset.y); // 위쪽만 사용
                    Vector3 newPos = mainCamera.transform.position + randomOffset.normalized * 1.0f;

                    targetA.transform.position = newPos;
                }
            }

            // UI 위치 계산 (로봇 위치 + Y 오프셋)
            targetPos = targetA.transform.position + Vector3.up * yOffset;
            targetB.position = Vector3.Lerp(targetB.position, targetPos, Time.deltaTime * moveSmoothSpeed);

            // UI 회전 (카메라 바라보게)
            Vector3 dirToCamera = mainCamera.transform.position - targetB.position;
            if (dirToCamera != Vector3.zero)
            {
                targetRot = Quaternion.LookRotation(-dirToCamera.normalized);
                targetB.rotation = Quaternion.Slerp(targetB.rotation, targetRot, Time.deltaTime * rotateSmoothSpeed);
            }
        }
    }
}
