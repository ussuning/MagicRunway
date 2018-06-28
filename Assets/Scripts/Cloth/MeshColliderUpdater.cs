using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(ObiCollider))]
public class MeshColliderUpdater : MonoBehaviour
{

    MeshCollider meshCollider;
    SkinnedMeshRenderer meshRenderer;
    ObiCollider obiCollider;
    // Use this for initialization
    void Start()
    {
        meshRenderer = GetComponent<SkinnedMeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        obiCollider = GetComponent<ObiCollider>();
    }

    // Update is called once per frame
    void OnRenderObject()
    {
        UpdateCollider();
        //obiCollider.SourceCollider = meshCollider;
        obiCollider.SourceCollider = meshCollider;

    }

    public void UpdateCollider()
    {
        Mesh colliderMesh = new Mesh();
        meshRenderer.BakeMesh(colliderMesh); // Maybe try baking into existing mesh
        Mesh oldMesh = meshCollider.sharedMesh;
        Destroy(oldMesh);
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = colliderMesh;
        obiCollider.SourceCollider = meshCollider;

        //meshCollider.sharedMesh.Clear();
        //meshRenderer.BakeMesh(meshCollider.sharedMesh);

    }
}
