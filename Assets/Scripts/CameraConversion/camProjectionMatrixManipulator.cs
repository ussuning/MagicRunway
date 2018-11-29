using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class camProjectionMatrixManipulator : MonoBehaviour {
    public Matrix4x4 projectionMatrix = Matrix4x4.identity;
    public Matrix4x4 offsetMatrix = Matrix4x4.zero;
    protected Camera cam;

    internal Windows.Kinect.KinectSensor kinectSensor;
    internal Windows.Kinect.FrameDescription colorFrameDesc;
    internal Windows.Kinect.FrameDescription depthFrameDesc;

    // Use this for initialization
    void Start () {
        cam = GetComponent<Camera>();
        Init();

	}

    public void Init()
    {
        projectionMatrix = cam.projectionMatrix;
        for (int row = 0; row < 4; row++)
        {
            Vector4 values = projectionMatrix.GetRow(row);
            Vector4 offsets = offsetMatrix.GetRow(row);
            values += offsets;
            projectionMatrix.SetRow(row, values);
        }

        DepthSensorInterface dsi = KinectManager.Instance.sensorInterfaces[0];
        Kinect2Interface k2i = dsi as Kinect2Interface;
        if (k2i != null)
        {
            //GUILayout.Label("depthCameraHFOV: " + (KinectManager.Instance.sensorInterfaces[0];
            kinectSensor = k2i.kinectSensor;
            colorFrameDesc = kinectSensor.ColorFrameSource.FrameDescription;
            depthFrameDesc = kinectSensor.DepthFrameSource.FrameDescription;

            Debug.Log("kinectSensor:uid = " + kinectSensor.UniqueKinectId);

            Debug.Log(GetColorFrameDesc());
            Debug.Log(GetDepthFrameDesc());
        }
        else
        {
            Debug.LogError("No Kinect2Interface Detected.");
        }
    }

    internal string GetColorFrameDesc()
    {
        if (colorFrameDesc == null)
            return "colorFrameDesc == null";
        else
            return "colorCameraFOV: " + colorFrameDesc.HorizontalFieldOfView + "H x " + colorFrameDesc.VerticalFieldOfView + "V" + " (" + colorFrameDesc.Width + "px x " + colorFrameDesc.Height + "px)";
    }

    internal string GetDepthFrameDesc()
    {
        if (depthFrameDesc == null)
            return "depthFrameDesc == null";
        else
            return "depthCameraFOV: " + depthFrameDesc.HorizontalFieldOfView + "H x " + depthFrameDesc.VerticalFieldOfView + "V" + " (" + depthFrameDesc.Width + "px x " + depthFrameDesc.Height + "px)";
    }
	
	// Update is called once per frame
	void Update () {
		if (projectionMatrix != cam.projectionMatrix)
        {
            cam.projectionMatrix = projectionMatrix;
        }
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(camProjectionMatrixManipulator))]
public class camProjectionMatrixManipulatorEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        camProjectionMatrixManipulator myScript = (camProjectionMatrixManipulator)target;
        if (GUILayout.Button("ReInitialize"))
        {
            myScript.Init();
        }

        GUILayout.Label("kinectSensor: uid = " + (myScript.kinectSensor != null ? myScript.kinectSensor.UniqueKinectId : "null"));
        GUILayout.Label(myScript.GetColorFrameDesc());
        GUILayout.Label(myScript.GetDepthFrameDesc());
    }
}
#endif