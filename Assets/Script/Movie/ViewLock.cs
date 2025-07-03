using UnityEngine;
using Unity.XR.CoreUtils;

public class ViewLock : MonoBehaviour
{
    public static ViewLock Instance;

    private XROrigin xrOrigin;

    private Vector3 lockPosition;
    private Quaternion lockRotation;

    void Awake()
    {
        Instance = this;
        xrOrigin = FindFirstObjectByType<XROrigin>();
    }

    public void LockView(Vector3 worldPos, Quaternion worldRot)
    {
        if (xrOrigin != null)
        {
            xrOrigin.transform.position = worldPos;
            xrOrigin.transform.rotation = worldRot;
            lockPosition = worldPos;
            lockRotation = worldRot;
            Debug.Log($"[ViewLock] XR Origin 이동! pos:{worldPos}, rot:{worldRot.eulerAngles}");
        }
    }

    public void UnlockView()
    {
        Debug.Log("[ViewLock] 시점 해제!");
        // 필요하다면 원위치로 이동, 여기선 단순 로그만
    }
}
