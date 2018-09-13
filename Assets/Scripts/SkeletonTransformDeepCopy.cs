using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MR;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SkeletonTransformDeepCopy : MonoBehaviour {
    public Transform source;
    protected Transform dest;
	
	// Update is called once per frame
	public void Copy()
    {
        dest = transform;
        RecursiveCopy(source);

    }

    void RecursiveCopy(Transform node)
    {
        TransformDeepCopy.TransformValues tValues = node.ToTransformValues();
        Transform destNode = dest.FindDeepChild(node.name);
        if (destNode == null)
        {
            Debug.LogError("node.name could not be found = " + node.name);
            return;
        }
        else
        {
            // Compare and log any mismatching values.
            if (destNode.ToTransformValues() != tValues)
                Debug.LogWarning("destNode values do not match node values, where node.name = " + node.name);
        }
        destNode.transform.ApplyTransformValues(tValues);

        if (node.childCount > 0)
            foreach (Transform child in node)
                RecursiveCopy(child);
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(SkeletonTransformDeepCopy))]
public class SkeletonTransformDeepCopyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SkeletonTransformDeepCopy myScript = (SkeletonTransformDeepCopy)target;
        if (GUILayout.Button("Copy Values"))
        {
            myScript.Copy();
        }
    }
}
#endif
