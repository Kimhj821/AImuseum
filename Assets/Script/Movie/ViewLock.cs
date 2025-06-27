using UnityEngine;
using Unity.XR.CoreUtils;

public class ViewLock : MonoBehaviour
{
    public static ViewLock Instance;

    private XROrigin xrOrigin;
    //private bool isLocked = false;
    private Vector3 lockPosition;
    private Quaternion lockRotation;

    void Awake()
    {
        Instance = this;
        xrOrigin = FindFirstObjectByType<XROrigin>();
    }

    /// <summary>
    /// XR Origin을 지정 위치/회전으로 '고정'한다.
    /// </summary>
    public void LockView(Vector3 worldPos, Quaternion worldRot)
    {
        if (xrOrigin != null)
        {
            // XR Origin 이동 및 회전
            xrOrigin.transform.position = worldPos;
            xrOrigin.transform.rotation = worldRot;
            //isLocked = true;
            lockPosition = worldPos;
            lockRotation = worldRot;
            Debug.Log($"[ViewLock] XR Origin 이동! pos:{worldPos}, rot:{worldRot.eulerAngles}");
        }
    }

    /// <summary>
    /// XR Origin '고정' 해제 (사실 XR에서 사용자는 여전히 움직일 수 있음)
    /// </summary>
    // public void UnlockView()
    // {
    //     isLocked = false;
    //     Debug.Log("[ViewLock] 시점 해제!");
    // }

    // LateUpdate는 이제 불필요, XR에서는 XR Origin만 움직이면 됨
    // void LateUpdate() { ... } <<== 삭제
}
