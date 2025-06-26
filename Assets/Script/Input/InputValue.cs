using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.Video;//videoPlay
using Unity.XR.CoreUtils; //xr Origin

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
    private ContinuousMoveProvider moveProvider;    // XR Toolkit 3.0.8 정식 타입
    private SnapTurnProvider snapTurnProvider;      // XR Toolkit 3.0.8 정식 타입

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

        if (leftHovered != null && leftSelectAction.WasPressedThisFrame())
            HandleInteraction(leftHovered);

        if (rightHovered != null && rightSelectAction.WasPressedThisFrame())
            HandleInteraction(rightHovered);
    }

    private void HandleInteraction(IXRInteractable interactable)
    {
        GameObject targetObj = interactable.transform.gameObject;
        // (생략: 필요에 따라 구현)
    }

    private void OnTriggerEnter(Collider col)
    {
        GameObject targetObj = col.gameObject;
        var teleport = targetObj.GetComponent<RoomTeleport>();
        if (teleport != null && teleport.linkedRoomInfo != null && teleport.isTeleportDoor == true)
        {
            int roomNum = teleport.linkedRoomInfo.PlayerNum;
            RoomTeleport.CurrentRoomNumber = roomNum;
            Debug.Log($"✅ 현재 방 번호: {roomNum}");

            StartCoroutine(EnableRelicsAfterDelay(roomNum, 6f));

            // 이동/회전 기능 비활성화
            if (moveProvider != null) moveProvider.enabled = false;
            if (snapTurnProvider != null) snapTurnProvider.enabled = false;

            FadeManager.Instance.FadeAndMoveTo(teleport.targetPosition);
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
