using UnityEngine;
using UnityEngine.UI;
public class AIguide : MonoBehaviour
{ 
    public Transform targetObject;
    public Camera mainCamera;
    public RectTransform uiLabel;
    public Canvas canvas;
    public float scaleMultiplier = 1.0f;
    public float smoothSpeed = 10f;
    public Vector2 offset = Vector2.zero;  // Inspector에서 설정 가능한 기본 offset

    private Vector2 currentPos;

    void Start()
    {
        currentPos = uiLabel.anchoredPosition;
    }

    void Update()
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(targetObject.position);

        if (screenPos.z > 0)
        {
            // 거리 계산
            float distance = Vector3.Distance(mainCamera.transform.position, targetObject.position);
            float scaleFactor = 1 / distance;

            // UI 스케일 (거리 보정)
            uiLabel.localScale = Vector3.one * scaleFactor * scaleMultiplier;

            // 스크린 좌표 → Canvas 좌표 변환
            Vector2 targetPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPos,
                canvas.worldCamera,
                out targetPos
            );

            // offset에 distance 보정 적용
            Vector2 adjustedOffset = offset * scaleFactor * scaleMultiplier;  // scaleMultiplier는 추가 보정
            targetPos += adjustedOffset;

            // 위치 부드럽게 보간
            currentPos = Vector2.Lerp(currentPos, targetPos, Time.deltaTime * smoothSpeed);
            uiLabel.anchoredPosition = currentPos;

            uiLabel.gameObject.SetActive(true);
        }
        else
        {
            uiLabel.gameObject.SetActive(false);
        }
    }

}
