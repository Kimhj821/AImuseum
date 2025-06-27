using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.Video;
using Unity.XR.CoreUtils;
using UnityEngine.XR;

public class InputValue : MonoBehaviour
{
    public InputActionAsset inputAsset;
    public Animator lAnim;
    public Animator rAnim;

    public XRRayInteractor leftRay;
    public XRRayInteractor rightRay;

    private InputAction leftGrip, leftTrigger, rightGrip, rightTrigger;
    private InputAction leftSelectAction, rightSelectAction;

    private XROrigin xrOrigin;
    private ContinuousMoveProvider moveProvider;
    private SnapTurnProvider snapTurnProvider;

    private IXRInteractable currentLockedChair = null;
    private MoviePlayer currentLockedMovie = null;
    [Range(0f, 1f)] public float fistThreshold = 0.1f;

    void Start()
    {
        xrOrigin = Object.FindFirstObjectByType<XROrigin>();
        if (xrOrigin != null)
        {
            moveProvider = xrOrigin.GetComponent<ContinuousMoveProvider>();
            snapTurnProvider = xrOrigin.GetComponent<SnapTurnProvider>();
        }

        var leftMap = inputAsset.FindActionMap("XRI Left Interaction");
        leftGrip = leftMap.FindAction("Select Value");
        leftTrigger = leftMap.FindAction("Activate Value");
        leftSelectAction = leftMap.FindAction("Select");

        var rightMap = inputAsset.FindActionMap("XRI Right Interaction");
        rightGrip = rightMap.FindAction("Select Value");
        rightTrigger = rightMap.FindAction("Activate Value");
        rightSelectAction = rightMap.FindAction("Select");

        leftGrip.Enable(); leftTrigger.Enable(); leftSelectAction.Enable();
        rightGrip.Enable(); rightTrigger.Enable(); rightSelectAction.Enable();

        FadeManager.Instance.OnTeleportCompleted += OnTeleportCompleted;
    }

    void OnDestroy()
    {
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.OnTeleportCompleted -= OnTeleportCompleted;
        }
    }

    private void OnTeleportCompleted()
    {
        if (moveProvider != null) moveProvider.enabled = true;
        if (snapTurnProvider != null) snapTurnProvider.enabled = true;
        Debug.Log("✅ 텔레포트 완료! 컨트롤러가 다시 활성화되었습니다.");
    }

    void Update()
    {
        float leftGripValue = leftGrip.ReadValue<float>();
        float leftTriggerValue = leftTrigger.ReadValue<float>();
        lAnim.SetFloat("Grip", leftGripValue);
        lAnim.SetFloat("Trigger", leftTriggerValue);
        leftRay.enabled = !(leftTriggerValue > fistThreshold);

        float rightGripValue = rightGrip.ReadValue<float>();
        float rightTriggerValue = rightTrigger.ReadValue<float>();
        rAnim.SetFloat("RightGrip", rightGripValue);
        rAnim.SetFloat("RightTrigger", rightTriggerValue);
        rightRay.enabled = !(rightTriggerValue > fistThreshold);

        var leftHovered = leftRay.GetOldestInteractableHovered();
        var rightHovered = rightRay.GetOldestInteractableHovered();

        // 왼손 Select(X버튼) - 필요 시 동작 추가
        if (leftHovered != null && leftSelectAction.WasPressedThisFrame())
            HandleInteraction(leftHovered, XRNode.LeftHand);

        // 오른손 Select(A버튼) 단, hover이 되어있어야 작동
        if (rightHovered != null && rightSelectAction.WasPressedThisFrame())
            HandleInteraction(rightHovered, XRNode.RightHand);

        // 오른손 Select(A버튼)과 hover이 안되 상태로 A버튼 작동동
        if (rightHovered == null && rightSelectAction.WasPressedThisFrame())
        {
            HandleSelection(XRNode.RightHand);
        }
    }


    private void HandleSelection(XRNode hand)
    {
        if (currentLockedChair != null && currentLockedMovie != null)
        {
            Debug.Log("[Update] 오른손 아무것도 hover 안한 채 select: 시점 해제 + 영상 일시정지!");
            currentLockedMovie.VideoPause();
            if (moveProvider != null) moveProvider.enabled = true;
            currentLockedChair = null;
            currentLockedMovie = null;
        }
    }
    private void HandleInteraction(IXRInteractable interactable, XRNode hand)
    {
        GameObject targetObj = interactable.transform.gameObject;
        var chairPlayer = targetObj.GetComponent<MoviePlayer>();

        // MoviePlayer 오브젝트인 경우만 특별 동작
        if (chairPlayer != null)
        {
            if (hand == XRNode.RightHand)
            {
                Debug.Log($"[HandleInteraction] 오른손 select. 현재 고정 의자:{(currentLockedChair != null ? currentLockedChair.transform.name : "없음")}");

                    Debug.Log($"[HandleInteraction] -> 시점고정 + 영상 재생! (의자:{targetObj.name})");
                    chairPlayer.LockAndPlay();
                    if (moveProvider != null) moveProvider.enabled = false;
                    currentLockedChair = interactable;
                    currentLockedMovie = chairPlayer;
                
            }
            else if (hand == XRNode.LeftHand)
            {
                Debug.Log("[HandleInteraction] 왼손 select - 현재 동작 없음");
            }
            return;
        }
        // 여기에 다른 interactable 대응 코드 추가 가능
        Debug.Log("[HandleInteraction] MoviePlayer 아닌 다른 interactable");
    }

    private void OnTriggerEnter(Collider col)
    {
        GameObject targetObj = col.gameObject;
        var teleport = targetObj.GetComponent<RoomTeleport>();
        int roomNum = teleport.linkedRoomInfo.PlayerNum;
        if (teleport != null && teleport.linkedRoomInfo != null && teleport.isTeleportDoor == true && teleport.fastTeleport == false)
        {
            RoomTeleport.CurrentRoomNumber = roomNum;
            Debug.Log($"✅ 현재 방 번호: {roomNum}");
            
            StartCoroutine(EnableRelicsAfterDelay(roomNum, 6f));

            // 이동/회전 기능 비활성화
            if (moveProvider != null) moveProvider.enabled = false;
            if (snapTurnProvider != null) snapTurnProvider.enabled = false;

            FadeManager.Instance.FadeAndMoveTo(teleport.targetPosition, teleport.targetRotationEuler.y);
        }
        
        if (teleport != null && teleport.isTeleportDoor == true && teleport.fastTeleport == true)
        {
            if (xrOrigin != null)
            {
                xrOrigin.MoveCameraToWorldLocation(teleport.targetPosition);
                xrOrigin.transform.rotation = Quaternion.Euler(0, teleport.targetRotationEuler.y, 0);
                Debug.Log($"✅ 현재 방 번호: {roomNum}");
            }
        }

        var roomTeleport = col.gameObject.GetComponent<RoomTeleport>();
        if (roomTeleport != null && roomTeleport.isTeleportDoor == false)
        {
            roomTeleport.SetRoomNumberOnly();
        }
    }

    private IEnumerator EnableRelicsAfterDelay(int roomNumber, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        RelicManager.Instance.EnableRelicByRoomNum(roomNumber);
        RelicManager.Instance.EnableLightByRoomNum(roomNumber);
    }
}
