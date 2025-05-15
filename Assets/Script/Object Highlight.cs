using UnityEngine;
using System.Collections.Generic;
public class ObjectHighlight : MonoBehaviour
{
    public Material outlineMaterial;  // 외곽선 머티리얼
    private Renderer rend;

    private Material[] OriginalInstanceMat;

    void Start()
    {
        rend = GetComponent<Renderer>();
        OriginalInstanceMat = rend.materials;
    }

   

    public void OnHoverEnter()
    {
        var newMaterials = new List<Material>(OriginalInstanceMat);
        newMaterials.Add(outlineMaterial);
        rend.materials = newMaterials.ToArray();
    }

    public void OnHoverExit()
    {
        rend.materials = OriginalInstanceMat;
    }
}