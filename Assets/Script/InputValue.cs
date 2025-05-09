using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

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


        
    }
}