using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SphereColliderGenerator : MonoBehaviour {

    public SkinnedMeshRenderer skinnedMeshRenderer;
	
	
    public void generate() {

        Vector3[] verts = skinnedMeshRenderer.sharedMesh.vertices;

        /*
        GameObject neoObj = new GameObject(skinnedMeshRenderer.name + " generated");
        neoObj.transform.parent = this.transform;
        neoObj.transform.position = skinnedMeshRenderer.transform.position;

        SkinnedMeshRenderer neoSkinnedMeshRenderer = neoObj.AddComponent<SkinnedMeshRenderer>();
        neoSkinnedMeshRenderer.bones = skinnedMeshRenderer.bones;
        neoSkinnedMeshRenderer.rootBone = skinnedMeshRenderer.rootBone;
        Mesh neoMesh = new Mesh();
        neoMesh.vertices = skinnedMeshRenderer.sharedMesh.vertices;
        neoMesh.triangles = skinnedMeshRenderer.sharedMesh.triangles;
        neoMesh.boneWeights = skinnedMeshRenderer.sharedMesh.boneWeights;
        neoMesh.bindposes = skinnedMeshRenderer.sharedMesh.bindposes;
        neoMesh.RecalculateNormals();
        neoSkinnedMeshRenderer.sharedMesh = neoMesh;
        */

        for (int i = 0; i < verts.Length; i++) {
            Vector3 vert = verts[i];
            GameObject neoObj = new GameObject("v_" + i);
            neoObj.transform.parent = this.transform;
            neoObj.transform.position = vert;
            neoObj.transform.position *= 0.01f;
            SphereCollider neoCollider = neoObj.AddComponent<SphereCollider>();
            neoCollider.radius = 0.01f;
            SkinnedMeshRenderer neoSkinnedMeshRenderer = neoObj.AddComponent<SkinnedMeshRenderer>();
            neoSkinnedMeshRenderer.bones = skinnedMeshRenderer.bones;
            neoSkinnedMeshRenderer.rootBone = skinnedMeshRenderer.rootBone;
            Mesh neoMesh = new Mesh();
            neoMesh.vertices = new Vector3[] { vert, vert, vert };
            neoMesh.triangles = new int[] { 0, 1, 2 };
            neoMesh.boneWeights = new BoneWeight[] { skinnedMeshRenderer.sharedMesh.boneWeights[i], skinnedMeshRenderer.sharedMesh.boneWeights[i], skinnedMeshRenderer.sharedMesh.boneWeights[i] };
            neoMesh.bindposes = skinnedMeshRenderer.sharedMesh.bindposes;
            neoMesh.RecalculateNormals();
            neoSkinnedMeshRenderer.sharedMesh = neoMesh;
        }
    }

    public void clear() {
        foreach (Transform child in transform)
        {
            GameObject.DestroyImmediate(child.gameObject);
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(SphereColliderGenerator))]
public class SphereColliderGeneratorEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SphereColliderGenerator t = (SphereColliderGenerator)target;
        if (GUILayout.Button("Generate"))
        {
            t.generate();
        }
        if (GUILayout.Button("Clear"))
        {
            t.clear();
        }
    }
}
#endif