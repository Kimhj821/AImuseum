using UnityEngine;

public class PortalViewSync : MonoBehaviour
{
    public Transform playerCamera;       // 메인 카메라 (XR에서는 CenterEyeAnchor)
    public Transform portalIn;           // 입구 포탈 Transform
    public Transform portalOut;          // 출구 포탈 Transform
    public Camera portalCamera;          // 출구 포탈에서 보는 카메라

    void LateUpdate()
    {
        Vector3 relativePos = playerCamera.position - portalIn.position;
        portalCamera.transform.position = portalOut.position + relativePos;

        Quaternion relativeRot = playerCamera.rotation * Quaternion.Inverse(portalIn.rotation);
        portalCamera.transform.rotation = portalOut.rotation * relativeRot;
    }
}
