using UnityEngine;

public class ObjectHighlight : MonoBehaviour
{
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;

    private Material instanceMat;

    void Start()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();

        // ✨ 기존 머티리얼을 복제해서 인스턴스 생성
        instanceMat = new Material(renderer.sharedMaterial);
        renderer.material = instanceMat;

        instanceMat.color = normalColor;
    }

    public void OnHoverEnter()
    {
        Debug.Log("Hover 됨!");
        instanceMat.color = highlightColor;
    }

    public void OnHoverExit()
    {
        instanceMat.color = normalColor;
    }
}