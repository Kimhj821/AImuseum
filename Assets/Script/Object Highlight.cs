using UnityEngine;

public class ObjectHighlight : MonoBehaviour
{
    public Material normalMat;
    public Material highlightedMat;
    private MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void OnHoverEnter()
    {
        meshRenderer.material = highlightedMat;
    }

    public void OnHoverExit()
    {
        meshRenderer.material = normalMat;
    }
}
