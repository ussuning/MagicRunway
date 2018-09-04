using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SkinnedMeshCopier : MonoBehaviour {
    public SkinnedMeshRenderer source;
    public SkinnedMeshRenderer dest;
	internal void Copy()
    {
        // copy bones and bindpose fields from source
        SkinnedMeshRenderer skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer == null)
            skinnedMeshRenderer = this.gameObject.AddComponent<SkinnedMeshRenderer>();
        skinnedMeshRenderer.sharedMesh = source.sharedMesh;
        skinnedMeshRenderer.materials = source.materials;
        skinnedMeshRenderer.bones = source.bones;
        skinnedMeshRenderer.sharedMesh.boneWeights = source.sharedMesh.boneWeights;
        skinnedMeshRenderer.sharedMesh.bindposes = source.sharedMesh.bindposes;
        CopyBonesWithDictionary();
    }

    // From https://forum.unity.com/threads/transfer-the-rig-of-a-skinned-mesh-renderer-to-a-new-smr-with-code.499008/ -HH
    // This corrects bone mapping from source skinnedMeshRenderer to destination.
    internal void CopyBonesWithDictionary()
    {
        SkinnedMeshRenderer targetRenderer = dest;

        Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
        foreach (Transform bone in targetRenderer.bones)
        {
            boneMap[bone.name] = bone;
        }

        SkinnedMeshRenderer thisRenderer = GetComponent<SkinnedMeshRenderer>();
        Transform[] boneArray = thisRenderer.bones;
        for (int idx = 0; idx < boneArray.Length; ++idx)
        {
            string boneName = boneArray[idx].name;
            if (false == boneMap.TryGetValue(boneName, out boneArray[idx]))
            {
                Debug.LogError("failed to get bone: " + boneName);
                Debug.Break();
            }
        }
        thisRenderer.bones = boneArray;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SkinnedMeshCopier))]
public class SkinnedMeshCopierEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SkinnedMeshCopier myScript = (SkinnedMeshCopier)target;
        if (GUILayout.Button("Copy"))
        {
            myScript.Copy();
        }
    }
}
#endif