using UnityEngine;

public class RoomTeleport : MonoBehaviour
{
    public Vector3 targetPosition;
    public RoomInfo linkedRoomInfo;
    public static int CurrentRoomNumber = -1; // 현재 플레이어가 있는 방 번호 (전역)
}