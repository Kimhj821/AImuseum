using UnityEngine;

public class NPCUILook : MonoBehaviour
{
    public GameObject NPC_UI;
    public Transform player; // 플레이어 Transform (Inspector에서 할당)
    public float activeDistance = 2.0f; // 활성화 거리 (Inspector에서 조절)

    private bool isActive = false;

    void Update()
    {
        if (player == null || NPC_UI == null) return;
        float dist = Vector3.Distance(player.position, transform.position);
        if (dist <= activeDistance)
        {
            if (!isActive)
            {
                NPC_UI.SetActive(true);
                isActive = true;
            }
        }
        else
        {
            if (isActive || NPC_UI.activeSelf)
            {
                NPC_UI.SetActive(false);
                isActive = false;
            }
        }
    }
}
