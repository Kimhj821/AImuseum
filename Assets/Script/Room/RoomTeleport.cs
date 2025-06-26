using UnityEngine;

public class RoomTeleport : MonoBehaviour
{
    public Vector3 targetPosition;
    public RoomInfo linkedRoomInfo;
    public static int CurrentRoomNumber = -1; // 현재 플레이어가 있는 방 번호 (전역)
    public bool isTeleportDoor = true; // (추가) 텔레포트 기능 ON/OFF

    // 이 함수는 텔레포트 여부와 상관 없이 방 번호만 바꿔줌
    public void SetRoomNumberOnly()
    {
        if (linkedRoomInfo != null)
        {
            CurrentRoomNumber = linkedRoomInfo.PlayerNum;
            Debug.Log($"RoomTeleport: PlayerNum 변경됨 = {CurrentRoomNumber}");
        }
    }
}