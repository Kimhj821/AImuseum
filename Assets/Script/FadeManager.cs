using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FadeManager : MonoBehaviour
{
    // 어디서든 접근 가능한 싱글톤
    public static FadeManager Instance;

    // 페이드 효과를 위한 애니메이터 (검은 화면용 이미지에 붙어 있음)
    public Animator fadeAnimator;

    // 이동 대상이 될 XR Origin
    public XROrigin xrOrigin;

    void Awake() => Instance = this;

    /// <summary>
    /// 페이드 인/아웃과 함께 XR Origin 위치 이동
    /// </summary>
    public void FadeAndMoveTo(Vector3 targetPosition)
    {
        StartCoroutine(FadeSequence(targetPosition));
    }

    private IEnumerator FadeSequence(Vector3 targetPos)
    {
        // 1. 화면 어둡게
        fadeAnimator.SetTrigger("fadeOut");

        // 2. 1초 대기 (애니메이션 길이만큼)
        yield return new WaitForSeconds(5f);

        // 3. XR Origin 위치 이동
        xrOrigin.transform.position = targetPos;

        // 4. 화면 다시 밝게
        fadeAnimator.SetTrigger("fadeIn");
    }
}
