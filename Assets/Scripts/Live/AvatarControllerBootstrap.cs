using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using MR;
using Obi;
using System;

// This should be used on clothing prefabs, in editor mod. 
// Be sure to click "Initialize" BEFORE to starting the app. Buggy things happen if you try to run Init() after the app starts.
// If you don't want AvatarController to be controlled via kinect, select disableOnStart or invoke DisableAvatarControllers();

public class AvatarControllerBootstrap : MonoBehaviour {
    //public bool disableOnStart = false;
    public int playerIndex = 0;
    public AvatarBoneMapMode avatarBoneMapMode = AvatarBoneMapMode.Mixamo;

    //protected string BackgroundCamera1 = "BackgroundCamera1";
    protected string ConversionCamera = "Conversion Camera";
    protected AvatarControllerClassic avatarController = null;

    private void Awake()
    {
        // Remove any animator controller
        Animator animator = GetComponent<Animator>();
        if (animator != null)
            animator.runtimeAnimatorController = null;
    }

    //public void DisableAvatarControllers()
    //{
    //    AvatarControllerClassic avatarController = GetComponent<AvatarControllerClassic>();
    //    AvatarScaler avatarScalar = GetComponent<AvatarScaler>();
    //    FacetrackingManager faceTrackingMgr = GetComponent<FacetrackingManager>();
    //    MonoBehaviour[] scripts = new MonoBehaviour[] { avatarScalar, avatarController, faceTrackingMgr };

    //    foreach (MonoBehaviour script in scripts)
    //        if (script != null)
    //            Destroy(script);
    //}

    [ExecuteInEditMode]
    public void Init(int playerIndex = 0) {

        // First, deactivate this gameobject.
        gameObject.SetActive(false);


        transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        transform.FindDeepChild("body")?.gameObject.SetActive(false);
        transform.FindDeepChild("shoes")?.gameObject.SetActive(false);

        // Initialize avatar controller classic
        avatarController = GetComponent<AvatarControllerClassic>();
        if (avatarController == null)
            avatarController = this.gameObject.AddComponent<AvatarControllerClassic>();

        avatarController.posRelativeToCamera = gameObject.FindAny<Camera>(ConversionCamera);
        if (avatarController.posRelativeToCamera == null)
            Debug.LogError("Failed to find " + ConversionCamera);
        avatarController.posRelOverlayColor = true;

        MapBones();

        avatarController.mirroredMovement = true;
        avatarController.verticalMovement = true;
        avatarController.smoothFactor = 0;
        avatarController.playerIndex = playerIndex;
        avatarController.hipWidthFactor = 0f;
        avatarController.shoulderWidthFactor = 0f;
        avatarController.Awake();

        //// Initialize avatar scalar
        //AvatarScaler avatarScalar = GetComponent<AvatarScaler>();
        //if (avatarScalar == null)
        //    avatarScalar = this.gameObject.AddComponent<AvatarScaler>();
        //avatarScalar.foregroundCamera = GameObject.Find(MainCamera)?.GetComponent<Camera>();
        //if (avatarScalar.foregroundCamera == null)
        //    Debug.LogError("Failed to find " + MainCamera);
        //avatarScalar.mirroredAvatar = true;
        //avatarScalar.continuousScaling = true;
        //avatarScalar.smoothFactor = 10;

        // Initialize face tracking manager
        FacetrackingManager faceTrackingMgr = GetComponent<FacetrackingManager>();
        if (faceTrackingMgr == null)
            faceTrackingMgr = this.gameObject.AddComponent<FacetrackingManager>();

        // Change simulation mode to LateUpdate for performance
        ObiSolver[] solvers = GetComponentsInChildren<ObiSolver>();
        foreach (ObiSolver solver in solvers)
            solver.UpdateOrder = ObiSolver.SimulationOrder.LateUpdate;

        // Register with KinectManager
        if (!KinectManager.Instance.avatarControllers.Contains(avatarController))
            KinectManager.Instance.avatarControllers.Add(avatarController);

        // Reactivate
        gameObject.SetActive(true);
        avatarController.LoadConfigData();

        // RefreshAvaterUserIds, this is important to bind, otherwise clothing will wait until another user enters/leaves scene -HH
        KinectManager.Instance.RefreshAvatarUserIds();

        // Other KinectManager settings.
        // Smoothing is not desired, we want the most resonsiveness. Also, smoothing implementation seems to create weird rotational drifting,
        // most noticably in the shoulder joints (overrotatting if you rotate them quickly). -HH
        KinectManager.Instance.velocitySmoothing = KinectManager.Smoothing.None;
        KinectManager.Instance.smoothing = KinectManager.Smoothing.None;
        KinectManager.Instance.sensorHeight = KinectManager.Instance.sensorAngle = 0;
        KinectManager.Instance.autoHeightAngle = KinectManager.AutoHeightAngle.DontUse;
        KinectManager.Instance.computeUserMap = KinectManager.UserMapType.BodyTexture;
        KinectManager.Instance.computeColorMap = true;
        KinectManager.Instance.computeInfraredMap = false;
        KinectManager.Instance.lateUpdateAvatars = true;
        KinectManager.Instance.estimateJointVelocities = false;

        // Add reflection probes to clothing
        ReflectionProbe reflectionProbe = GameObject.FindObjectOfType<ReflectionProbe>();
        reflectionProbe.bakedTexture = GameObject.Find("Conversion Camera Canvas").GetComponentInChildren<UnityEngine.UI.RawImage>().mainTexture;
        if (reflectionProbe != null) {
            SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers)
            {
                renderer.probeAnchor = reflectionProbe.transform;
            }
        }
    }

    protected void MapBones()
    {
        if (avatarController == null) { 
            Debug.LogError("No AvatarController detected. Can't MapBones()");
            return;
        }

        Dictionary<string, string> boneMap = GetBoneMap();
        
        avatarController.BodyRoot = transform;
        System.Type avatarControllerType = avatarController.GetType();
        foreach (KeyValuePair<string, string> kvp in boneMap)
        {
            string boneName = kvp.Value;
            Transform boneTransform = transform.FindDeepChild(boneName);
            if (boneTransform != null)
            {
                // Set avatarController field to boneTransform;
                avatarControllerType.GetField(kvp.Key).SetValue(avatarController, boneTransform);
            }
            else
            {
                Debug.LogError("Mapping error for [" + kvp.Key +"]. Unable to find bone named [" + boneName + "]");
            }
        }

    }

    private Dictionary<string, string> GetBoneMap()
    {
        Dictionary<string, string> boneMap = new Dictionary<string, string>();

        switch (avatarBoneMapMode)
        {
            case AvatarBoneMapMode.Mixamo:
                boneMap.Add("HipCenter",        "mixamorig:Hips");
                boneMap.Add("Spine",            "mixamorig:Spine");
                boneMap.Add("SpineMid",         "mixamorig:Spine1");
                boneMap.Add("ShoulderCenter",   "mixamorig:Spine2");
                boneMap.Add("Neck",             "mixamorig:Neck");
                boneMap.Add("Head",             "mixamorig:Head");
                boneMap.Add("ClavicleLeft",     "mixamorig:LeftShoulder");
                boneMap.Add("ShoulderLeft",     "mixamorig:LeftArm");
                boneMap.Add("ElbowLeft",        "mixamorig:LeftForeArm");
                boneMap.Add("HandLeft",         "mixamorig:LeftHand");
                boneMap.Add("FingersLeft",      "mixamorig:LeftHandIndex1");
                boneMap.Add("ThumbLeft",        "mixamorig:LeftHandThumb1");
                boneMap.Add("ClavicleRight",    "mixamorig:RightShoulder");
                boneMap.Add("ShoulderRight",    "mixamorig:RightArm");
                boneMap.Add("ElbowRight",       "mixamorig:RightForeArm");
                boneMap.Add("HandRight",        "mixamorig:RightHand");
                boneMap.Add("FingersRight",     "mixamorig:RightHandIndex1");
                boneMap.Add("ThumbRight",       "mixamorig:RightHandThumb1");
                boneMap.Add("HipLeft",          "mixamorig:LeftUpLeg");
                boneMap.Add("KneeLeft",         "mixamorig:LeftLeg");
                boneMap.Add("FootLeft",         "mixamorig:LeftFoot");
                boneMap.Add("ToesLeft",         "mixamorig:LeftToeBase");
                boneMap.Add("HipRight",         "mixamorig:RightUpLeg");
                boneMap.Add("KneeRight",        "mixamorig:RightLeg");
                boneMap.Add("FootRight",        "mixamorig:RightFoot");
                boneMap.Add("ToesRight",        "mixamorig:RightToeBase");
                break;
            case AvatarBoneMapMode.KinectCustom:
                boneMap.Add("HipCenter",    "spine_base");
                boneMap.Add("Spine",        "spine_mid");
                boneMap.Add("ShoulderCenter","spine_shoulder");
                boneMap.Add("Neck",         "neck");
                boneMap.Add("Head",         "head");
                boneMap.Add("ShoulderLeft", "shoulder_l");
                boneMap.Add("ElbowLeft",    "elbow_l");
                boneMap.Add("HandLeft",     "wrist_l");
                boneMap.Add("FingersLeft",  "hand_l");
                boneMap.Add("ThumbLeft",    "thumb_l");
                boneMap.Add("ShoulderRight","shoulder_r");
                boneMap.Add("ElbowRight",   "elbow_r");
                boneMap.Add("HandRight",    "wrist_r");
                boneMap.Add("FingersRight", "hand_r");
                boneMap.Add("ThumbRight",   "thumb_r");
                boneMap.Add("HipLeft",      "hip_l");
                boneMap.Add("KneeLeft",     "knee_l");
                boneMap.Add("FootLeft",     "ankle_l");
                boneMap.Add("ToesLeft",     "foot_l");
                boneMap.Add("HipRight",     "hip_r");
                boneMap.Add("KneeRight",    "knee_r");
                boneMap.Add("FootRight",    "ankle_r");
                boneMap.Add("ToesRight",    "foot_r");
                break;
        }

        return boneMap;
    }
}

public enum AvatarBoneMapMode
{
    Mixamo = 0,
    KinectCustom = 1,
}



#if UNITY_EDITOR
[CustomEditor(typeof(AvatarControllerBootstrap))]
public class AvatarControllerBootstrapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AvatarControllerBootstrap myScript = (AvatarControllerBootstrap)target;
        if (GUILayout.Button("Initialize"))
        {
            myScript.Init(myScript.playerIndex);
        }
    }
}
#endif