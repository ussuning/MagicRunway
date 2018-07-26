using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using MR;

public class LimbObjectTransferer : MonoBehaviour {
    public Transform source;
    public Transform destination;
    private string delimiter = "<>";

    public void CopyToDest () {
        Dictionary<LimbObject, string> limbMap = new Dictionary<LimbObject, string>();
        LimbObject [] limbObjs = source.GetComponentsInChildren<LimbObject>();
        foreach (LimbObject limbObj in limbObjs) {
            Transform parent = limbObj.transform.parent;
            if (parent != null) {
                limbMap.Add(limbObj, parent.name);
            } else {
                limbMap.Add(limbObj, null);
            }
        }

        foreach (KeyValuePair<LimbObject, string> kvp in limbMap) {
            string parentName = kvp.Value;
            LimbObject limbObject = kvp.Key;

            Transform parent = destination;
            if (parentName != null)
            {
                parent = destination.FindDeepChild(parentName);
                if (parent == null)
                    Debug.LogError("Could not find child " + parentName + " in destination for limbObject " + limbObject.name + "!");
            }
                
            LimbObject neoLimbObj = Instantiate<LimbObject>(limbObject);
            neoLimbObj.transform.parent = parent;
        }
	}

    public void SelectInSource() {
        LimbObject [] limbObjs = source.GetComponentsInChildren<LimbObject>();
#if UNITY_EDITOR
        Selection.objects = limbObjs;
#endif
    }

    public void SelectInDest()
    {
        LimbObject[] limbObjs = destination.GetComponentsInChildren<LimbObject>();

#if UNITY_EDITOR
        Selection.objects = limbObjs;
#endif
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(LimbObjectTransferer))]
public class LimbObjectTransfererEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LimbObjectTransferer t = (LimbObjectTransferer)target;
        if (GUILayout.Button("Copy"))
        {
            if (t.source == null || t.destination == null )
                Debug.LogError("Missing Source or Destination");
            else
            {
                t.CopyToDest();
            }

        }
        if (GUILayout.Button("Select Source")) {
            t.SelectInSource();
        }
        if (GUILayout.Button("Select Dest"))
        {
            t.SelectInDest();
        }
    }
}
#endif