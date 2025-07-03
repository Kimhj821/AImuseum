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

    // Inspector에서 지정할 카메라
    public Camera followCamera;

    private NavMeshAgent agent;
    private Animator animator;
    private bool isStartupComplete = false;

    // 모드 관리
    public enum RobotMode { Follow, Explain }
    private RobotMode currentMode = RobotMode.Follow;

    // 설명모드 관리
    private bool isExplaining = false;
    private int currentEventId = -1;
    private Transform explainTarget; // 설명할 타겟 위치

    public ExhibitDescriptionUI descriptionUI; // Inspector에서 할당

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stopDistance;
        StartCoroutine(StartupDelayRoutine());
    }

    IEnumerator StartupDelayRoutine()
    {
        Idle_Animation();
        yield return new WaitForSeconds(startupDelay);
        isStartupComplete = true;
    }

    void Update()
    {
        if (!isStartupComplete || player == null) return;

        Camera cam = followCamera;
        if (cam == null) return;

        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);
        bool isVisible =
            viewportPos.z > 0 &&
            viewportPos.x > 0 && viewportPos.x < 1 &&
            viewportPos.y > 0 && viewportPos.y < 1;

        switch (currentMode)
        {
            case RobotMode.Follow:
                if (!isVisible)
                {
                    // 시야 밖이면 따라오고, walk 애니메이션 유지
                    FollowPlayerLogic();
                }
                else
                {
                    // 시야 안이면 이동 중지 및 idle
                    agent.isStopped = true;
                    Idle_Animation();
                }
                break;
            case RobotMode.Explain:
                ExplainModeLogic();
                break;
        }
    }

    // 플레이어 추적 모드 로직
    void FollowPlayerLogic()
    {
        agent.isStopped = false;

        // 플레이어의 뒤쪽을 목적지로 설정
        float behindOffset = 2.0f; // 플레이어 뒤로 2m
        Vector3 targetPos = player.position - player.forward.normalized * behindOffset;
        targetPos.y = player.position.y;

        agent.SetDestination(targetPos);
        Walk_Animation();

        // 목적지 도착하면 idle로 전환
        if (!agent.pathPending && agent.remainingDistance <= stopDistance)
        {
            agent.isStopped = true;
            Idle_Animation();
        }

        // Follow 모드에서 플레이어와 멀어지면 순간이동
        float teleportDistance = 10f; // 순간이동 트리거 거리
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer > teleportDistance)
        {
            // 플레이어가 바라보는 방향 뒤 0.5미터 위치로 텔레포트
            Vector3 spawnPos = player.position - player.forward.normalized * 1f;
            spawnPos.y = player.position.y;
            agent.Warp(spawnPos);
            Idle_Animation();
        }
    }

    // 설명모드 진입 함수 (외부에서 호출)
    public void StartExplainMode(int eventId)
    {
        if (isExplaining) return; // 설명 중에는 무시
        currentEventId = eventId;
        isExplaining = true;
        currentMode = RobotMode.Explain;

        // 플레이어 기준 가장 가까운 타겟 저장
        float minDist = float.MaxValue;
        Transform nearest = null;
        for (int i = 0; i < targets.Count; i++)
        {
            float dist = Vector3.Distance(player.position, targets[i].position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = targets[i];
            }
        }
        explainTarget = nearest;
        if (explainTarget != null)
        {
            agent.SetDestination(explainTarget.position);
            Walk_Animation();
        }
    }

    // 설명모드 로직
    void ExplainModeLogic()
    {
        if (explainTarget == null) return;
        // 타겟 도착 판정
        if (!agent.pathPending && agent.remainingDistance <= stopDistance)
        {
            Idle_Animation();
            LookAtPlayer();
            // 설명 시작(한 번만)
            if (isExplaining)
            {
                isExplaining = false;
                // 설명 텍스트/오디오 재생 등
                Debug.Log($"[로봇] 이벤트ID {currentEventId} 설명 시작");
                // 설명 종료시 원래 추적모드로 복귀 (타이밍은 실제 설명 끝나면 호출)
                // StartCoroutine(EndExplainModeDelay(3f)); // 3초 후 모드 복귀 예시
            }
        }
    }

    // 외부에서 설명 종료 호출 (설명 끝나면)
    public void EndExplainMode()
    {
        currentMode = RobotMode.Follow;
        currentEventId = -1;
        explainTarget = null;
        isExplaining = false;
        if (descriptionUI != null)
            descriptionUI.ClearDescription();
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

    void Walk_Animation()
    {
        animator.SetBool("Open_Anim", true);
        animator.SetBool("Walk_Anim", true);
        animator.SetBool("Roll_Anim", false);
    }
    void Idle_Animation()
    {
        animator.SetBool("Open_Anim", true);
        animator.SetBool("Walk_Anim", false);
        animator.SetBool("Roll_Anim", false);
    }

    // 추후 필요시 충돌 처리
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 필요시 구현
        }
    }
}
