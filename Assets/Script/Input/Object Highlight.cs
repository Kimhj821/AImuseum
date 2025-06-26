using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
public class ObjectHighlight : MonoBehaviour
{
    public Material outlineMaterial;
    
    private Renderer rend;

    private Material[] originalMaterials;

    void Start()
    {
        rend = GetComponent<Renderer>();
        originalMaterials = rend.materials;
    }

    public void OnHoverEnter()
    {
        var newMat = new List<Material>(originalMaterials);
        newMat.Add(outlineMaterial);
        rend.materials = newMat.ToArray();
    }

    public void OnHoverExit()
    {
        rend.materials = originalMaterials;
    }
    
    
}