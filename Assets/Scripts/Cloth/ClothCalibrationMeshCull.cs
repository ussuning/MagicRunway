
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class ClothCalibrationMeshCull : MonoBehaviour
{
    public SkinnedMeshRenderer body;
    public Collider[] inclusionZones;
    public Mesh originalMesh;

    // Use this for initialization
    void Awake()
    {
    }

    public void CullBodyMeshFaces()
    {
        RestoreOriginalMesh();

        if (body.sharedMesh.name.Contains("Clone") == false)
        {
            originalMesh = body.sharedMesh;
        }
        Mesh copy = Instantiate<Mesh>(body.sharedMesh);

        //// Pre-calculate sqr magnitudes for hit-detection.
        //List<float> zoneSqrMagnitudeByIdx = new List<float>();
        //for (int z = 0; z < invisibleBodyZones.Length; z++) {
        //    float radius = invisibleBodyZones[z].transform.lossyScale.x / 2.0f;
        //    zoneSqrMagnitudeByIdx.Add(radius * radius);
        //}
        //List<Bounds> boundsByIdx = new List<Bounds>();
        //for (int z = 0; z < invisibleBodyZones.Length; z++) {
        //    float sizeX = invisibleBodyZones[z].transform.lossyScale.x;
        //    boundsByIdx.Add(new Bounds(invisibleBodyZones[z].transform.position, new Vector3(sizeX, sizeX, sizeX)));
        //}

        int fastFailCount = 0;

        for (int submeshIdx = 0; submeshIdx < copy.subMeshCount; submeshIdx++)
        {

            int[] oldTris = copy.GetTriangles(submeshIdx);
            List<int> newTris = new List<int>();
            for (int i = 0; i < oldTris.Length; i += 3)
            {
                Vector3 v1 = copy.vertices[oldTris[i]];
                Vector3 v2 = copy.vertices[oldTris[i + 1]];
                Vector3 v3 = copy.vertices[oldTris[i + 2]];
                //Vector3 avg = (v1 + v2 + v3) / 3.0f;

                bool hit = false;
                for (int z = 0; z < inclusionZones.Length; z++)
                {
                    if (inclusionZones[z].bounds.Contains(v1) ||
                        inclusionZones[z].bounds.Contains(v2) ||
                        inclusionZones[z].bounds.Contains(v3)
                       )
                    {
                        hit = true;
                        break;
                    }
                }

                if (hit)
                {
                    newTris.Add(oldTris[i]);
                    newTris.Add(oldTris[i + 1]);
                    newTris.Add(oldTris[i + 2]);
                }
            }

            copy.SetTriangles(newTris, submeshIdx);
        }
        Debug.Log("FastFailCount = " + fastFailCount);
        body.sharedMesh = copy;
    }

    public void RestoreOriginalMesh()
    {
        if (originalMesh == null)
        {
            Debug.LogError("No Original Mesh from which to restore!");
            return;
        }
        if (body.sharedMesh.name.Contains("Clone"))
        {
            DestroyImmediate(body.sharedMesh);
        }
        body.sharedMesh = null;
        body.sharedMesh = originalMesh;
    }
}



#if UNITY_EDITOR
[CustomEditor(typeof(ClothCalibrationMeshCull))]
public class ClothCalibrationMeshCullEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ClothCalibrationMeshCull myScript = (ClothCalibrationMeshCull)target;
        if (GUILayout.Button("Cull BodyMesh Faces"))
        {
            myScript.CullBodyMeshFaces();
        }
        if (GUILayout.Button("Restore Mesh"))
        {
            myScript.RestoreOriginalMesh();
        }
    }
}
#endif