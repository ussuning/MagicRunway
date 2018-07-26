using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class ClothCalibrationGroup : MonoBehaviour {

    public float minBackstop = 0;

    
    public void CalibrateAll()
    {
        ClothCalibration[] clothCalibrations = GetComponentsInChildren<ClothCalibration>();
        foreach (ClothCalibration clothCalibration in clothCalibrations)
        {
            clothCalibration.CalibrateSkinBackStop();
        }
    }
}



#if UNITY_EDITOR
[CustomEditor(typeof(ClothCalibrationGroup))]
public class ClothCalibrationGroupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ClothCalibrationGroup myScript = (ClothCalibrationGroup)target;
        if (GUILayout.Button("Calibrate All"))
        {
            myScript.CalibrateAll();
        }
    }
}
#endif
