using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;

public class Robot_Control : MonoBehaviour
{
    public Transform player;
    public List<Transform> targets;
    public float stopDistance = 2.0f;
    public float rotationSpeed = 5.0f;
    public float startupDelay = 1.0f;
    public float minFollowDistance = 1.5f;

    public Camera followCamera;

    private NavMeshAgent agent;
    private Animator animator;
    private bool isStartupComplete = false;

    public enum RobotMode { Follow, Explain }
    private RobotMode currentMode = RobotMode.Follow;

    private bool isExplaining = false;
    private int currentEventId = -1;
    private Transform explainTarget;

    public ExhibitDescriptionUI descriptionUI;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.stoppingDistance = stopDistance;
        StartCoroutine(StartupDelayRoutine());
    }

    IEnumerator StartupDelayRoutine()
    {
        SetIdle();
        yield return new WaitForSeconds(startupDelay);
        isStartupComplete = true;
    }

    void Update()
    {
        if (!isStartupComplete || player == null || followCamera == null)
            return;

        bool isVisible = IsVisibleToCamera();
        switch (currentMode)
        {
            case RobotMode.Follow:
                if (!isVisible) FollowPlayer();
                else StopAndIdle();
                break;
            case RobotMode.Explain:
                ExplainModeLogic();
                break;
        }
    }

    // === 플레이어 추적 ===
    void FollowPlayer()
    {
        if (agent == null) return;

        agent.isStopped = false;
        Vector3 playerBackward = -player.forward.normalized; // 플레이어가 보는 방향의 반대
        Vector3 targetPos = player.position + playerBackward * minFollowDistance;
        targetPos.y = player.position.y;

        agent.SetDestination(targetPos);
        SetWalk();

        if (!agent.pathPending && agent.remainingDistance <= stopDistance)
            StopAndIdle();

        if (Vector3.Distance(transform.position, player.position) > 10f)
        {
            agent.Warp(targetPos);
            SetIdle();
        }
    }

    // === 설명모드 진입(가장 가까운 타겟) ===
    public void StartExplainMode(int eventId)
    {
        if (isExplaining) return;
        currentEventId = eventId;
        isExplaining = true;
        currentMode = RobotMode.Explain;
        explainTarget = GetNearestTarget(player.position);
        MoveToTarget(explainTarget);
    }

    // === 설명모드 진입(특정 타겟) ===
    public void MoveToGuideTarget(Transform target)
    {
        if (target == null) return;
        currentMode = RobotMode.Explain;
        isExplaining = true;
        currentEventId = -1;
        explainTarget = target;
        MoveToTarget(target);
    }

    // === 설명모드 로직 ===
    void ExplainModeLogic()
    {
        if (explainTarget == null || agent == null) return;

        if (!agent.pathPending && agent.remainingDistance <= stopDistance)
        {
            StopAndIdle();
            LookAtPlayer();

            if (isExplaining)
            {
                isExplaining = false;
                Debug.Log($"[로봇] 이벤트ID {currentEventId} 설명 시작");
                // 실제 설명 시작(오디오, 텍스트 등)
                // 설명 종료시 EndExplainMode() 호출 필요
            }
        }
    }

    // === 설명모드 종료 ===
    public void EndExplainMode()
    {
        currentMode = RobotMode.Follow;
        ResetExplainState();
        if (descriptionUI != null)
            descriptionUI.ClearDescription();
    }

    // === 타겟 이동 및 애니메이션 ===
    void MoveToTarget(Transform target)
    {
        if (target == null || agent == null) return;
        agent.isStopped = false;
        agent.SetDestination(target.position);
        SetWalk();
    }

    // === 가장 가까운 타겟 찾기 ===
    Transform GetNearestTarget(Vector3 pos)
    {
        Transform nearest = null;
        float minDist = float.MaxValue;
        foreach (var t in targets)
        {
            float dist = Vector3.Distance(pos, t.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = t;
            }
        }
        return nearest;
    }

    // === 카메라에서 보이는지 체크 ===
    bool IsVisibleToCamera()
    {
        Vector3 viewportPos = followCamera.WorldToViewportPoint(transform.position);
        return viewportPos.z > 0 && viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1;
    }

    void LookAtPlayer()
    {
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
        }
    }

    void SetWalk()
    {
        if (animator == null) return;
        animator.SetBool("Open_Anim", true);
        animator.SetBool("Walk_Anim", true);
        animator.SetBool("Roll_Anim", false);
    }
    void SetIdle()
    {
        if (animator == null) return;
        animator.SetBool("Open_Anim", true);
        animator.SetBool("Walk_Anim", false);
        animator.SetBool("Roll_Anim", false);
    }
    void StopAndIdle()
    {
        if (agent != null) agent.isStopped = true;
        SetIdle();
    }

    void ResetExplainState()
    {
        currentEventId = -1;
        explainTarget = null;
        isExplaining = false;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 필요시 구현
        }
    }
}
