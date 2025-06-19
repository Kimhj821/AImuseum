using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.Video;


public class InputValue : MonoBehaviour
{
    public InputActionAsset inputAsset;
    public Animator lAnim;
    public Animator rAnim;

    public XRRayInteractor leftRay;
    public XRRayInteractor rightRay;

    private InputAction leftGrip, leftTrigger, rightGrip, rightTrigger;
    private InputAction leftSelectAction, rightSelectAction;
    


    [Range(0f, 1f)] public float fistThreshold = 0.1f;

    void Start()
    {
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
    }

    void Update()
    {
        // 왼손 애니메이션
        float leftGripValue = leftGrip.ReadValue<float>();
        float leftTriggerValue = leftTrigger.ReadValue<float>();
        lAnim.SetFloat("Grip", leftGripValue);
        lAnim.SetFloat("Trigger", leftTriggerValue);
        leftRay.enabled = !(leftTriggerValue > fistThreshold);

        // 오른손 애니메이션
        float rightGripValue = rightGrip.ReadValue<float>();
        float rightTriggerValue = rightTrigger.ReadValue<float>();
        rAnim.SetFloat("RightGrip", rightGripValue);
        rAnim.SetFloat("RightTrigger", rightTriggerValue);
        rightRay.enabled = !(rightTriggerValue > fistThreshold);

        // Hover된 객체
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

        // 색상 강조
        var renderer = targetObj.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            var instanceMat = new Material(renderer.material);
            instanceMat.color = Color.yellow;
            renderer.material = instanceMat;
        }

        // Video 재생
        var videoPlayer = targetObj.GetComponent<VideoPlayer>();
        if (videoPlayer != null && !videoPlayer.isPlaying)
        {
            videoPlayer.Play();
            Debug.Log($"▶️ 비디오 재생됨: {targetObj.name}");
            // WebCamera 활성화
        }

        // RoomTeleport 이동 처리
        var teleport = targetObj.GetComponent<RoomTeleport>();
        if (teleport != null && teleport.linkedRoomInfo != null)
        {
            int roomNum = teleport.linkedRoomInfo.PlayerNum;
            RoomTeleport.CurrentRoomNumber = roomNum;

            Debug.Log($"✅ 현재 방 번호: {roomNum}");

            // 유물 및 조명 처리
            StartCoroutine(EnableRelicsAfterDelay(roomNum, 6f));

            // 위치 이동
            FadeManager.Instance.FadeAndMoveTo(teleport.targetPosition);
        }
    }

    private IEnumerator EnableRelicsAfterDelay(int roomNumber, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        RelicManager.Instance.EnableRelicByRoomNum(roomNumber);
        RelicManager.Instance.EnableLightByRoomNum(roomNumber);
    }
}
