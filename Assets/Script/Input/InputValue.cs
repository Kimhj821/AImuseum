// 필요한 네임스페이스들
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
using System.IO;

public class InputValue : MonoBehaviour
{
    // --- 입력 관련 ---
    public InputActionAsset inputAsset; // 입력 액션 에셋
    public Animator lAnim; // 왼손 애니메이터
    public Animator rAnim; // 오른손 애니메이터

    public XRRayInteractor leftRay; // 왼손 레이
    public XRRayInteractor rightRay; // 오른손 레이

    private InputAction leftGrip, leftTrigger, rightGrip, rightTrigger;
    private InputAction leftSelectAction, rightSelectAction;

    // XR 관련
    private XROrigin xrOrigin;
    private ContinuousMoveProvider moveProvider;
    private SnapTurnProvider snapTurnProvider;

    [Range(0f, 1f)] public float fistThreshold = 0.1f; // 주먹 감지 기준값

    // 상태 플래그
    public bool isDoor2Open = false;
    public bool isDoor3Open = false;
    public bool isLastLook = false;

    public Robot_Control robotControl; // 로봇 컨트롤러 참조

    // --- 이벤트 추적 ---
    private Dictionary<int, bool> eventStates = new Dictionary<int, bool>(); // 이벤트 실행 여부 저장

    [Header("전시품 본 갯수")]
    private int exhibit1Count = 0;
    private int exhibit2Count = 0;
    private int exhibit3Count = 0;

    public int totalEventCount = 0; // 전체 이벤트 완료 개수

    [Header("모든 전시품 확인")]
    public bool isExhibit1Complete = false;
    public bool isExhibit2Complete = false;
    public bool isExhibit3Complete = false;
    public bool isAllExhibitsComplete = false;
    
    [Header("모든 이벤트 확인")]

    public float last_event1_count = 5f;
    public float last_event2_count = 5f;
    public float last_event3_count = 5f;
    
    private const int exhibit1Total = 6;
    private const int exhibit2Total = 8;
    private const int exhibit3Total = 7;

    public ExhibitDescriptionUI descriptionUI; // Inspector에서 할당

    void Start()
    {
        // XR 시스템 초기화
        xrOrigin = Object.FindFirstObjectByType<XROrigin>();
        if (xrOrigin != null)
        {
            moveProvider = xrOrigin.GetComponent<ContinuousMoveProvider>();
            snapTurnProvider = xrOrigin.GetComponent<SnapTurnProvider>();
        }

        // 입력 액션 매핑
        var leftMap = inputAsset.FindActionMap("XRI Left Interaction");
        leftGrip = leftMap.FindAction("Select Value");
        leftTrigger = leftMap.FindAction("Activate Value");
        leftSelectAction = leftMap.FindAction("Select");

        var rightMap = inputAsset.FindActionMap("XRI Right Interaction");
        rightGrip = rightMap.FindAction("Select Value");
        rightTrigger = rightMap.FindAction("Activate Value");
        rightSelectAction = rightMap.FindAction("Select");

        // 입력 활성화
        leftGrip.Enable(); leftTrigger.Enable(); leftSelectAction.Enable();
        rightGrip.Enable(); rightTrigger.Enable(); rightSelectAction.Enable();

        // 텔레포트 이벤트 등록
        FadeManager.Instance.OnTeleportCompleted += OnTeleportCompleted;
    }

    void OnDestroy()
    {
        // 텔레포트 이벤트 해제
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.OnTeleportCompleted -= OnTeleportCompleted;
        }
    }

    private void OnTeleportCompleted()
    {
        // 텔레포트 후 이동 활성화
        if (moveProvider != null) moveProvider.enabled = true;
        if (snapTurnProvider != null) snapTurnProvider.enabled = true;
        Debug.Log("✅ 텔레포트 완료! 컨트롤러가 다시 활성화되었습니다.");
    }

    void Update()
    {
        // 입력값 받아와서 애니메이션과 Ray 활성화 설정
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
            HandleInteraction(leftHovered, XRNode.LeftHand);

        if (rightHovered != null && rightSelectAction.WasPressedThisFrame())
            HandleInteraction(rightHovered, XRNode.RightHand);

        if (rightHovered == null && rightSelectAction.WasPressedThisFrame())
        {
            HandleSelection(XRNode.RightHand);
        }

        if(isExhibit1Complete == true && last_event1_count > 0)
        {
            last_event1_count -= Time.deltaTime;
        }
        if(last_event1_count < 0)
        {
            last_event1_count = 0;
            PlayGuideScene2("GuideScene2.json","GuideScene2_v.mp3");
        }

        if(isExhibit2Complete == true && last_event2_count > 0)
        {
            last_event2_count -= Time.deltaTime;
        }
        if(last_event2_count < 0)
        {
            last_event2_count = 0;
            PlayGuideScene2("GuideScene2.json","GuideScene2_v.mp3");
        }

        if(isExhibit3Complete == true && last_event3_count > 0)
        {
            last_event3_count -= Time.deltaTime;
        }
        if(last_event3_count < 0)
        {
            last_event3_count = 0;
            PlayGuideScene2("GuideScene2.json","GuideScene2_v.mp3");
        }
    }
    // 실제 상호작용 처리 함수
    private void HandleInteraction(IXRInteractable interactable, XRNode hand)
    {
        var obj = interactable.transform.gameObject;
        var seatLock = obj.GetComponent<SeatLockPoint>();

        // 오른손 좌석 고정
        if (seatLock != null && hand == XRNode.RightHand)
        {
            ViewLock.Instance.LockView(seatLock.lockPosition, Quaternion.Euler(seatLock.lockEuler));
            Debug.Log($"[좌석 고정] {obj.name} 에 XR Origin 고정");
        }

        // 이벤트 처리
        var ray = (hand == XRNode.LeftHand) ? leftRay : rightRay;
        var hovered = ray.GetOldestInteractableHovered();

        if (hovered != null)
        {
            var relic = hovered.transform.GetComponent<RelicAndPictureNum>();
            if (relic != null)
            {
                int eventId = relic.eventId;

                // 이벤트 최초 실행 체크
                if (!eventStates.ContainsKey(eventId) || eventStates[eventId] == false)
                {
                    eventStates[eventId] = true;
                    totalEventCount++;
                    
                    if (eventId >= 1001 && eventId <= 1006)
                    {
                        exhibit1Count++;
                        if (exhibit1Count == exhibit1Total) {
                            isExhibit1Complete = true;
                        }
                    }
                    else if (eventId >= 2001 && eventId <= 2008)
                    {
                        exhibit2Count++;
                        if (exhibit2Count == exhibit2Total) isExhibit2Complete = true;
                    }
                    else if (eventId >= 3001 && eventId <= 3007)
                    {
                        exhibit3Count++;
                        if (exhibit3Count == exhibit3Total) isExhibit3Complete = true;
                    }

                    // 전체 완료 체크
                    if (isExhibit1Complete && isExhibit2Complete && isExhibit3Complete)
                    {
                        isAllExhibitsComplete = true;
                        Debug.Log("🎉 모든 전시관 이벤트 완료!");
                    }
                }

                // 이벤트 실행
                MuseumEventManagement.Instance.OnEventTriggered(eventId);

                if (robotControl != null)
                {
                    robotControl.StartExplainMode(eventId);
                }
            }
        }
    }

    private void HandleSelection(XRNode hand)
    {
        // 오른손에서 아무것도 선택되지 않았을 때 처리 가능
    }

    private void OnTriggerEnter(Collider col)
    {
        // 방 이동 처리
        GameObject targetObj = col.gameObject;
        var teleport = targetObj.GetComponent<RoomTeleport>();
        if (teleport != null && teleport.linkedRoomInfo != null)
        {
            int roomNum = teleport.linkedRoomInfo.PlayerNum;
            Debug.Log($"[DEBUG] isExhibit1Complete={isExhibit1Complete}, CurrentRoomNumber={RoomTeleport.CurrentRoomNumber}, roomNum={roomNum}");
           

            if (teleport.isTeleportDoor && !teleport.fastTeleport)
            {
                RoomTeleport.CurrentRoomNumber = roomNum;
                if (roomNum == 1) isDoor2Open = true;
                if (roomNum == 2) isDoor3Open = true;
                if (roomNum == 3) isLastLook = true;
                

                if (moveProvider != null) moveProvider.enabled = false;
                if (snapTurnProvider != null) snapTurnProvider.enabled = false;
                FadeManager.Instance.FadeAndMoveTo(teleport.targetPosition, teleport.targetRotationEuler.y);
            }
            else if (teleport.isTeleportDoor && teleport.fastTeleport)
            {
                if (xrOrigin != null)
                {
                    xrOrigin.MoveCameraToWorldLocation(teleport.targetPosition);
                    xrOrigin.transform.rotation = Quaternion.Euler(0, teleport.targetRotationEuler.y, 0);
                    if (teleport.ExitTel != null) teleport.ExitTel.SetActive(true);
                }
            }
            else if (!teleport.isTeleportDoor)
            {
                teleport.SetRoomNumberOnly();
            }
        }

        // 영상 자동 재생
        if (col.CompareTag("Video_Play"))
        {
            var moviePlayer = col.GetComponent<MoviePlayer>();
            if (moviePlayer == null)
                moviePlayer = col.GetComponentInChildren<MoviePlayer>();

            if (moviePlayer != null)
                moviePlayer.PlayMovie();
            else
                Debug.LogWarning("MoviePlayer를 찾지 못했습니다. (트리거나 자식에 없음)");
        }
    }

    private void OnTriggerExit(Collider col)
    {
        // 영상 자동 정지
        if (col.CompareTag("Video_Play"))
        {
            var moviePlayer = col.GetComponent<MoviePlayer>();
            if (moviePlayer == null)
                moviePlayer = col.GetComponentInChildren<MoviePlayer>();

            if (moviePlayer != null)
                moviePlayer.VideoPause();
            else
                Debug.LogWarning("MoviePlayer를 찾지 못했습니다. (트리거나 자식에 없음)");
        }
    }

    void PlayGuideScene2(string jsonFile, string mp3File)
    {
        string jsonPath = Path.Combine(Application.streamingAssetsPath, "GuideFile", jsonFile);
        string mp3Path = Path.Combine(Application.streamingAssetsPath, "GuideFile", mp3File);

        if (descriptionUI != null)
        {
            descriptionUI.ShowExhibitDescription(jsonPath);
            descriptionUI.PlayExhibitAudio(mp3Path);
        }
    }
}
