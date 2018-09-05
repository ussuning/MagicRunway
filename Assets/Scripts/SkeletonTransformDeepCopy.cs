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
        Transform destNode = dest.FindDeepChild(node.name);
        destNode.transform.ApplyTransformValues(node.transform.ToTransformValues());

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
