using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;
using UnityEditor;

[ExecuteInEditMode]
public class ClothCalibration : MonoBehaviour {
    private string bodyLayer = "Body";
    public LayerMask collisionLayers;
    public float minBackstop = 0;
    public float maxRadius = 0.2f;
    public float rayOriginHeight = 0.002f;
    public Collider[] inclusionZones;
    public SkinnedMeshRenderer body;
    public Mesh originalMesh;

    ObiCloth obiCloth;

	// Use this for initialization
	void Awake () {
        if (obiCloth == null) {
            obiCloth = GetComponent<ObiCloth>();
            collisionLayers = LayerMask.GetMask(bodyLayer);
        }    	
	}
	
    public void CalibrateSkinBackStop() {
        if (body != null) {
            body.gameObject.layer = LayerMask.NameToLayer(bodyLayer);
            if (body.gameObject.GetComponent<MeshCollider>() == null) {
                MeshCollider neoCollider = body.gameObject.AddComponent<MeshCollider>();
                if (neoCollider.sharedMesh == null) {
                    neoCollider.sharedMesh = body.sharedMesh;
                }
            }
        }
        List<ObiSkinConstraintBatch> batches = obiCloth.SkinConstraints.GetSkinBatches();
        Debug.Log("CalibrateSkinBackstop - batches count = " + batches.Count);
        foreach (ObiSkinConstraintBatch batch in batches) {
            for (int i = 0; i < batch.skinIndices.Count; i++) {
                int skinRadiusIdx = i * 3;
                int collisionRadiusIdx = skinRadiusIdx + 1;
                int backstopIdx = skinRadiusIdx + 2;

                Vector3 pos = batch.skinPoints[i];
                Vector3 normal = batch.skinNormals[i];

                Vector3 origin = pos + normal.normalized * rayOriginHeight;
                Vector3 direction = -normal;

                RaycastHit hitInfo;
                int layerMask = collisionLayers;
                if (Physics.Raycast(origin, direction, out hitInfo, float.MaxValue, layerMask)) {
                    float dist = (hitInfo.point - pos).magnitude;
                    float finalDist = dist - minBackstop;
                    if (finalDist > maxRadius) {
                        finalDist = maxRadius;
                    }
                    batch.skinRadiiBackstop[backstopIdx] = finalDist;
                    batch.skinRadiiBackstop[skinRadiusIdx] = finalDist;
                } else {
                    Debug.LogError("Missed the raycast! This shouldn't happen! index=" + batch.skinIndices[i] + " pos=" + pos + " normal="+normal);
                }

            }
        }
    }

    public void CullBodyMeshFaces() {
        RestoreOriginalMesh();

        if (body.sharedMesh.name.Contains("Clone") == false) {
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
                       ) {
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

    public void RestoreOriginalMesh() {
        if (originalMesh == null) {
            Debug.LogError("No Original Mesh from which to restore!");
            return;
        }
        if (body.sharedMesh.name.Contains("Clone")) {
            DestroyImmediate(body.sharedMesh);
        }
        body.sharedMesh = null;
        body.sharedMesh = originalMesh;
    }
}



[CustomEditor(typeof(ClothCalibration))]
public class ClothCalibrationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ClothCalibration myScript = (ClothCalibration)target;
        if (GUILayout.Button("Calibrate Skin"))
        {
            myScript.CalibrateSkinBackStop();
        }
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