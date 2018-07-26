using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MR;

[ExecuteInEditMode]
public class TransformDeepCopy : MonoBehaviour {
    public struct TransformValues
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
    }

    Dictionary<string, TransformValues> valuesByName = new Dictionary<string, TransformValues>();
    public Transform destination;

	internal void Copy()
    {
        if (destination == null)
            Debug.LogError("No Destination Transform defined! Aborting TransformDeepCopy()!");

        valuesByName.Clear();
        CopyRecursive(transform);
        PasteValues(destination);
    }

    void CopyRecursive(Transform t)
    {
        valuesByName.Add(t.name, t.ToTransformValues());
        for(int i=0; i<t.childCount; i++)
        {
            CopyRecursive(t.GetChild(i));
        }
    }

    void PasteValues(Transform t)
    {
        bool success = true;
        foreach(KeyValuePair<string, TransformValues> kvp in valuesByName)
        {
            string name = kvp.Key;
            TransformValues tValues = kvp.Value;
            Transform foundChild = t.FindDeepChild(name);
            if (foundChild == null)
            {
                Debug.LogError("Unable to find child " + name + " in destination Transform! Skipping!");
                success = false;
                continue;
            } else
            {
                foundChild.ApplyTransformValues(tValues);
            }
        }

        Debug.Log("Paste Values success = " + success);
    }
}

[CustomEditor(typeof(TransformDeepCopy))]
public class TransformDeepCopyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TransformDeepCopy myScript = (TransformDeepCopy)target;
        if (GUILayout.Button("Copy"))
        {
            myScript.Copy();
        }
    }
}
