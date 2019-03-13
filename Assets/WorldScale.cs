using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WorldScale : MonoBehaviour {
    public void Reset()
    {
        AvatarController.ResetJointScale(transform);
    }

    //Recursive
    public void AddToChildren(GameObject go)
    {
        if (go.GetComponent<WorldScale>() == null)
            go.AddComponent<WorldScale>();

        foreach (Transform t in go.transform)
            AddToChildren(t.gameObject);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WorldScale))]
public class WorldScaleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WorldScale myScript = (WorldScale)target;
        GUILayout.Label(myScript.transform.lossyScale.ToString("G4"));
        if (GUILayout.Button("Reset Scale"))
        {
            myScript.Reset();
        }
        if (GUILayout.Button("Add To Children"))
        {
            myScript.AddToChildren(myScript.gameObject);
        }
    }
}
#endif
