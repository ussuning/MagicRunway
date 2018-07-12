using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;
using UnityEditor;
using System;

[ExecuteInEditMode]
public class ClothCalibration : MonoBehaviour
{
    public string collisionLayerName = "ClothCalibration";
    public float backstopOffset = 0;
    public float backstopScale = 1.0f; // 1 = 100%, scale backstop by 
    public float backstopMax = 0.2f;
    public float rayOriginHeight = 0.002f;
    public MeshRenderer[] backstopCollisionMeshes;

    ObiCloth obiCloth;
    ObiCloth destObiCloth; // Only used for copying particle values;

    // Use this for initialization
    void Awake()
    {
        if (obiCloth == null)
        {
            obiCloth = GetComponent<ObiCloth>();
        }
    }

    public void CalibrateSkinBackStop()
    {
        int collisionLayer = LayerMask.NameToLayer(collisionLayerName);
        if (collisionLayer == 0)
        {
            Debug.LogError("collisionLayerName " + collisionLayerName + " doesn't exist! Please define in user layer settings! Aborting!");
            return;
        }

        if (backstopCollisionMeshes == null)
        {
            Debug.LogError("No backstop collision meshes defined! Aborting!");
            return;
        }
        else
        {
            foreach (MeshRenderer mesh in backstopCollisionMeshes)
            {
                mesh.gameObject.layer = collisionLayer;
                if (mesh.gameObject.GetComponent<MeshCollider>() == null)
                {
                    MeshCollider neoCollider = mesh.gameObject.AddComponent<MeshCollider>();
                    if (neoCollider.sharedMesh == null)
                    {
                        neoCollider.sharedMesh = mesh.GetComponent<MeshFilter>()?.mesh;
                        if (neoCollider.sharedMesh == null)
                        {
                            Debug.LogError("Could not create MeshCollider for mesh " + mesh.name + " because it lacks a MeshFilter component! Aborting!");
                            return;
                        }
                    }
                }
            }
        }
        List<ObiSkinConstraintBatch> batches = obiCloth.SkinConstraints.GetSkinBatches();
        for (int b = 0; b < batches.Count; b++)
        {
            ObiSkinConstraintBatch batch = batches[b];
            for (int i = 0; i < batch.skinIndices.Count; i++)
            {
                int skinRadiusIdx = i * 3;
                int collisionRadiusIdx = skinRadiusIdx + 1;
                int backstopIdx = skinRadiusIdx + 2;

                Vector3 pos = batch.skinPoints[i];
                Vector3 normal = batch.skinNormals[i];

                Vector3 origin = pos + normal.normalized * rayOriginHeight;
                Vector3 direction = -normal;

                RaycastHit hitInfo;
                int layerMask = LayerMask.GetMask(collisionLayerName);
                if (Physics.Raycast(origin, direction, out hitInfo, float.MaxValue, layerMask))
                {
                    float dist = (hitInfo.point - pos).magnitude;
                    float finalDist = dist * backstopScale + backstopOffset;
                    if (finalDist > backstopMax)
                    {
                        finalDist = backstopMax;
                    }
                    batch.skinRadiiBackstop[backstopIdx] = finalDist;
                    batch.skinRadiiBackstop[skinRadiusIdx] = finalDist;
                }
                else
                {
                    Debug.LogError("[" + b + "," + i + "] Missed the raycast! This shouldn't happen! pos=" + pos + " normal=" + normal);
                }

            }
        }
    }

    internal void CopyCalibrationValues()
    {
        List<ObiSkinConstraintBatch> batches = obiCloth.SkinConstraints.GetSkinBatches();
        List<ObiSkinConstraintBatch> destBatches = destObiCloth.SkinConstraints.GetSkinBatches();
        if (batches.Count != destBatches.Count)
        {
            Debug.LogError("Mismatching batches.Count! Aborting!");
            return;
        }
        for (int b = 0; b < batches.Count; b++)
        {
            ObiSkinConstraintBatch batch = batches[b];
            ObiSkinConstraintBatch destBatch = destBatches[b];
            if (batch.skinIndices.Count != destBatch.skinIndices.Count) {
                Debug.LogError("Mismatching skinIndices.Count at batch idx="+b+"! Aborting!");
                return;
            }
            for (int i = 0; i < batch.skinIndices.Count; i++)
            {
                if (batch.skinIndices[i] != destBatch.skinIndices[i]) {
                    Debug.LogError("Mismatching skinIndices at skin idx=" + i + "! Aborting!");
                    return;
                }
            }
        }

        // Ok, do actual copying.
        for (int b = 0; b < batches.Count; b++)
        {
            ObiSkinConstraintBatch batch = batches[b];
            ObiSkinConstraintBatch destBatch = destBatches[b];
            for (int i = 0; i < batch.skinIndices.Count; i++)
            {
                destBatch.skinNormals = new List<Vector4>(batch.skinNormals);
                destBatch.skinPoints = new List<Vector4>(batch.skinPoints);
                destBatch.skinRadiiBackstop = new List<float>(batch.skinRadiiBackstop);
                destBatch.skinStiffnesses = new List<float>(batch.skinStiffnesses);
            }
        }

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
        if (GUILayout.Button("Copy Particles"))
        {
            myScript.CopyCalibrationValues();
        }
    }
}




