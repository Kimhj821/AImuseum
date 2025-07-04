using UnityEngine;
using UnityEngine.UI;

public class NpcTransparencyURP : MonoBehaviour
{
    [Header("대상 설정")]
    public Transform playerCamera;
    public Renderer npcRenderer;
    public CanvasGroup npcUI;

    [Header("거리 기준")]
    public float minDistance = 1.0f; // 가까울 때 투명
    public float maxDistance = 5.0f; // 멀 때 불투명

    private Material npcMaterial;
    private static readonly string baseColorProperty = "_BaseColor";

    void Start()
    {
        if (npcRenderer != null)
        {
            // 인스턴스화 (다른 NPC와 머티리얼 공유 시 변경 방지)
            npcMaterial = npcRenderer.material;
        }
    }

    void Update()
    {
        if (playerCamera == null || npcMaterial == null) return;

        float distance = Vector3.Distance(transform.position, playerCamera.position);
        float alpha = Mathf.InverseLerp(maxDistance, minDistance, distance); // 가까울수록 alpha↓
        alpha = Mathf.Clamp01(alpha);

        // URP의 _BaseColor를 수정
        Color baseColor = npcMaterial.GetColor(baseColorProperty);
        baseColor.a = alpha;
        npcMaterial.SetColor(baseColorProperty, baseColor);

        // UI 투명도도 함께 조절
        if (npcUI != null)
        {
            npcUI.alpha = alpha;
        }
    }
}
