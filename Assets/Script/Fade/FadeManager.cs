using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;
    public Animator fadeAnimator;
    public XROrigin xrOrigin;
    public event Action OnTeleportCompleted;

    void Awake() => Instance = this;

    /// <summary>
    /// 페이드 인/아웃과 함께 XR Origin 위치 이동 및 회전(Y축)
    /// </summary>
    public void FadeAndMoveTo(Vector3 targetPosition, float targetYRotation)
    {
        StartCoroutine(FadeSequence(targetPosition, targetYRotation));
    }

    private IEnumerator FadeSequence(Vector3 targetPos, float targetY)
    {
        fadeAnimator.SetTrigger("fadeOut");
        yield return new WaitForSeconds(5f);

        xrOrigin.transform.position = targetPos;
        xrOrigin.transform.rotation = Quaternion.Euler(0, targetY, 0);

        fadeAnimator.SetTrigger("fadeIn");
        yield return new WaitForSeconds(1.5f);

        OnTeleportCompleted?.Invoke();
    }
}
