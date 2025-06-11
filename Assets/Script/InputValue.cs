using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class InputValue : MonoBehaviour
{
    public InputActionAsset inputAsset;
    public Animator lAnim;
    public Animator rAnim;

    public XRRayInteractor leftRay;
    public XRRayInteractor rightRay;

    // 실행 시 한 번만 찾아오고 계속 재사용
    private InputAction leftGrip;
    private InputAction leftTrigger;
    private InputAction rightGrip;
    private InputAction rightTrigger;

    private InputAction leftSelectAction;
    private InputAction rightSelectAction;

    [Range(0f, 1f)] public float fistThreshold = 0.1f;

    void Start()
    {
        // 기존 인덱스 2: XRI LeftHand Interaction
        var leftMap = inputAsset.FindActionMap("XRI Left Interaction");
        leftGrip = leftMap.FindAction("Select Value");
        leftTrigger = leftMap.FindAction("Activate Value");

        // 기존 인덱스 5: XRI RightHand Interaction
        var rightMap = inputAsset.FindActionMap("XRI Right Interaction");
        rightGrip = rightMap.FindAction("Select Value");
        rightTrigger = rightMap.FindAction("Activate Value");

        // 활성화
        leftGrip.Enable();
        leftTrigger.Enable();
        rightGrip.Enable();
        rightTrigger.Enable();

        //select 버튼 찾기
        leftSelectAction = inputAsset.FindActionMap("XRI Left Interaction").FindAction("Select");
        rightSelectAction = inputAsset.FindActionMap("XRI Right Interaction").FindAction("Select");

        //select 활성화
        leftSelectAction.Enable();
        rightSelectAction.Enable();

        
    }

    void Update()
    {
        // 왼손 애니메이션 값 설정
        float leftGripValue = leftGrip.ReadValue<float>();
        float leftTriggerValue = leftTrigger.ReadValue<float>();
        lAnim.SetFloat("Grip", leftGripValue);
        lAnim.SetFloat("Trigger", leftTriggerValue);

        bool isLeftFist = leftTriggerValue > fistThreshold;
        if (leftRay != null) leftRay.enabled = !isLeftFist;


        // 오른손 애니메이션 값 설정
        float rightGripValue = rightGrip.ReadValue<float>();
        float rightTriggerValue = rightTrigger.ReadValue<float>();
        rAnim.SetFloat("RightGrip", rightGripValue);
        rAnim.SetFloat("RightTrigger", rightTriggerValue);

        bool isRightFist = rightTriggerValue > fistThreshold;
        if (rightRay != null) rightRay.enabled = !isRightFist;

        // 1. 현재 Hover 중인 Interactable 확인
        var leftHoveredObject = leftRay.GetOldestInteractableHovered(); // 왼손 기준
        var rightHoveredObject = rightRay.GetOldestInteractableHovered(); //오른손 기준

        // 2. X버튼(A버튼) 눌렀는지 확인
        bool isLeftSelected = leftSelectAction.WasPressedThisFrame();
        bool isRightSelected = rightSelectAction.WasPressedThisFrame();

        // 3. 조건 충족 시 해당 오브젝트 위치로 이동
        //왼손
        if (leftHoveredObject != null && isLeftSelected)
        {
            var renderer = leftHoveredObject.transform.GetComponent<MeshRenderer>();

            if (renderer != null)
            {
                Material instanceMat = new Material(renderer.material); // 복제해서 분리
                instanceMat.color = Color.yellow; // Base Map 색상 변경
                renderer.material = instanceMat;
            }

            var teleport = leftHoveredObject.transform.GetComponent<RoomTeleport>();
            if (teleport != null)
            {
                if (teleport.linkedRoomInfo != null)
                {
                    RoomTeleport.CurrentRoomNumber = teleport.linkedRoomInfo.PlayerNum;
                    Debug.Log($"✅ 플레이어 방 번호 갱신됨: {RoomTeleport.CurrentRoomNumber}번 방으로 이동");

                    // ✅ 해당 방 번호에 맞는 유물만 활성화
                    StartCoroutine(EnableRelicsAfterDelay(RoomTeleport.CurrentRoomNumber, 6f));
                }

                // ✅ 이동 처리
                FadeManager.Instance.FadeAndMoveTo(teleport.targetPosition);
            }
            
        }

        //오른손
        if (rightHoveredObject != null && isRightSelected)
        {
            // 머티리얼 색상을 노란색으로 변경
            var renderer = rightHoveredObject.transform.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material instanceMat = new Material(renderer.material); // 복제해서 분리
                instanceMat.color = Color.yellow; // Base Map 색상 변경
                renderer.material = instanceMat;
            }

            var teleport = rightHoveredObject.transform.GetComponent<RoomTeleport>();
            if (teleport != null)
            {
                if (teleport.linkedRoomInfo != null)
                {
                    RoomTeleport.CurrentRoomNumber = teleport.linkedRoomInfo.PlayerNum;
                    Debug.Log($"✅ 플레이어 방 번호 갱신됨: {RoomTeleport.CurrentRoomNumber}번 방으로 이동");

                    // ✅ 해당 방 번호에 맞는 유물만 활성화
                    StartCoroutine(EnableRelicsAfterDelay(RoomTeleport.CurrentRoomNumber, 6f));
                }

                // ✅ 이동 처리
                FadeManager.Instance.FadeAndMoveTo(teleport.targetPosition);
            }
            
            
        }
        
        
    }
    private IEnumerator EnableRelicsAfterDelay(int roomNumber, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        RelicManager.Instance.EnableRelicByRoomNum(roomNumber);
        RelicManager.Instance.EnableLightByRoomNum(roomNumber);
    }

}