using UnityEngine;
using System.Collections;

public class Robot_Control : MonoBehaviour
{
    public Transform player;
    public float stopDistance = 2.0f;
    public float walkSpeed = 2.5f;
    public float rotationSpeed = 5.0f;
    public float startupDelay = 1.0f; // 처음 시작 시 대기 시간

    private Animator animator;
    private bool isStartupComplete = false;

    public float speed = 0;

    void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(StartupDelayRoutine());
    }

    IEnumerator StartupDelayRoutine()
    {
        Idle_Animation(); // 초기에는 정지 상태
        yield return new WaitForSeconds(startupDelay);
        isStartupComplete = true;
    }

    void Update()
    {
        if (player == null || animator == null || !isStartupComplete) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0;
        float distance = direction.magnitude;

        // 부드럽게 수평 회전
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        if (speed > 0)
        {
            Walk_Animation();
        }
        else
        {
            Idle_Animation();
        }


        // 이동 여부 결정
        if (distance > stopDistance)
        {
            Vector3 moveDir = direction.normalized;
            transform.position += moveDir * walkSpeed * Time.deltaTime;
            speed = walkSpeed;
        }
        else
        {
            speed = 0;
        }
    }

    void Walk_Animation()
    {
        animator.SetBool("Open_Anim", true);
        animator.SetBool("Walk_Anim", true);
        animator.SetBool("Roll_Anim", false); // 고정
    }

    void Idle_Animation()
    {
        animator.SetBool("Open_Anim", true);
        animator.SetBool("Walk_Anim", false);
        animator.SetBool("Roll_Anim", false); // 고정
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            speed = 0;
        }
    }
}
