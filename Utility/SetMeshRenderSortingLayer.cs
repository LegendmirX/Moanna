using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMeshRenderSortingLayer : MonoBehaviour
{
    public string LayerName;

    private void Awake()
    {
        MeshRenderer meshRenderer = this.GetComponent<MeshRenderer>();
        meshRenderer.sortingLayerName = LayerName;
    }
}
