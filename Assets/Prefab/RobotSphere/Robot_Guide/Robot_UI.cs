using UnityEngine;

public class Robot_UI : MonoBehaviour
{
    public Transform robot;
    public Camera mainCamera;
    public RectTransform uiElement;

    [Header("스케일 설정")]
    public float minScale = 0.3f;
    public float maxScale = 1.0f;

    [Header("오프셋 설정")]
    public float baseYOffset = 80f; // 기준 오프셋 (scale=1일 때)

    [Header("자동 거리 범위")]
    public float minDistance = 2f;
    public float maxDistance = 25f;

    public float smoothSpeed = 5f;

    private bool isUIVisible = false;

    void Update()
    {
        UpdateUIPositionAndVisibility();
        UpdateUIScaleAndOffset();
    }

    void UpdateUIPositionAndVisibility()
    {
        Vector3 worldPosition = robot.position;
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(worldPosition);

        isUIVisible = viewportPosition.z > 0 &&
                      viewportPosition.x >= 0 && viewportPosition.x <= 1 &&
                      viewportPosition.y >= 0 && viewportPosition.y <= 1;

        uiElement.gameObject.SetActive(isUIVisible);
    }

    void UpdateUIScaleAndOffset()
    {
        if (!isUIVisible) return;

        float distance = Vector3.Distance(robot.position, mainCamera.transform.position);
        float clampedDistance = Mathf.Clamp(distance, minDistance, maxDistance);
        float t = Mathf.InverseLerp(minDistance, maxDistance, clampedDistance);

        // 1. 스케일 계산 (가까울수록 큼)
        float targetScale = Mathf.Lerp(maxScale, minScale, t);
        float smoothScale = Mathf.Lerp(uiElement.localScale.x, targetScale, Time.deltaTime * smoothSpeed);
        uiElement.localScale = new Vector3(smoothScale, smoothScale, 1f);

        // 2. 스케일 기반 Y 오프셋 보정
        float yOffset = baseYOffset * smoothScale;

        // 3. 화면 좌표 변환 + 보정된 위치 설정
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(robot.position);
        uiElement.position = screenPosition + new Vector3(0, yOffset, 0);
    }
}
