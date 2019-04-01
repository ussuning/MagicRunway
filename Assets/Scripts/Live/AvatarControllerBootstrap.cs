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
    internal AvatarControllerClassic avatarController = null;

    public bool drawRawPositions = true;
    public Color rawColor = Color.red;
    public bool drawTunedPositions = true;
    public Color tunedColor = Color.cyan;
    public bool ShowBody = false;
    internal bool initialized = false;
    //public bool drawCurrentRawPositions = true;
    //public Color currentRawColor = Color.green;

    private void Awake()
    {
        onAwake();
    }

    protected virtual void onAwake() { 
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
    public virtual void Init(int playerIndex = 0) {

        // First, deactivate this gameobject.
        gameObject.SetActive(false);


        transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        if (!ShowBody)
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

        avatarController.mirroredMovement = true;
        avatarController.verticalMovement = true;
        //avatarController.smoothFactor = 0;
        avatarController.playerIndex = playerIndex;
        avatarController.Awake();
        MapBones();

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
        avatarController.FlattenBones();
        avatarController.tuner.LoadConfigData();

        // RefreshAvaterUserIds, this is important to bind, otherwise clothing will wait until another user enters/leaves scene -HH
        KinectManager.Instance.RefreshAvatarUserIds();

        initialized = true;

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
        if (reflectionProbe != null)
            {
                reflectionProbe.bakedTexture = GameObject.Find("Conversion Camera Canvas").GetComponentInChildren<UnityEngine.UI.RawImage>().mainTexture;
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

        Dictionary<AvatarControllerClassic.BoneSlot, string> boneMap = GetBoneMap();
        avatarController.initBoneMapping();


        avatarController.BodyRoot = transform;
        System.Type avatarControllerType = avatarController.GetType();
        foreach (KeyValuePair<AvatarControllerClassic.BoneSlot, string> kvp in boneMap)
        {
            AvatarControllerClassic.BoneSlot boneSlot = kvp.Key;
            string boneName = kvp.Value;
            Transform boneTransform = transform.FindDeepChild(boneName);
            if (boneTransform != null)
            {
                avatarController.MapBone(boneSlot, boneTransform);
            }
            else
            {
                Debug.LogError("Mapping error for [" + kvp.Key +"]. Unable to find bone named [" + boneName + "]");
            }
        }
    }

    private Dictionary<AvatarControllerClassic.BoneSlot, string> GetBoneMap()
    {
        Dictionary<AvatarControllerClassic.BoneSlot, string> boneMap = new Dictionary<AvatarControllerClassic.BoneSlot, string>();

        switch (avatarBoneMapMode)
        {
            case AvatarBoneMapMode.Mixamo:
                boneMap.Add(AvatarControllerClassic.BoneSlot.HipCenter,        "mixamorig:Hips");
                boneMap.Add(AvatarControllerClassic.BoneSlot.Spine,            "mixamorig:Spine");
                boneMap.Add(AvatarControllerClassic.BoneSlot.SpineMid,         "mixamorig:Spine1");
                boneMap.Add(AvatarControllerClassic.BoneSlot.ShoulderCenter,   "mixamorig:Spine2");
                boneMap.Add(AvatarControllerClassic.BoneSlot.Neck,             "mixamorig:Neck");
                boneMap.Add(AvatarControllerClassic.BoneSlot.Head,             "mixamorig:Head");
                boneMap.Add(AvatarControllerClassic.BoneSlot.ClavicleLeft,     "mixamorig:LeftShoulder");
                boneMap.Add(AvatarControllerClassic.BoneSlot.ShoulderLeft,     "mixamorig:LeftArm");
                boneMap.Add(AvatarControllerClassic.BoneSlot.ElbowLeft,        "mixamorig:LeftForeArm");
                boneMap.Add(AvatarControllerClassic.BoneSlot.HandLeft,         "mixamorig:LeftHand");
                boneMap.Add(AvatarControllerClassic.BoneSlot.FingersLeft,      "mixamorig:LeftHandIndex1");
                boneMap.Add(AvatarControllerClassic.BoneSlot.ThumbLeft,        "mixamorig:LeftHandThumb2");
                boneMap.Add(AvatarControllerClassic.BoneSlot.ClavicleRight,    "mixamorig:RightShoulder");
                boneMap.Add(AvatarControllerClassic.BoneSlot.ShoulderRight,    "mixamorig:RightArm");
                boneMap.Add(AvatarControllerClassic.BoneSlot.ElbowRight,       "mixamorig:RightForeArm");
                boneMap.Add(AvatarControllerClassic.BoneSlot.HandRight,        "mixamorig:RightHand");
                boneMap.Add(AvatarControllerClassic.BoneSlot.FingersRight,     "mixamorig:RightHandIndex1");
                boneMap.Add(AvatarControllerClassic.BoneSlot.ThumbRight,       "mixamorig:RightHandThumb2");
                boneMap.Add(AvatarControllerClassic.BoneSlot.HipLeft,          "mixamorig:LeftUpLeg");
                boneMap.Add(AvatarControllerClassic.BoneSlot.KneeLeft,         "mixamorig:LeftLeg");
                boneMap.Add(AvatarControllerClassic.BoneSlot.FootLeft,         "mixamorig:LeftFoot");
                boneMap.Add(AvatarControllerClassic.BoneSlot.ToesLeft,         "mixamorig:LeftToeBase");
                boneMap.Add(AvatarControllerClassic.BoneSlot.HipRight,         "mixamorig:RightUpLeg");
                boneMap.Add(AvatarControllerClassic.BoneSlot.KneeRight,        "mixamorig:RightLeg");
                boneMap.Add(AvatarControllerClassic.BoneSlot.FootRight,        "mixamorig:RightFoot");
                boneMap.Add(AvatarControllerClassic.BoneSlot.ToesRight,        "mixamorig:RightToeBase");
                break;
            case AvatarBoneMapMode.KinectCustom:
                boneMap.Add(AvatarControllerClassic.BoneSlot.HipCenter,    "spine_base");
                boneMap.Add(AvatarControllerClassic.BoneSlot.Spine,        "spine_mid");
                boneMap.Add(AvatarControllerClassic.BoneSlot.ShoulderCenter,"spine_shoulder");
                boneMap.Add(AvatarControllerClassic.BoneSlot.Neck,         "neck");
                boneMap.Add(AvatarControllerClassic.BoneSlot.Head,         "head");
                boneMap.Add(AvatarControllerClassic.BoneSlot.ShoulderLeft, "shoulder_l");
                boneMap.Add(AvatarControllerClassic.BoneSlot.ElbowLeft,    "elbow_l");
                boneMap.Add(AvatarControllerClassic.BoneSlot.HandLeft,     "wrist_l");
                boneMap.Add(AvatarControllerClassic.BoneSlot.FingersLeft,  "hand_l");
                boneMap.Add(AvatarControllerClassic.BoneSlot.ThumbLeft,    "thumb_l");
                boneMap.Add(AvatarControllerClassic.BoneSlot.ShoulderRight,"shoulder_r");
                boneMap.Add(AvatarControllerClassic.BoneSlot.ElbowRight,   "elbow_r");
                boneMap.Add(AvatarControllerClassic.BoneSlot.HandRight,    "wrist_r");
                boneMap.Add(AvatarControllerClassic.BoneSlot.FingersRight, "hand_r");
                boneMap.Add(AvatarControllerClassic.BoneSlot.ThumbRight,   "thumb_r");
                boneMap.Add(AvatarControllerClassic.BoneSlot.HipLeft,      "hip_l");
                boneMap.Add(AvatarControllerClassic.BoneSlot.KneeLeft,     "knee_l");
                boneMap.Add(AvatarControllerClassic.BoneSlot.FootLeft,     "ankle_l");
                boneMap.Add(AvatarControllerClassic.BoneSlot.ToesLeft,     "foot_l");
                boneMap.Add(AvatarControllerClassic.BoneSlot.HipRight,     "hip_r");
                boneMap.Add(AvatarControllerClassic.BoneSlot.KneeRight,    "knee_r");
                boneMap.Add(AvatarControllerClassic.BoneSlot.FootRight,    "ankle_r");
                boneMap.Add(AvatarControllerClassic.BoneSlot.ToesRight,    "foot_r");
                break;
        }

        return boneMap;
    }

    internal void ToggleShowBody()
    {
        ShowBody = !ShowBody;
        transform.FindDeepChild("body")?.gameObject.SetActive(ShowBody);
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
        if (myScript.initialized)
        {
            if (GUILayout.Button("ToggleBody"))
                myScript.ToggleShowBody();
        }
    }

    void OnSceneGUI()
    {

        AvatarControllerBootstrap t = (AvatarControllerBootstrap)target;
        AvatarControllerClassic a = t.avatarController;
        if (a != null && a.HipCenter != null)
        {
            if (t.drawTunedPositions)
            {
                Handles.color = t.tunedColor;
                // These draw the shared bone mappings, they are the same across all AvatarBoneMapModes
                //Right Leg
                Handles.DrawLine(a.HipCenter.position, a.HipRight.position);
                Handles.DrawLine(a.HipRight.position, a.KneeRight.position);
                Handles.DrawLine(a.KneeRight.position, a.FootRight.position);
                Handles.DrawLine(a.FootRight.position, a.ToesRight.position);
                //Left Leg
                Handles.DrawLine(a.HipCenter.position, a.HipLeft.position);
                Handles.DrawLine(a.HipLeft.position, a.KneeLeft.position);
                Handles.DrawLine(a.KneeLeft.position, a.FootLeft.position);
                Handles.DrawLine(a.FootLeft.position, a.ToesLeft.position);
                // Right Arm
                Handles.DrawLine(a.ShoulderRight.position, a.ElbowRight.position);
                Handles.DrawLine(a.ElbowRight.position, a.HandRight.position);
                Handles.DrawLine(a.HandRight.position, a.FingersRight.position);
                // Left Arm
                Handles.DrawLine(a.ShoulderLeft.position, a.ElbowLeft.position);
                Handles.DrawLine(a.ElbowLeft.position, a.HandLeft.position);
                Handles.DrawLine(a.HandLeft.position, a.FingersLeft.position);

                // These draw the unshared bone mappings. For example, Mixamo has Clavicle bones, but
                // KinectCustom does not.
                switch (t.avatarBoneMapMode)
                {
                    case AvatarBoneMapMode.Mixamo:
                        // Spine to Head
                        Handles.DrawLine(a.HipCenter.position, a.Spine.position);
                        Handles.DrawLine(a.Spine.position, a.SpineMid.position);
                        Handles.DrawLine(a.SpineMid.position, a.ShoulderCenter.position);
                        Handles.DrawLine(a.ShoulderCenter.position, a.Neck.position);
                        Handles.DrawLine(a.Neck.position, a.Head.position);
                        // Right Arm
                        Handles.DrawLine(a.ShoulderCenter.position, a.ClavicleRight.position);
                        Handles.DrawLine(a.ClavicleRight.position, a.ShoulderRight.position);
                        // Left Arm
                        Handles.DrawLine(a.ShoulderCenter.position, a.ClavicleLeft.position);
                        Handles.DrawLine(a.ClavicleLeft.position, a.ShoulderLeft.position);
                        break;
                    case AvatarBoneMapMode.KinectCustom:
                        // Spine to Head
                        Handles.DrawLine(a.HipCenter.position, a.Spine.position);
                        Handles.DrawLine(a.Spine.position, a.ShoulderCenter.position);
                        Handles.DrawLine(a.ShoulderCenter.position, a.Neck.position);
                        Handles.DrawLine(a.Neck.position, a.Head.position);
                        // Right Arm
                        Handles.DrawLine(a.ShoulderCenter.position, a.ShoulderRight.position);
                        // Left Arm
                        Handles.DrawLine(a.ShoulderCenter.position, a.ShoulderLeft.position);
                        break;
                }
            }

            if (a.kinectManager != null)
            {
                if (t.drawRawPositions)
                {
                    Handles.color = t.rawColor;
                    float screenSpaceSize = 5f;
                    // Spine
                    Handles.DrawDottedLine(a.rawJointPos[KinectInterop.JointType.HipLeft], a.rawJointPos[KinectInterop.JointType.HipRight], screenSpaceSize);
                    Handles.DrawDottedLine(a.rawJointPos[KinectInterop.JointType.SpineBase], a.rawJointPos[KinectInterop.JointType.SpineMid], screenSpaceSize);
                    Handles.DrawDottedLine(a.rawJointPos[KinectInterop.JointType.SpineMid], a.rawJointPos[KinectInterop.JointType.SpineShoulder], screenSpaceSize);
                    Handles.DrawDottedLine(a.rawJointPos[KinectInterop.JointType.SpineShoulder], a.rawJointPos[KinectInterop.JointType.Neck], screenSpaceSize);
                    Handles.DrawDottedLine(a.rawJointPos[KinectInterop.JointType.Neck], a.rawJointPos[KinectInterop.JointType.Head], screenSpaceSize);

                    // Left Leg
                    Handles.DrawDottedLine(a.rawJointPos[KinectInterop.JointType.HipLeft], a.rawJointPos[KinectInterop.JointType.KneeLeft], screenSpaceSize);
                    Handles.DrawDottedLine(a.rawJointPos[KinectInterop.JointType.KneeLeft], a.rawJointPos[KinectInterop.JointType.AnkleLeft], screenSpaceSize);
                    Handles.DrawDottedLine(a.rawJointPos[KinectInterop.JointType.AnkleLeft], a.rawJointPos[KinectInterop.JointType.FootLeft], screenSpaceSize);
                    // Right Leg
                    Handles.DrawDottedLine(a.rawJointPos[KinectInterop.JointType.HipRight], a.rawJointPos[KinectInterop.JointType.KneeRight], screenSpaceSize);
                    Handles.DrawDottedLine(a.rawJointPos[KinectInterop.JointType.KneeRight], a.rawJointPos[KinectInterop.JointType.AnkleRight], screenSpaceSize);
                    Handles.DrawDottedLine(a.rawJointPos[KinectInterop.JointType.AnkleRight], a.rawJointPos[KinectInterop.JointType.FootRight], screenSpaceSize);
                    // Left Arm
                    Handles.DrawDottedLine(a.rawJointPos[KinectInterop.JointType.SpineShoulder], a.rawJointPos[KinectInterop.JointType.ShoulderLeft], screenSpaceSize);
                    Handles.DrawDottedLine(a.rawJointPos[KinectInterop.JointType.ShoulderLeft], a.rawJointPos[KinectInterop.JointType.ElbowLeft], screenSpaceSize);
                    Handles.DrawDottedLine(a.rawJointPos[KinectInterop.JointType.ElbowLeft], a.rawJointPos[KinectInterop.JointType.WristLeft], screenSpaceSize);
                    Handles.DrawDottedLine(a.rawJointPos[KinectInterop.JointType.WristLeft], a.rawJointPos[KinectInterop.JointType.HandLeft], screenSpaceSize);
                    // Right Arm
                    Handles.DrawDottedLine(a.rawJointPos[KinectInterop.JointType.SpineShoulder], a.rawJointPos[KinectInterop.JointType.ShoulderRight], screenSpaceSize);
                    Handles.DrawDottedLine(a.rawJointPos[KinectInterop.JointType.ShoulderRight], a.rawJointPos[KinectInterop.JointType.ElbowRight], screenSpaceSize);
                    Handles.DrawDottedLine(a.rawJointPos[KinectInterop.JointType.ElbowRight], a.rawJointPos[KinectInterop.JointType.WristRight], screenSpaceSize);
                    Handles.DrawDottedLine(a.rawJointPos[KinectInterop.JointType.WristRight], a.rawJointPos[KinectInterop.JointType.HandRight], screenSpaceSize);

                }

                //if (t.drawCurrentRawPositions)
                //{
                //    // Draw raw joint positions
                //    Handles.color = t.currentRawColor;
                //    float screenSpaceSize = 5f;
                //    // Spine
                //    Handles.DrawDottedLine(a.GetRawJointWorldPos(KinectInterop.JointType.HipLeft), a.GetRawJointWorldPos(KinectInterop.JointType.HipRight), screenSpaceSize);
                //    Handles.DrawDottedLine(a.GetRawJointWorldPos(KinectInterop.JointType.SpineBase), a.GetRawJointWorldPos(KinectInterop.JointType.SpineMid), screenSpaceSize);
                //    Handles.DrawDottedLine(a.GetRawJointWorldPos(KinectInterop.JointType.SpineMid), a.GetRawJointWorldPos(KinectInterop.JointType.SpineShoulder), screenSpaceSize);
                //    Handles.DrawDottedLine(a.GetRawJointWorldPos(KinectInterop.JointType.SpineShoulder), a.GetRawJointWorldPos(KinectInterop.JointType.Neck), screenSpaceSize);
                //    Handles.DrawDottedLine(a.GetRawJointWorldPos(KinectInterop.JointType.Neck), a.GetRawJointWorldPos(KinectInterop.JointType.Head), screenSpaceSize);

                //    // Left Leg 
                //    Handles.DrawDottedLine(a.GetRawJointWorldPos(KinectInterop.JointType.HipLeft), a.GetRawJointWorldPos(KinectInterop.JointType.KneeLeft), screenSpaceSize);
                //    Handles.DrawDottedLine(a.GetRawJointWorldPos(KinectInterop.JointType.KneeLeft), a.GetRawJointWorldPos(KinectInterop.JointType.AnkleLeft), screenSpaceSize);
                //    Handles.DrawDottedLine(a.GetRawJointWorldPos(KinectInterop.JointType.AnkleLeft), a.GetRawJointWorldPos(KinectInterop.JointType.FootLeft), screenSpaceSize);
                //    // Right Leg
                //    Handles.DrawDottedLine(a.GetRawJointWorldPos(KinectInterop.JointType.HipRight), a.GetRawJointWorldPos(KinectInterop.JointType.KneeRight), screenSpaceSize);
                //    Handles.DrawDottedLine(a.GetRawJointWorldPos(KinectInterop.JointType.KneeRight), a.GetRawJointWorldPos(KinectInterop.JointType.AnkleRight), screenSpaceSize);
                //    Handles.DrawDottedLine(a.GetRawJointWorldPos(KinectInterop.JointType.AnkleRight), a.GetRawJointWorldPos(KinectInterop.JointType.FootRight), screenSpaceSize);
                //    // Left Arm
                //    Handles.DrawDottedLine(a.GetRawJointWorldPos(KinectInterop.JointType.SpineShoulder), a.GetRawJointWorldPos(KinectInterop.JointType.ShoulderLeft), screenSpaceSize);
                //    Handles.DrawDottedLine(a.GetRawJointWorldPos(KinectInterop.JointType.ShoulderLeft), a.GetRawJointWorldPos(KinectInterop.JointType.ElbowLeft), screenSpaceSize);
                //    Handles.DrawDottedLine(a.GetRawJointWorldPos(KinectInterop.JointType.ElbowLeft), a.GetRawJointWorldPos(KinectInterop.JointType.WristLeft), screenSpaceSize);
                //    Handles.DrawDottedLine(a.GetRawJointWorldPos(KinectInterop.JointType.WristLeft), a.GetRawJointWorldPos(KinectInterop.JointType.HandLeft), screenSpaceSize);
                //    // Right Arm
                //    Handles.DrawDottedLine(a.GetRawJointWorldPos(KinectInterop.JointType.SpineShoulder), a.GetRawJointWorldPos(KinectInterop.JointType.ShoulderRight), screenSpaceSize);
                //    Handles.DrawDottedLine(a.GetRawJointWorldPos(KinectInterop.JointType.ShoulderRight), a.GetRawJointWorldPos(KinectInterop.JointType.ElbowRight), screenSpaceSize);
                //    Handles.DrawDottedLine(a.GetRawJointWorldPos(KinectInterop.JointType.ElbowRight), a.GetRawJointWorldPos(KinectInterop.JointType.WristRight), screenSpaceSize);
                //    Handles.DrawDottedLine(a.GetRawJointWorldPos(KinectInterop.JointType.WristRight), a.GetRawJointWorldPos(KinectInterop.JointType.HandRight), screenSpaceSize);
                //}
            }

        }
    }
}
#endif