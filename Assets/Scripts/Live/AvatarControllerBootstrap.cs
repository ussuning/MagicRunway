using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MR;

// This should be used on clothing prefabs, in editor mod. 
// Be sure to click "Initialize" BEFORE to starting the app. Buggy things happen if you try to run Init() after the app starts.
// If you don't want AvatarController to be controlled via kinect, select disableOnStart or invoke DisableAvatarControllers();

public class AvatarControllerBootstrap : MonoBehaviour {
    public bool disableOnStart = false;
    
    protected string BackgroundCamera1 = "BackgroundCamera1";
    protected string MainCamera = "Main Camera";

    void Start()
    {
        if (disableOnStart)
        {
            DisableAvatarControllers();
        }

        // There's a bug where camera unset themselves on play after being set with the "Initialize" button.
        GetComponent<AvatarControllerClassic>().posRelativeToCamera = gameObject.FindAny<Camera>(BackgroundCamera1);// Find(BackgroundCamera1)?.GetComponent<Camera>();
        GetComponent<AvatarScaler>().foregroundCamera = gameObject.FindAny<Camera>(MainCamera);
    }

    public void DisableAvatarControllers()
    {
        AvatarControllerClassic avatarController = GetComponent<AvatarControllerClassic>();
        AvatarScaler avatarScalar = GetComponent<AvatarScaler>();
        FacetrackingManager faceTrackingMgr = GetComponent<FacetrackingManager>();
        MonoBehaviour[] scripts = new MonoBehaviour[] { avatarScalar, avatarController, faceTrackingMgr };

        foreach (MonoBehaviour script in scripts)
            if (script != null)
                Destroy(script);
    }

    [ExecuteInEditMode]
    public void Init() { 

        // Initialize avatar controller classic
        AvatarControllerClassic avatarController = GetComponent<AvatarControllerClassic>();
        if (avatarController == null)
            avatarController = this.gameObject.AddComponent<AvatarControllerClassic>();

        avatarController.posRelativeToCamera = GameObject.Find(BackgroundCamera1)?.GetComponent<Camera>();
        if (avatarController.posRelativeToCamera == null)
            Debug.LogError("Failed to find " + BackgroundCamera1);
        avatarController.posRelOverlayColor = true;
        avatarController.HipCenter =        transform.FindDeepChild("mixamorig:Hips");
        avatarController.Spine =            transform.FindDeepChild("mixamorig:Spine");
        avatarController.ShoulderCenter =   transform.FindDeepChild("mixamorig:Spine2");
        avatarController.Neck =             transform.FindDeepChild("mixamorig:Neck");
        avatarController.Head =             transform.FindDeepChild("mixamorig:Head");
        avatarController.ClavicleLeft =     transform.FindDeepChild("mixamorig:LeftShoulder");
        avatarController.ShoulderLeft =     transform.FindDeepChild("mixamorig:LeftArm");
        avatarController.ElbowLeft =        transform.FindDeepChild("mixamorig:LeftForeArm");
        avatarController.HandLeft =         transform.FindDeepChild("mixamorig:LeftHand");
        avatarController.FingersLeft =      transform.FindDeepChild("mixamorig:LeftHandIndex1");
        avatarController.ThumbLeft =        transform.FindDeepChild("mixamorig:LeftHandThumb1");
        avatarController.ClavicleRight =    transform.FindDeepChild("mixamorig:RightShoulder");
        avatarController.ShoulderRight =    transform.FindDeepChild("mixamorig:RightArm");
        avatarController.ElbowRight =       transform.FindDeepChild("mixamorig:RightForeArm");
        avatarController.HandRight =        transform.FindDeepChild("mixamorig:RightHand");
        avatarController.FingersRight =     transform.FindDeepChild("mixamorig:RightHandIndex1");
        avatarController.ThumbRight =       transform.FindDeepChild("mixamorig:RightHandThumb1");
        avatarController.HipLeft =          transform.FindDeepChild("mixamorig:LeftUpLeg");
        avatarController.KneeLeft =         transform.FindDeepChild("mixamorig:LeftLeg");
        avatarController.FootLeft =         transform.FindDeepChild("mixamorig:LeftFoot");
        avatarController.ToesLeft =         transform.FindDeepChild("mixamorig:LeftToeBase");
        avatarController.HipRight =         transform.FindDeepChild("mixamorig:RightUpLeg");
        avatarController.KneeRight =        transform.FindDeepChild("mixamorig:RightLeg");
        avatarController.FootRight =        transform.FindDeepChild("mixamorig:RightFoot");
        avatarController.ToesRight =        transform.FindDeepChild("mixamorig:RightToeBase");
        avatarController.BodyRoot =         transform;

        avatarController.mirroredMovement = true;
        avatarController.verticalMovement = true;
        avatarController.smoothFactor = 10;
        avatarController.Awake();

        // Initialize avatar scalar
        AvatarScaler avatarScalar = GetComponent<AvatarScaler>();
        if (avatarScalar == null)
            avatarScalar = this.gameObject.AddComponent<AvatarScaler>();
        avatarScalar.foregroundCamera = GameObject.Find(MainCamera)?.GetComponent<Camera>();
        if (avatarScalar.foregroundCamera == null)
            Debug.LogError("Failed to find " + MainCamera);
        avatarScalar.mirroredAvatar = true;
        avatarScalar.continuousScaling = true;
        avatarScalar.smoothFactor = 10;

        // Initialize face tracking manager
        FacetrackingManager faceTrackingMgr = GetComponent<FacetrackingManager>();
        if (faceTrackingMgr == null)
            faceTrackingMgr = this.gameObject.AddComponent<FacetrackingManager>();
	}
}



[CustomEditor(typeof(AvatarControllerBootstrap))]
public class AvatarControllerBootstrapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AvatarControllerBootstrap myScript = (AvatarControllerBootstrap)target;
        if (GUILayout.Button("Initialize"))
        {
            myScript.Init();
        }
    }
}