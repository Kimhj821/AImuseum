using UnityEngine;

public class ObjectHighlight : MonoBehaviour
{
    public Material normalMat;
    public Material highlightedMat;
    private MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>(); //컴포넌트 가지고 오기
    }

    public void OnHoverEnter()
    {
        meshRenderer.material = highlightedMat; //닿으면 highlightedMat 
    }

    public void OnHoverExit()
    {
        meshRenderer.material = normalMat; //떼면 다시 원상 복구
    }
}
