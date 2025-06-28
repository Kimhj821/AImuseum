using Unity.VisualScripting;
using UnityEngine;

public class RoomTeleport : MonoBehaviour
{
    public Vector3 targetPosition;
    public Vector3 targetRotationEuler = Vector3.zero;  // << 추가: 텔레포트 후 바라볼 방향(오일러 각)
    public RoomInfo linkedRoomInfo;
    public static int CurrentRoomNumber = -1;
    public bool isTeleportDoor = true;
    public bool fastTeleport = true;

    public GameObject ExitTel;

    public void SetRoomNumberOnly()
    {
        if (linkedRoomInfo != null)
        {
            CurrentRoomNumber = linkedRoomInfo.PlayerNum;
            Debug.Log($"RoomTeleport: PlayerNum 변경됨 = {CurrentRoomNumber}");
        }
    }
}
