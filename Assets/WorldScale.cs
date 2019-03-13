using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WorldScale : MonoBehaviour {
    public void Reset()
    {
        Vector3 scale = transform.lossyScale;
        transform.localScale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
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
        GUILayout.Label(myScript.transform.lossyScale.ToString());
        if (GUILayout.Button("Reset"))
        {
            myScript.Reset();
        }
    }
}
#endif
