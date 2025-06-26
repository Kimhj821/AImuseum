using UnityEngine;

public class PortalViewSync : MonoBehaviour
{
    [Header("필수 연결")]
    public Transform playerCamera;
    public Transform[] portalInList;    // 여러 개의 포탈 입구 쿼드
    public Transform portalOut;
    public Camera portalCamera;

    [Header("포탈 카메라 이동/FOV 제한")]
    public float maxPortalViewDistance = 3.0f;
    public float minFOV = 40f;
    public float maxFOV = 70f;

    [Header("포탈 시야각 제한")]
    [Tooltip("플레이어가 포탈을 바라볼 때만 회전 동기화 (도 단위, 예: 30 = 정면±30도까지 동기화)")]
    public float maxViewAngle = 30f;

    void LateUpdate()
    {
        if (playerCamera == null || portalInList == null || portalInList.Length == 0 || portalOut == null || portalCamera == null)
            return;

        // 1. 플레이어와 가장 가까운 포탈 입구(쿼드) 찾기
        Transform closestPortalIn = null;
        float closestDist = float.MaxValue;

        foreach (var pi in portalInList)
        {
            if (pi == null) continue;
            float d = Vector3.Distance(playerCamera.position, pi.position);
            if (d < closestDist)
            {
                closestDist = d;
                closestPortalIn = pi;
            }
        }

        // 유효한 포탈 입구가 없으면 중단
        if (closestPortalIn == null) return;

        // 기존 portalIn 변수 대신 closestPortalIn을 사용!
        Vector3 playerOffsetFromPortal = closestPortalIn.InverseTransformPoint(playerCamera.position);
        float distance = playerOffsetFromPortal.magnitude;
        Vector3 portalCameraLocalOffset;

        if (distance > maxPortalViewDistance)
        {
            portalCameraLocalOffset = new Vector3(0, 0, -maxPortalViewDistance);
        }
        else
        {
            portalCameraLocalOffset = playerOffsetFromPortal;
        }

        portalCamera.transform.position = portalOut.TransformPoint(portalCameraLocalOffset);

        // 2. 회전 제한
        Quaternion targetRotation = portalOut.rotation;
        Vector3 toPortal = (closestPortalIn.position - playerCamera.position).normalized;
        float viewAngle = Vector3.Angle(playerCamera.forward, toPortal * -1f);

        if (distance <= maxPortalViewDistance && viewAngle <= maxViewAngle)
        {
            Quaternion rotationDifference = portalOut.rotation * Quaternion.Inverse(closestPortalIn.rotation);
            targetRotation = rotationDifference * playerCamera.rotation;
        }

        portalCamera.transform.rotation = targetRotation;

        // 3. FOV(화각) 제한: 멀어질수록 좁아짐(줌인 효과)
        float t = Mathf.Clamp01(distance / maxPortalViewDistance);
        float targetFOV = Mathf.Lerp(maxFOV, minFOV, t);
        portalCamera.fieldOfView = targetFOV;

        // 4. Aspect(화면비) 동기화
        Camera mainCam = playerCamera.GetComponent<Camera>();
        if (mainCam != null)
        {
            portalCamera.aspect = mainCam.aspect;
        }
    }
}
