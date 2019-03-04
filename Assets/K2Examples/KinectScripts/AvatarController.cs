using UnityEngine;
//using Windows.Kinect;

using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// Avatar controller is the component that transfers the captured user motion to a humanoid model (avatar).
/// </summary>
[RequireComponent(typeof(Animator))]
public class AvatarController : MonoBehaviour
{
    [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
    public int playerIndex = 0;

    [Tooltip("Whether the avatar is facing the player or not.")]
    public bool mirroredMovement = false;

    [Tooltip("Whether the avatar is allowed to move vertically or not.")]
    public bool verticalMovement = false;

    [Tooltip("Whether the avatar's root motion is applied by other component or script.")]
    public bool externalRootMotion = false;

    [Tooltip("Whether the head rotation is controlled externally (e.g. by VR-headset).")]
    public bool externalHeadRotation = false;

    [Tooltip("Whether the hand and finger rotations are controlled externally (e.g. by LeapMotion controller)")]
    public bool externalHandRotations = false;

    [Tooltip("Whether the finger orientations are allowed or not.")]
    public bool fingerOrientations = false;

    [Tooltip("Rate at which the avatar will move through the scene.")]
    public float moveRate = 1f;

    [Tooltip("Smooth factor used for avatar movements and joint rotations.")]
    public float smoothFactor = 10f;

    [Tooltip("Game object this transform is relative to (optional).")]
    public GameObject offsetNode;

    [Tooltip("If enabled, makes the avatar position relative to this camera to be the same as the player's position to the sensor.")]
    public Camera posRelativeToCamera;

    [Tooltip("Whether the avatar's position should match the color image (in Pos-rel-to-camera mode only).")]
    public bool posRelOverlayColor = false;

    [Tooltip("Plane used to render the color camera background to overlay.")]
    public Transform backgroundPlane;

    [Tooltip("Whether z-axis movement needs to be inverted (Pos-Relative mode only).")]
    public bool posRelInvertedZ = false;

    [Tooltip("Whether the avatar's feet must stick to the ground.")]
    public bool groundedFeet = false;

    [Tooltip("Whether to apply the humanoid model's muscle limits or not.")]
    public bool applyMuscleLimits = false;

    [Tooltip("Whether to flip left and right, relative to the sensor.")]
    public bool flipLeftRight = false;


    [Tooltip("Vertical offset of the avatar with respect to the position of user's spine-base.")]
    [Range(-0.5f, 0.5f)]
    public float verticalOffset = 0f;

    [Tooltip("Forward offset of the avatar with respect to the position of user's spine-base.")]
    [Range(-0.5f, 0.5f)]
    public float forwardOffset = 0f;

    //[Tooltip("Forward offset of the avatar with respect to the position of user's spine-base.")]
    //[Range(-2.0f, 2.0f)]
    //public float shoulderUnrotateFactor = 1.0f;

    // userId of the player
    [NonSerialized]
    public Int64 playerId = 0;


    // The body root node
    protected Transform bodyRoot;

    // Variable to hold all them bones. It will initialize the same size as initialRotations.
    protected Transform[] bones;
    protected Transform[] fingerBones;

    // Rotations of the bones when the Kinect tracking starts.
    protected Quaternion[] initialRotations;
    protected Quaternion[] localRotations;
    protected bool[] isBoneDisabled;
    protected Vector3[] initialPositions;
    protected Vector3[] initialLocalPositions;

    // Local rotations of finger bones
    protected Dictionary<HumanBodyBones, Quaternion> fingerBoneLocalRotations = new Dictionary<HumanBodyBones, Quaternion>();
    protected Dictionary<HumanBodyBones, Vector3> fingerBoneLocalAxes = new Dictionary<HumanBodyBones, Vector3>();

    // Initial position and rotation of the transform
    protected Vector3 initialPosition;
    protected Quaternion initialRotation;
    protected Vector3 initialHipsPosition;
    protected Quaternion initialHipsRotation;
    protected Vector3 initialKneeLeftPosition;

    protected Vector3 offsetNodePos;
    protected Quaternion offsetNodeRot;
    protected Vector3 bodyRootPosition;

    [Range(-0.25f, 0.25f)]
    public float spineVerticalOffset = 0f;
    [Range(-0.25f, 0.25f)]
    public float shoulderCenterVerticalOffset = 0f; //Compensate for when shoulderCenter is not actually (vertically) at same height as left and right arm sockets.
    [Range(-0.25f, 0.25f)]
    public float neckVerticalOffset = 0f;
    [Range(-0.25f, 0.25f)]
    public float headVerticalOffset = 0f; //Compensate for head is not actually
    internal float hipWidthFactor = 0.0f; // These are automatically computed by comparing kinect postions to the model's initial positions
    internal float shoulderWidthFactor = 0.0f; // These are automatically computed by comparing kinect postions to the model's initial positions
    [Range(0.5f, 2.0f)]
    public float shoulderAdjustWidthFactor = 1.0f; // Sometimes, the model will produce very wide or narrow shoulderWidthFactors due to their initial shoulder widths. Use this to adjsut
    [Range(0.5f, 2.0f)]
    public float hipAdjustWidthFactor = 1.0f; // Sometimes, the model will produce very wide or narrow shoulderWidthFactors due to their initial shoulder widths. Use this to adjsut
    [Range(0.5f, 2.0f)]
    public float hipZFactor = 1.0f;
    [Range(0f, 1.0f)]
    public float hipUpwardsFactor = 0.1f;
    //public float shoulderAngleRange = 1f;

    float initialShoulderWidth = 0;
    float initialHipWidth = 0;
    float initialTorsoHeight = 0;

    float lastShoulderAdjustWidthFactor = 0.0f;
    float lastHipAdjustWidthFactor = 0.0f;

    bool lastArmsRaised = false;
    bool needTuningReset = false;

    public MeshRenderer debugTuningObj = null;

    // Calibration Offset Variables for Character Position.
    [NonSerialized]
    public bool offsetCalibrated = false;
    protected Vector3 offsetPos = Vector3.zero;
    //protected float xOffset, yOffset, zOffset;
    //private Quaternion originalRotation;

    private Animator animatorComponent = null;
    private HumanPoseHandler humanPoseHandler = null;
    private HumanPose humanPose = new HumanPose();

    // whether the parent transform obeys physics
    protected bool isRigidBody = false;

    // private instance of the KinectManager
    protected KinectManager kinectManager;
    protected AvatarScaler avatarScaler;

    // last hand events
    private InteractionManager.HandEventType lastLeftHandEvent = InteractionManager.HandEventType.Release;
    private InteractionManager.HandEventType lastRightHandEvent = InteractionManager.HandEventType.Release;

    // fist states
    private bool bLeftFistDone = false;
    private bool bRightFistDone = false;

    // grounder constants and variables
    private const int raycastLayers = ~2;  // Ignore Raycast
    private const float maxFootDistanceGround = 0.02f;  // maximum distance from lower foot to the ground
    private const float maxFootDistanceTime = 0.2f; // 1.0f;  // maximum allowed time, the lower foot to be distant from the ground
    private Transform leftFoot, rightFoot;

    private float fFootDistanceInitial = 0f;
    private float fFootDistance = 0f;
    private float fFootDistanceTime = 0f;

    // background plane rectangle
    private Rect planeRect = new Rect();
    private bool planeRectSet = false;


    /// <summary>
    /// Gets the number of bone transforms (array length).
    /// </summary>
    /// <returns>The number of bone transforms.</returns>
    public int GetBoneTransformCount()
    {
        return bones != null ? bones.Length : 0;
    }

    /// <summary>
    /// Gets the bone transform by index.
    /// </summary>
    /// <returns>The bone transform.</returns>
    /// <param name="index">Index</param>
    public Transform GetBoneTransform(int index)
    {
        if (index >= 0 && index < bones.Length)
        {
            return bones[index];
        }

        return null;
    }

    /// <summary>
    /// Get joint position with respect of player world and kinect offsets   ( //!!still some problems with accurate Y pos, probably connected with kinect sensor height estimation ) 
    /// </summary>
    /// <param name="jointType"></param>
    /// <returns></returns>
    public Vector3 GetJointWorldPos(KinectInterop.JointType jointType)
    {
        if (!kinectManager)
        {
            return Vector3.zero;
        }

        Vector3 jointPosition = kinectManager.GetJointPosition(playerId, (int)jointType);
        Vector3 worldPosition = new Vector3(
            jointPosition.x - offsetPos.x,
            //            jointPosition.y - offsetPos.y + kinectManager.sensorHeight,  //!! this should be better investigated .. 
            jointPosition.y + offsetPos.y - kinectManager.sensorHeight,  //!! this workds better on my example 
            !mirroredMovement && !posRelativeToCamera ? (-jointPosition.z - offsetPos.z) : (jointPosition.z - offsetPos.z));

        Quaternion posRotation = mirroredMovement ? Quaternion.Euler(0f, 180f, 0f) * initialRotation : initialRotation;
        worldPosition = posRotation * worldPosition;

        return bodyRootPosition + worldPosition;
    }

    public Vector3 GetRawJointWorldPos(KinectInterop.JointType jointType)
    {
        return kinectManager.GetJointPosition(playerId, (int)jointType);
    }

    /// <summary>
    /// Disables the bone and optionally resets its orientation.
    /// </summary>
    /// <param name="index">Bone index.</param>
    /// <param name="resetBone">If set to <c>true</c> resets bone orientation.</param>
    public void DisableBone(int index, bool resetBone)
    {
        if (index >= 0 && index < bones.Length)
        {
            isBoneDisabled[index] = true;

            if (resetBone && bones[index] != null)
            {
                bones[index].rotation = localRotations[index];
            }
        }
    }

    /// <summary>
    /// Enables the bone, so AvatarController could update its orientation.
    /// </summary>
    /// <param name="index">Bone index.</param>
    public void EnableBone(int index)
    {
        if (index >= 0 && index < bones.Length)
        {
            isBoneDisabled[index] = false;
        }
    }

    /// <summary>
    /// Determines whether the bone orientation update is enabled or not.
    /// </summary>
    /// <returns><c>true</c> if the bone update is enabled; otherwise, <c>false</c>.</returns>
    /// <param name="index">Bone index.</param>
    public bool IsBoneEnabled(int index)
    {
        if (index >= 0 && index < bones.Length)
        {
            return !isBoneDisabled[index];
        }

        return false;
    }

    /// <summary>
    /// Gets the bone index by joint type.
    /// </summary>
    /// <returns>The bone index.</returns>
    /// <param name="joint">Joint type</param>
    /// <param name="bMirrored">If set to <c>true</c> gets the mirrored joint index.</param>
    public int GetBoneIndexByJoint(KinectInterop.JointType joint, bool bMirrored)
    {
        int boneIndex = -1;

        if (jointMap2boneIndex.ContainsKey(joint))
        {
            boneIndex = !bMirrored ? jointMap2boneIndex[joint] : mirrorJointMap2boneIndex[joint];
        }

        return boneIndex;
    }

    /// <summary>
    /// Gets the special index by two joint types.
    /// </summary>
    /// <returns>The spec index by joint.</returns>
    /// <param name="joint1">Joint 1 type.</param>
    /// <param name="joint2">Joint 2 type.</param>
    /// <param name="bMirrored">If set to <c>true</c> gets the mirrored joint index.</param>
    public int GetSpecIndexByJoint(KinectInterop.JointType joint1, KinectInterop.JointType joint2, bool bMirrored)
    {
        int boneIndex = -1;

        if ((joint1 == KinectInterop.JointType.ShoulderLeft && joint2 == KinectInterop.JointType.SpineShoulder) ||
           (joint2 == KinectInterop.JointType.ShoulderLeft && joint1 == KinectInterop.JointType.SpineShoulder))
        {
            return (!bMirrored ? 25 : 26);
        }
        else if ((joint1 == KinectInterop.JointType.ShoulderRight && joint2 == KinectInterop.JointType.SpineShoulder) ||
                (joint2 == KinectInterop.JointType.ShoulderRight && joint1 == KinectInterop.JointType.SpineShoulder))
        {
            return (!bMirrored ? 26 : 25);
        }
        else if ((joint1 == KinectInterop.JointType.HandTipLeft && joint2 == KinectInterop.JointType.HandLeft) ||
                (joint2 == KinectInterop.JointType.HandTipLeft && joint1 == KinectInterop.JointType.HandLeft))
        {
            return (!bMirrored ? 27 : 28);
        }
        else if ((joint1 == KinectInterop.JointType.HandTipRight && joint2 == KinectInterop.JointType.HandRight) ||
                (joint2 == KinectInterop.JointType.HandTipRight && joint1 == KinectInterop.JointType.HandRight))
        {
            return (!bMirrored ? 28 : 27);
        }
        else if ((joint1 == KinectInterop.JointType.ThumbLeft && joint2 == KinectInterop.JointType.HandLeft) ||
                (joint2 == KinectInterop.JointType.ThumbLeft && joint1 == KinectInterop.JointType.HandLeft))
        {
            return (!bMirrored ? 29 : 30);
        }
        else if ((joint1 == KinectInterop.JointType.ThumbRight && joint2 == KinectInterop.JointType.HandRight) ||
                (joint2 == KinectInterop.JointType.ThumbRight && joint1 == KinectInterop.JointType.HandRight))
        {
            return (!bMirrored ? 30 : 29);
        }

        return boneIndex;
    }

    /// <summary>
    /// Gets the number of finger bone transforms (array length).
    /// </summary>
    /// <returns>The number of finger bone transforms.</returns>
    public int GetFingerTransformCount()
    {
        return fingerBones != null ? fingerBones.Length : 0;
    }

    /// <summary>
    /// Gets the finger bone transform by index.
    /// </summary>
    /// <returns>The finger bone transform.</returns>
    /// <param name="index">Index</param>
    public Transform GetFingerTransform(int index)
    {
        if (index >= 0 && index < fingerBones.Length)
        {
            return fingerBones[index];
        }

        return null;
    }


    // transform caching gives performance boost since Unity calls GetComponent<Transform>() each time you call transform 
    private Transform _transformCache;
    public new Transform transform
    {
        get
        {
            if (!_transformCache)
            {
                _transformCache = base.transform;
            }

            return _transformCache;
        }
    }


    public void Awake()
    {
        // Clean up the name in case this is a (Clone) object.
        // This is important for loading tuning data files later.
        string cloneStr = "(Clone)";
        int cloneIdx = this.name.IndexOf(cloneStr);
        if (cloneIdx >= 0)
            this.name = this.name.Remove(cloneIdx);

        // check for double start
        if (bones != null)
            return;
        if (!gameObject.activeInHierarchy)
            return;

        // inits the bones array
        bones = new Transform[31];

        // get the animator reference
        animatorComponent = GetComponent<Animator>();

        // Map bones to the points the Kinect tracks
        MapBones();

        // Set model's arms to be in T-pose, if needed
        SetModelArmsInTpose();
        SetModelLegsVertical();

        // Initial rotations and directions of the bones.
        initialRotations = new Quaternion[bones.Length];
        localRotations = new Quaternion[bones.Length];
        isBoneDisabled = new bool[bones.Length];
        initialPositions = new Vector3[bones.Length];
        initialLocalPositions = new Vector3[bones.Length];

        // Get initial bone rotations
        GetInitialRotations();

        // enable all bones
        for (int i = 0; i < bones.Length; i++)
        {
            isBoneDisabled[i] = false;
        }

        // get initial distance to ground
        fFootDistanceInitial = GetDistanceToGround();
        fFootDistance = 0f;
        fFootDistanceTime = 0f;

        // if parent transform uses physics
        isRigidBody = (gameObject.GetComponent<Rigidbody>() != null);

        // get the pose handler reference
        if (animatorComponent && animatorComponent.avatar && animatorComponent.avatar.isHuman)
        {
            //Transform hipsTransform = animator.GetBoneTransform(HumanBodyBones.Hips);
            //Transform rootTransform = hipsTransform.parent;
            Transform rootTransform = transform;

            humanPoseHandler = new HumanPoseHandler(animatorComponent.avatar, rootTransform);
            humanPoseHandler.GetHumanPose(ref humanPose);

            initialHipsPosition = (humanPose.bodyPosition - rootTransform.position);  // hipsTransform.position
            initialHipsRotation = humanPose.bodyRotation;
        }

        avatarScaler = GetComponent<AvatarScaler>();

        debugTuningObj = GameObject.Find("DebugTuningObj")?.GetComponent<MeshRenderer>();
    }


    // applies the muscle limits for humanoid avatar
    private void CheckMuscleLimits()
    {
        if (humanPoseHandler == null)
            return;

        humanPoseHandler.GetHumanPose(ref humanPose);

        Debug.Log(playerId + " - Trans: " + transform.position + ", body: " + humanPose.bodyPosition);

        bool isPoseChanged = false;

        float muscleMin = -1f;
        float muscleMax = 1f;

        for (int i = 0; i < humanPose.muscles.Length; i++)
        {
            if (float.IsNaN(humanPose.muscles[i]))
            {
                //humanPose.muscles[i] = 0f;
                continue;
            }

            if (humanPose.muscles[i] < muscleMin)
            {
                humanPose.muscles[i] = muscleMin;
                isPoseChanged = true;
            }
            else if (humanPose.muscles[i] > muscleMax)
            {
                humanPose.muscles[i] = muscleMax;
                isPoseChanged = true;
            }
        }

        if (isPoseChanged)
        {
            //Quaternion localBodyRot = Quaternion.Inverse(transform.rotation) * humanPose.bodyRotation;
            Quaternion localBodyRot = Quaternion.Inverse(initialHipsRotation) * humanPose.bodyRotation;

            // recover the body position & orientation
            //humanPose.bodyPosition = Vector3.zero;
            //humanPose.bodyPosition.y = initialHipsPosition.y;
            humanPose.bodyPosition = initialHipsPosition;
            humanPose.bodyRotation = localBodyRot; // Quaternion.identity;

            humanPoseHandler.SetHumanPose(ref humanPose);
            //Debug.Log("  Human pose updated.");
        }

    }


    /// <summary>
    /// Updates the avatar each frame.
    /// </summary>
    /// <param name="UserID">User ID</param>
    public void UpdateAvatar(Int64 UserID)
    {
        if (!gameObject.activeInHierarchy)
            return;

        // Get the KinectManager instance
        if (kinectManager == null)
        {
            kinectManager = KinectManager.Instance;
        }

        // get the background plane rectangle if needed 
        if (backgroundPlane && !planeRectSet && kinectManager && kinectManager.IsInitialized())
        {
            planeRectSet = true;

            planeRect.width = 10f * Mathf.Abs(backgroundPlane.localScale.x);
            planeRect.height = 10f * Mathf.Abs(backgroundPlane.localScale.z);
            planeRect.x = backgroundPlane.position.x - planeRect.width / 2f;
            planeRect.y = backgroundPlane.position.y - planeRect.height / 2f;
        }

        // move the avatar to its Kinect position
        if (!externalRootMotion)
        {
            if (!MoveAvatar(UserID))
            {
                return;
            }
        }

        // get the left hand state and event
        if (kinectManager && kinectManager.GetJointTrackingState(UserID, (int)KinectInterop.JointType.HandLeft) != KinectInterop.TrackingState.NotTracked)
        {
            KinectInterop.HandState leftHandState = kinectManager.GetLeftHandState(UserID);
            InteractionManager.HandEventType leftHandEvent = InteractionManager.HandStateToEvent(leftHandState, lastLeftHandEvent);

            if (leftHandEvent != InteractionManager.HandEventType.None)
            {
                lastLeftHandEvent = leftHandEvent;
            }
        }

        // get the right hand state and event
        if (kinectManager && kinectManager.GetJointTrackingState(UserID, (int)KinectInterop.JointType.HandRight) != KinectInterop.TrackingState.NotTracked)
        {
            KinectInterop.HandState rightHandState = kinectManager.GetRightHandState(UserID);
            InteractionManager.HandEventType rightHandEvent = InteractionManager.HandStateToEvent(rightHandState, lastRightHandEvent);

            if (rightHandEvent != InteractionManager.HandEventType.None)
            {
                lastRightHandEvent = rightHandEvent;
            }
        }

        CalibrateHipShoulders();

        // move the bones
        translatedBoneTransforms.Clear();
        for (var boneIndex = 0; boneIndex < bones.Length; boneIndex++)
        {
            if (!bones[boneIndex] || isBoneDisabled[boneIndex])
                continue;

            if (boneIndex2JointMap.ContainsKey(boneIndex))
            {
                KinectInterop.JointType joint = !(mirroredMovement ^ flipLeftRight) ?
                    boneIndex2JointMap[boneIndex] : boneIndex2MirrorJointMap[boneIndex];

                TranslateBone(UserID, joint, boneIndex, !(mirroredMovement ^ flipLeftRight));
            }
        }

        // rotate the avatar bones
        for (var boneIndex = 0; boneIndex < bones.Length; boneIndex++)
        {
            if (!bones[boneIndex] || isBoneDisabled[boneIndex])
                continue;

            if (boneIndex2JointMap.ContainsKey(boneIndex))
            {
                KinectInterop.JointType joint = !(mirroredMovement ^ flipLeftRight) ?
                    boneIndex2JointMap[boneIndex] : boneIndex2MirrorJointMap[boneIndex];

                if (externalHeadRotation && joint == KinectInterop.JointType.Head)   // skip head if moved externally
                {
                    continue;
                }

                if (externalHandRotations &&    // skip hands if moved externally
                    (joint == KinectInterop.JointType.WristLeft || joint == KinectInterop.JointType.WristRight ||
                        joint == KinectInterop.JointType.HandLeft || joint == KinectInterop.JointType.HandRight))
                {
                    continue;
                }

                TransformBone(UserID, joint, boneIndex, !(mirroredMovement ^ flipLeftRight));
            }
            else if (specIndex2JointMap.ContainsKey(boneIndex))
            {
                // special bones (clavicles)
                List<KinectInterop.JointType> alJoints = !(mirroredMovement ^ flipLeftRight) ?
                    specIndex2JointMap[boneIndex] : specIndex2MirrorMap[boneIndex];

                if (alJoints.Count >= 2)
                {
                    //Debug.Log(alJoints[0].ToString());
                    Vector3 baseDir = alJoints[0].ToString().EndsWith("Left") ? Vector3.left : Vector3.right;
                    TransformSpecialBone(UserID, alJoints[0], alJoints[1], boneIndex, baseDir, !(mirroredMovement ^ flipLeftRight));
                }
            }
        }

        for (var boneIndex = 0; boneIndex < bones.Length; boneIndex++)
        {
            if (!bones[boneIndex] || isBoneDisabled[boneIndex])
                continue;

            if (boneIndex2JointMap.ContainsKey(boneIndex))
            {
                KinectInterop.JointType joint = !(mirroredMovement ^ flipLeftRight) ?
                    boneIndex2JointMap[boneIndex] : boneIndex2MirrorJointMap[boneIndex];

                PostRotateBone(UserID, joint, boneIndex, !(mirroredMovement ^ flipLeftRight));
            }
        }

        ScaleTorso();

        for (var boneIndex = 0; boneIndex < bones.Length; boneIndex++)
        {
            if (!bones[boneIndex] || isBoneDisabled[boneIndex])
                continue;

            if (boneIndex2JointMap.ContainsKey(boneIndex))
            {
                KinectInterop.JointType joint = !(mirroredMovement ^ flipLeftRight) ?
                    boneIndex2JointMap[boneIndex] : boneIndex2MirrorJointMap[boneIndex];

                ScaleBone(UserID, joint, boneIndex, !(mirroredMovement ^ flipLeftRight));
            }
        }
        //if (applyMuscleLimits && kinectManager && kinectManager.IsUserTracked(UserID)) 
        //{
        //	// check for limits
        //	CheckMuscleLimits();
        //}
    }

    protected void CalibrateHipShoulders()
    {
        // Determine hipWidthFactor, shoulderWidthFactor, torseHeightFactor
        if (hipAdjustWidthFactor != lastHipAdjustWidthFactor)
        {
            lastHipAdjustWidthFactor = hipAdjustWidthFactor;
            hipWidthFactor = 0.0f; // reset hipWidthFactor for compute
        }

        if (shoulderAdjustWidthFactor != lastShoulderAdjustWidthFactor)
        {
            lastShoulderAdjustWidthFactor = shoulderAdjustWidthFactor;
            shoulderWidthFactor = 0.0f; // reset shoulderWidthFactor for compute.
        }

        Vector3 shoulderLeft = GetRawJointWorldPos(KinectInterop.JointType.ShoulderLeft);
        Vector3 shoulderRight = GetRawJointWorldPos(KinectInterop.JointType.ShoulderRight);
        Vector3 elbowLeft = GetRawJointWorldPos(KinectInterop.JointType.ElbowLeft);
        Vector3 elbowRight = GetRawJointWorldPos(KinectInterop.JointType.ElbowRight);
        Transform spineTransform = bones[jointMap2boneIndex[KinectInterop.JointType.SpineMid]];
        bool leftArmRaised = spineTransform.InverseTransformPoint(elbowLeft).y > spineTransform.InverseTransformPoint(shoulderLeft).y;
        bool rightArmRaised = spineTransform.InverseTransformPoint(elbowRight).y > spineTransform.InverseTransformPoint(shoulderRight).y;
        bool armsRaised = leftArmRaised || rightArmRaised;
        //if (armsRaised)
        //    Debug.Log("Arms Raised @ " + Time.time);
        if (armsRaised == false && lastArmsRaised == true)
            needTuningReset = true;

        lastArmsRaised = armsRaised;

        float facingCamera = Vector3.Dot(spineTransform.forward.normalized, -posRelativeToCamera.transform.forward.normalized);
        //Debug.Log("facingCamera = " + facingCamera);

        Vector3 hipLeft = GetRawJointWorldPos(KinectInterop.JointType.HipLeft);
        Vector3 hipRight = GetRawJointWorldPos(KinectInterop.JointType.HipRight);
        float hipWidth = (hipLeft - hipRight).magnitude;
        float currHipWidthFactor = hipWidth / initialHipWidth;
        //Debug.Log("hipWidth = " + hipWidth);
        //Debug.Log("hipWidthFactor = " + currHipWidthFactor);
        float shoulderWidth = (shoulderLeft - shoulderRight).magnitude;
        float currShoulderWidthFactor = shoulderWidth / initialShoulderWidth;
        //Debug.Log("shoulderWidth = " + shoulderWidth);
        //Debug.Log("shoulderWidthFactor = " + shoulderWidth / initialShoulderWidth);
        float torsoHeight = (Vector3.Lerp(shoulderLeft, shoulderRight, 0.5f) - Vector3.Lerp(hipLeft, hipRight, 0.5f)).magnitude;
        //Debug.Log("torsoHeight = " + torsoHeight);
        //Debug.Log("torsoHeightFactor = " + torsoHeight / initialTorsoHeight);
        float camToUserDistSqrd = (spineTransform.position - posRelativeToCamera.transform.position).sqrMagnitude;
        float minDistSqrd = 1f; //1 meter
        float maxDistSqrd = 16f; // 4 meters

        if ((leftArmRaised && rightArmRaised) == false &&// Raised arms creates distortion
            facingCamera > 0.9f && // rotating shoulders away from camera creates distortion also
            (camToUserDistSqrd > minDistSqrd && camToUserDistSqrd < maxDistSqrd)) // distance can create inaccurate measurements
        {
            if (debugTuningObj != null)
            {
                debugTuningObj.material.color = Color.green;
            }
            // Only reset tuning values when we get good values with which to replace.
            if (needTuningReset)
            {
                hipWidthFactor = 0.0f; // reset hipWidthFactor for compute
                shoulderWidthFactor = 0.0f; // reset shoulderWidthFactor for compute.
                needTuningReset = false;
            }
            if (currHipWidthFactor > hipWidthFactor)
                hipWidthFactor = currHipWidthFactor * hipAdjustWidthFactor;
            if (currShoulderWidthFactor > shoulderWidthFactor)
                shoulderWidthFactor = currShoulderWidthFactor * shoulderAdjustWidthFactor;
        }
        else
        {
            if (debugTuningObj != null)
            {
                debugTuningObj.material.color = Color.red;
            }
        }
    }

    protected virtual void ScaleTorso()
    {
        // Do nothing here. Only AvatarControllerClassic has all the necessary spine points to scale torso properly.
    }

    /// <summary>
    /// Resets bones to their initial positions and rotations. This also releases avatar control from KM, by settings playerId to 0 
    /// </summary>
    public virtual void ResetToInitialPosition()
    {
        playerId = 0;

        if (bones == null)
            return;

        // For each bone that was defined, reset to initial position.
        transform.rotation = Quaternion.identity;

        for (int pass = 0; pass < 2; pass++)  // 2 passes because clavicles are at the end
        {
            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i] != null)
                {
                    bones[i].rotation = initialRotations[i];
                }
            }
        }

        // reset finger bones to initial position
        //Animator animatorComponent = GetComponent<Animator>();
        foreach (HumanBodyBones bone in fingerBoneLocalRotations.Keys)
        {
            Transform boneTransform = animatorComponent ? animatorComponent.GetBoneTransform(bone) : null;

            if (boneTransform)
            {
                boneTransform.localRotation = fingerBoneLocalRotations[bone];
            }
        }

        //		if(bodyRoot != null)
        //		{
        //			bodyRoot.localPosition = Vector3.zero;
        //			bodyRoot.localRotation = Quaternion.identity;
        //		}

        // Restore the offset's position and rotation
        if (offsetNode != null)
        {
            offsetNode.transform.position = offsetNodePos;
            offsetNode.transform.rotation = offsetNodeRot;
        }

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        //		if (bones[0]) 
        //		{
        //			bones[0].localPosition = initialHipsPosition;
        //			bones[0].localRotation = initialHipsRotation;
        //		}
    }

    /// <summary>
    /// Invoked on the successful calibration of the player.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    public virtual void SuccessfulCalibration(Int64 userId, bool resetInitialTransform)
    {
        playerId = userId;

        // reset the models position
        if (offsetNode != null)
        {
            offsetNode.transform.position = offsetNodePos;
            offsetNode.transform.rotation = offsetNodeRot;
        }

        // reset initial position / rotation if needed 
        if (resetInitialTransform)
        {
            bodyRootPosition = transform.position;
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        //		// enable all bones
        //		for(int i = 0; i < bones.Length; i++)
        //		{
        //			isBoneDisabled[i] = false;
        //		}

        // re-calibrate the position offset
        offsetCalibrated = false;
    }

    /// <summary>
    /// Moves the avatar to its initial/base position 
    /// </summary>
    /// <param name="position"> world position </param>
    /// <param name="rotation"> rotation offset </param>
    public void resetInitialTransform(Vector3 position, Vector3 rotation)
    {
        bodyRootPosition = position;
        initialPosition = position;
        initialRotation = Quaternion.Euler(rotation);

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        offsetCalibrated = false;       // this cause also calibrating kinect offset in moveAvatar function 
    }

    protected Vector3 GetTranslatedBonePos(KinectInterop.JointType joint)
    {
        Vector3 value;
        bool success = translatedBoneTransforms.TryGetValue(joint, out value);
        return value;
    }

    protected Dictionary<KinectInterop.JointType, Vector3> translatedBoneTransforms = new Dictionary<KinectInterop.JointType, Vector3>();

    protected void TranslateBone(Int64 userId, KinectInterop.JointType joint, int boneIndex, bool flip)
    {
        Transform boneTransform = bones[boneIndex];
        Vector3 oldPos = boneTransform.position;
        if (boneTransform == null || kinectManager == null)
            return;

        if (boneTransform.name.Contains("Hand"))
            return;

        int iJoint = (int)joint;
        if (iJoint < 0 || !kinectManager.IsJointTracked(userId, iJoint))
            return;



        switch (joint)
        {
            //case KinectInterop.JointType.AnkleLeft:
            //case KinectInterop.JointType.AnkleRight:
            //    //    Transform originalParent = boneTransform.parent;
            //    //    boneTransform.parent = null;
            //    //    boneTransform.position = GetRawJointWorldPos(joint);
            //    //    boneTransform.parent = originalParent;
            //    break;
            //case KinectInterop.JointType.ElbowLeft:
            //case KinectInterop.JointType.ElbowRight:
            //case KinectInterop.JointType.WristLeft:
            //case KinectInterop.JointType.WristRight:
            //case KinectInterop.JointType.HandLeft:
            //case KinectInterop.JointType.HandRight:
            //case KinectInterop.JointType.HandTipLeft:
            //case KinectInterop.JointType.HandTipRight:
            //// Don't override position. Just use what kinect gives us. Edit: this is a bad idea -HH
            //break;
            default:
                boneTransform.position = GetRawJointWorldPos(joint);
                break;
        }

        Vector3 shoulderLeft = GetRawJointWorldPos(KinectInterop.JointType.ShoulderLeft);
        Vector3 shoulderRight = GetRawJointWorldPos(KinectInterop.JointType.ShoulderRight);
        Vector3 shoulderCenter = Vector3.Lerp(shoulderLeft, shoulderRight, 0.5f);
        Vector3 hipLeft = GetRawJointWorldPos(KinectInterop.JointType.HipLeft);
        Vector3 hipRight = GetRawJointWorldPos(KinectInterop.JointType.HipRight);
        Vector3 hipCenter = Vector3.Lerp(hipLeft, hipRight, 0.5f);
        float hipUpwardOffset = (shoulderCenter - hipCenter).magnitude * hipUpwardsFactor;

        // Compensate for joint mapping differences and apply hip/shoulder tuning adjustments
        switch (joint)
        {
            case KinectInterop.JointType.SpineBase:
                boneTransform.localPosition += new Vector3(0, hipUpwardOffset, 0);
                break;
            case KinectInterop.JointType.SpineShoulder:
                //Transform leftClavicle = GetChildBone(boneTransform, 0);
                //Transform rightClavicle = GetChildBone(boneTransform, 2);
                //Vector3 spineToLeftShoulder = leftClavicle.position - boneTransform.position;
                //Vector3 spineToRightShoulder = rightClavicle.position - boneTransform.position;

                boneTransform.localPosition += new Vector3(0, shoulderCenterVerticalOffset, 0);

                //leftClavicle.position = boneTransform.position + spineToLeftShoulder;
                //rightClavicle.position = boneTransform.position + spineToRightShoulder;
                break;
            case KinectInterop.JointType.SpineMid:
                boneTransform.localPosition += new Vector3(0, spineVerticalOffset, 0);
                break;
            case KinectInterop.JointType.Neck:
                boneTransform.localPosition += new Vector3(0, neckVerticalOffset, 0);
                break;
            case KinectInterop.JointType.Head:
                boneTransform.localPosition += new Vector3(0, headVerticalOffset, 0);
                break;
            case KinectInterop.JointType.HipLeft:
            case KinectInterop.JointType.HipRight:
                Vector3 dirHipFromCenter = (joint == KinectInterop.JointType.HipLeft) ? hipLeft - hipCenter : hipRight - hipCenter;
                hipCenter = hipCenter + bones[jointMap2boneIndex[KinectInterop.JointType.SpineBase]].up * hipUpwardOffset;
                boneTransform.position = hipCenter + dirHipFromCenter * hipWidthFactor;
                break;
            case KinectInterop.JointType.KneeLeft:
            case KinectInterop.JointType.KneeRight:
                // Move them down if thighs are straight down because kinect seems to shift knees up when legs are straight down.

                Transform hipJoint = GetParentBone(boneTransform);
                Vector3 thighForward = boneTransform.position - hipJoint.position;  // kneePos - hipPos
                float straightness = Vector3.Dot(thighForward.normalized, -bodyRoot.up);

                //Debug.Log("straightness " + straightness);
                float adjustedStraightness = straightness;
                float straightLegOffset = 0.02f;
                float max = 0.9f;
                float min = 0.6f;
                float range = max - min;
                if (straightness > max)
                    adjustedStraightness = 1.0f;
                else if (straightness < min)
                    adjustedStraightness = 0f;
                else
                    adjustedStraightness = (adjustedStraightness - min) / range;
                //Debug.Log("adjustedStraightness " + adjustedStraightness);

                float thighLength = thighForward.magnitude + adjustedStraightness * straightLegOffset;
                boneTransform.position = hipJoint.position + thighForward.normalized * thighLength;
                break;
            case KinectInterop.JointType.ShoulderLeft:
            case KinectInterop.JointType.ShoulderRight:
                Vector3 dirShoulderFromCenter = (joint == KinectInterop.JointType.ShoulderLeft) ? shoulderLeft - shoulderCenter : shoulderRight - shoulderCenter;
                boneTransform.position = shoulderCenter + dirShoulderFromCenter * shoulderWidthFactor;
                boneTransform.position += GetShoulderVerticalOffset(joint);
                break;

                //case KinectInterop.JointType.AnkleLeft:
                //    // Move the ankle down to the toebase (fix for models wearing heels with lifted ankles, which causes shins to become shortened).
                //    Vector3 kneePos = boneTransform.parent.position;
                //    Transform toeEnd = boneTransform.GetChild(0);
                //    Vector3 toePos = toeEnd.position;
                //    Vector3 projection = Vector3.Project(toePos - kneePos, boneTransform.position - kneePos);
                //    Vector3 adjustedAnklePos = kneePos + projection;
                //    boneTransform.position = adjustedAnklePos;
                //    break;

                //case KinectInterop.JointType.AnkleRight:
                //    break;
        }

        //boneTransform.position = Vector3.Lerp(oldPos, boneTransform.position, 0.65f);

        translatedBoneTransforms.Add(joint, boneTransform.position);
    }

    //public Vector3 elbowOutty;
    //public Vector3 elbowPos;
    //public Vector3 shOutty;
    //public Vector3 shPos;
    //public Vector3 spineOutty;
    //public Vector3 finalOutty;
    //public Vector3 elbowForward;

    // Apply the rotations tracked by kinect to the joints.
    protected void TransformBone(Int64 userId, KinectInterop.JointType joint, int boneIndex, bool flip)
    {
        Transform boneTransform = bones[boneIndex];
        Quaternion oldRot = boneTransform.rotation;
        Vector3 oldScale = boneTransform.localScale;
        if (boneTransform == null || kinectManager == null)
            return;

        int iJoint = (int)joint;
        if (iJoint < 0 || !kinectManager.IsJointTracked(userId, iJoint))
            return;

        // Get Kinect joint orientation
        Quaternion jointRotation = kinectManager.GetJointOrientation(userId, iJoint, flip);
        if (jointRotation == Quaternion.identity)
            return;

        // calculate the new orientation
        Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);

        if (externalRootMotion)
        {
            newRotation = transform.rotation * newRotation;
        }


        // Smoothly transition to the new rotation
        if (smoothFactor != 0f)
            boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
        else
            boneTransform.rotation = newRotation;

        Vector3 spineToHipForward = GetRawJointWorldPos(KinectInterop.JointType.SpineBase) - GetRawJointWorldPos(KinectInterop.JointType.SpineMid);
        // Correct orientation for Shoulders and Thighs
        switch (joint)
        {
            case KinectInterop.JointType.ShoulderLeft:
                Vector3 shoulderLeftForward = GetTranslatedBonePos(KinectInterop.JointType.ElbowLeft) - boneTransform.position;
                Vector3 elbowLeftForward = GetTranslatedBonePos(KinectInterop.JointType.WristLeft) - GetTranslatedBonePos(KinectInterop.JointType.ElbowLeft);
                Vector3 shoulderDown = GetShoulderDown(boneTransform.position, shoulderLeftForward, elbowLeftForward);
                // Recalculate shoulderDown so that it is a perfect 90 degree angle from shoulderLeftFoward.
                Vector3 shoulderOut = Vector3.Cross(shoulderLeftForward, shoulderDown);
                shoulderDown = Vector3.Cross(shoulderOut, shoulderLeftForward);
                boneTransform.rotation = Quaternion.LookRotation(shoulderDown, shoulderLeftForward);
                //shPos = boneTransform.position;
                //shOutty = boneTransform.up;
                break;
            case KinectInterop.JointType.ShoulderRight:
                Vector3 shoulderRightForward = GetTranslatedBonePos(KinectInterop.JointType.ElbowRight) - boneTransform.position;
                Vector3 elbowRightForward = GetTranslatedBonePos(KinectInterop.JointType.WristRight) - GetTranslatedBonePos(KinectInterop.JointType.ElbowRight);
                shoulderDown = GetShoulderDown(boneTransform.position, shoulderRightForward, elbowRightForward);
                // Recalculate shoulderDown so that it is a perfect 90 degree angle from shoulderLeftFoward.
                shoulderOut = Vector3.Cross(shoulderRightForward, shoulderDown);
                shoulderDown = Vector3.Cross(shoulderOut, shoulderRightForward);
                boneTransform.rotation = Quaternion.LookRotation(shoulderDown, shoulderRightForward);
                break;
            case KinectInterop.JointType.ElbowLeft:
            case KinectInterop.JointType.ElbowRight:
                {
                    Transform shoulderJoint = GetParentBone(boneTransform);
                    Transform wristJoint = GetChildBone(boneTransform);
                    Vector3 shoulderForward = (boneTransform.position - shoulderJoint.position).normalized;
                    Vector3 elbowForward = (wristJoint.position - boneTransform.position).normalized;
                    float elbowStraightness = Vector3.Dot(shoulderForward, elbowForward);

                    Vector3 spineDown = (bones[jointMap2boneIndex[KinectInterop.JointType.SpineMid]].up * -1.0f).normalized;
                    float elbowDownness = Vector3.Dot(elbowForward, spineDown);

                    //Debug.Log("s = " + elbowStraightness);
                    //Debug.Log("d = " + elbowDownness);
                    if (elbowStraightness > 0.9f && elbowDownness > 0.95f)
                    {

                        shoulderDown = GetShoulderDown(shoulderJoint.position, shoulderForward, elbowForward);
                        // Recalculate shoulderDown so that it is a perfect 90 degree angle from shoulderLeftFoward.
                        shoulderOut = Vector3.Cross(shoulderForward, shoulderDown);
                        shoulderDown = Vector3.Cross(shoulderOut, shoulderForward);
                        Quaternion origRot = boneTransform.rotation;
                        boneTransform.Rotate(elbowForward, 90f * (joint == KinectInterop.JointType.ElbowRight ? -1f : 1f), Space.Self);
                        boneTransform.rotation = Quaternion.Slerp(origRot, boneTransform.rotation, (1f - elbowDownness) / 0.05f);
                    }
                }
                break;
            case KinectInterop.JointType.HipLeft:
                Vector3 hipLeftForward = GetTranslatedBonePos(KinectInterop.JointType.KneeLeft) - boneTransform.position;
                Vector3 kneeLeftForward = GetTranslatedBonePos(KinectInterop.JointType.AnkleLeft) - GetTranslatedBonePos(KinectInterop.JointType.KneeLeft);

                Vector3 kneeOut = GetKneeOut(hipLeftForward, kneeLeftForward, spineToHipForward);
                Vector3 hipLeftDown = Vector3.Cross(hipLeftForward, kneeOut);

                boneTransform.rotation = Quaternion.LookRotation(hipLeftDown, hipLeftForward);
                break;
            case KinectInterop.JointType.HipRight:
                Vector3 hipRightForward = GetTranslatedBonePos(KinectInterop.JointType.KneeRight) - boneTransform.position;
                Vector3 kneeRightForward = GetTranslatedBonePos(KinectInterop.JointType.AnkleRight) - GetTranslatedBonePos(KinectInterop.JointType.KneeRight);

                kneeOut = GetKneeOut(hipRightForward, kneeRightForward, spineToHipForward);
                Vector3 hipRightDown = Vector3.Cross(hipRightForward, kneeOut);

                boneTransform.rotation = Quaternion.LookRotation(hipRightDown, hipRightForward);
                break;
                //case KinectInterop.JointType.WristLeft:
                //case KinectInterop.JointType.WristRight:
                //case KinectInterop.JointType.HandLeft:
                //case KinectInterop.JointType.HandRight:
                //case KinectInterop.JointType.HandTipLeft:
                //case KinectInterop.JointType.HandTipRight:
                //    // Don't allow bending of wrists. It reveals skinning problems with sleeves.
                //    boneTransform.rotation = initialRotations[boneIndex];
                //    break;
        }

        //boneTransform.rotation = Quaternion.Slerp(oldRot, boneTransform.rotation, 0.65f);
    }

    protected void PostRotateBone(Int64 userId, KinectInterop.JointType joint, int boneIndex, bool flip)
    {
        Transform boneTransform = bones[boneIndex];
        if (boneTransform == null || kinectManager == null)
            return;

        int iJoint = (int)joint;
        if (iJoint < 0 || !kinectManager.IsJointTracked(userId, iJoint))
            return;

        // Get Kinect joint orientation
        Quaternion jointRotation = kinectManager.GetJointOrientation(userId, iJoint, flip);
        if (jointRotation == Quaternion.identity)
            return;
    }

    protected virtual Transform GetChildBone(Transform boneTransform, int idx = 0)
    {
        return boneTransform.GetChild(idx);
    }

    protected virtual Transform GetParentBone(Transform boneTransform)
    {
        return boneTransform.parent;
    }

    private float _origUpperArmLength = 0;
    protected float origUpperArmLength
    {
        get
        {
            if (_origUpperArmLength == 0)
                _origUpperArmLength = (initialPositions[jointMap2boneIndex[KinectInterop.JointType.ShoulderLeft]] -
                                initialPositions[jointMap2boneIndex[KinectInterop.JointType.ElbowLeft]]).magnitude;
            return _origUpperArmLength;
        }
    }

    private float _origLowerArmLength = 0;
    protected float origLowerArmLength
    {
        get
        {
            if (_origLowerArmLength == 0)
                _origLowerArmLength = (initialPositions[jointMap2boneIndex[KinectInterop.JointType.ElbowLeft]] -
                        initialPositions[jointMap2boneIndex[KinectInterop.JointType.WristLeft]]).magnitude;
            return _origLowerArmLength;
        }
    }

    private float _origThighLength = 0;
    protected float origThighLength
    {
        get
        {
            if (_origThighLength == 0)
                _origThighLength = (initialPositions[jointMap2boneIndex[KinectInterop.JointType.HipLeft]] -
                    initialPositions[jointMap2boneIndex[KinectInterop.JointType.KneeLeft]]).magnitude;
            return _origThighLength;
        }
    }

    private float _origShinLength = 0;
    protected float origShinLength
    {
        get
        {
            if (_origShinLength == 0)
                _origShinLength = (initialPositions[jointMap2boneIndex[KinectInterop.JointType.AnkleLeft]] -
                    initialPositions[jointMap2boneIndex[KinectInterop.JointType.KneeLeft]]).magnitude;
            return _origShinLength;
        }
    }

    protected virtual void SetBoneScale(ref Transform boneTransform, Vector3 localScale)
    {
        boneTransform.localScale = localScale;
    }

    // Apply the rotations tracked by kinect to the joints.
    protected void ScaleBone(Int64 userId, KinectInterop.JointType joint, int boneIndex, bool flip)
    {
        Transform boneTransform = bones[boneIndex];
        Vector3 oldScale = boneTransform.localScale;
        if (boneTransform == null || kinectManager == null)
            return;

        int iJoint = (int)joint;
        if (iJoint < 0 || !kinectManager.IsJointTracked(userId, iJoint))
            return;

        // Correct scaling
        switch (joint)
        {
            case KinectInterop.JointType.ShoulderLeft:
            case KinectInterop.JointType.ShoulderRight:
                {
                    Transform elbowJoint = GetChildBone(boneTransform);
                    float upperArmLength = (elbowJoint.position - boneTransform.position).magnitude;

                    resetJointScale(ref boneTransform);
                    SetBoneScale(ref boneTransform, 
                        new Vector3(boneTransform.localScale.x, upperArmLength / origUpperArmLength, boneTransform.localScale.z));
                    // Unscale child bone
                    resetJointScale(ref elbowJoint);

                    Transform wristJoint = GetChildBone(elbowJoint);
                    float lowerArmLength = (wristJoint.position - elbowJoint.position).magnitude;

                    SetBoneScale(ref elbowJoint, 
                        new Vector3(
                            boneTransform.localScale.x,
                            elbowJoint.localScale.y * (lowerArmLength / origLowerArmLength),
                            boneTransform.localScale.z));
                }
                break;
            //case KinectInterop.JointType.ShoulderRight:
            //    {
            //        Transform elbowRight = GetChildBone(boneTransform);
            //        float upperArmLength = (GetTranslatedBonePos(KinectInterop.JointType.ElbowRight) - boneTransform.position).magnitude;
            //        float origUpperArmLength = (initialPositions[jointMap2boneIndex[KinectInterop.JointType.ShoulderRight]] -
            //            initialPositions[jointMap2boneIndex[KinectInterop.JointType.ElbowRight]]).magnitude;
            //        resetJointScale(ref boneTransform);
            //        boneTransform.localScale = new Vector3(boneTransform.localScale.x, upperArmLength / origUpperArmLength, boneTransform.localScale.z);
            //        // Unscale child bone
            //        resetJointScale(ref elbowRight);
                    
            //        float lowerArmLength = (elbowRight.GetChild(0).GetChild(0).position - elbowRight.position).magnitude;
            //        float origLowerArmLength = (initialPositions[jointMap2boneIndex[KinectInterop.JointType.ElbowLeft]] -
            //            initialPositions[jointMap2boneIndex[KinectInterop.JointType.WristLeft]]).magnitude;

            //        elbowRight.localScale = new Vector3(
            //            boneTransform.localScale.x,
            //            elbowRight.localScale.y * (lowerArmLength / origLowerArmLength),
            //            boneTransform.localScale.z);
            //    }
            //    break;
            case KinectInterop.JointType.HipLeft:
            case KinectInterop.JointType.HipRight:
                {
                    Transform kneeBone = GetChildBone(boneTransform);
                    Transform ankleBone = GetChildBone(kneeBone);

                    Vector3 thighForward = kneeBone.position - boneTransform.position;
                    float thighLength = thighForward.magnitude;

                    //Debug.Log("hipLeftY scale = " + (thighLength / origThighLength));
                    float thighScaleFactor = thighLength / origThighLength;
                    //resetJointScale(ref boneTransform);
                    Transform hipBone = GetParentBone(boneTransform);
                    SetBoneScale(ref boneTransform,
                        new Vector3(boneTransform.localScale.x, thighScaleFactor, boneTransform.localScale.z));
                    

                    // Scale shin
                    //float shinLength = (ankleLeft.position - kneeLeft.position).magnitude;
                    // Since measuring shinLength is very unstable due to Kinect reliability when foot is on floor (low depth difference),
                    // we'll just use an average human shin-to-knee ratio of 1.03:1.00
                    float shinLength = thighLength * 1.03f;
                    float shinScaleFactor = shinLength / origShinLength;
                    //shinScaleFactor = 1;

                    //Debug.Log("thighScaleFactor = " + thighScaleFactor);
                    //Debug.Log("shinScaleFactor = " + shinScaleFactor);
                    //Debug.Log("origShinLength = " + origShinLength);

                    //resetJointScale(ref kneeLeft);
                    SetBoneScale(ref kneeBone,
                        new Vector3(kneeBone.localScale.x, shinScaleFactor, kneeBone.localScale.z));
                    //resetJointScale(ref ankleLeft);
                }
                break;
        }

        //boneTransform.localScale = Vector3.Lerp(oldScale, boneTransform.localScale, 0.65f);

    }

    protected virtual void resetJointScale(ref Transform joint)
    {
        Vector3 parentScale = joint.parent.lossyScale;
        joint.localScale = new Vector3(1f / parentScale.x, 1f / parentScale.y, 1f / parentScale.z);

        int idx = joint.GetSiblingIndex();
        Transform oldParent = joint.parent;
        joint.parent = null;
        joint.localScale = Vector3.one;
        joint.parent = oldParent;
        joint.SetSiblingIndex(idx);
    }


    //public Vector3 shPos;
    //public Vector3 shFinalDown;
    //public Vector3 shLeftForward;
    //public Vector3 shElbowForward;
    //public Vector3 shElbowDown;
    //public Vector3 shShoulderDown;
    //public Vector3 shSpineIn;

    protected Vector3 GetKneeOut(Vector3 hipForward, Vector3 kneeForward, Vector3 spineToHipForward)
    {
        // Use the most reliable kneeOut (if knee is bent, use that, if hips are bent, use that, otherwise, just use spine out)
        Vector3 spineOut = bones[jointMap2boneIndex[KinectInterop.JointType.SpineBase]].right;

        float kneeInterp = 0;
        float hipInterp = 0;
        Vector3 kneeOut = spineOut;
        Vector3 hipOut = spineOut;

        float legStraightness = Vector3.Dot(hipForward.normalized, kneeForward.normalized);
        if (legStraightness < 0.95f)
        {
            kneeOut = Vector3.Cross(hipForward, kneeForward);
            kneeInterp = Mathf.Clamp((0.95f - legStraightness) / .1f, 0, 1);
        }

        float hipStraightness = Vector3.Dot(hipForward.normalized, spineToHipForward.normalized);
        if (hipStraightness < 0.95f)
        {
            hipOut = Vector3.Cross(spineToHipForward, hipForward);
            if (Vector3.Dot(hipOut, spineOut) < 0) // if kneeout is facing inward, invert it.
                hipOut *= -1.0f;
            hipInterp = Mathf.Clamp((0.95f - hipStraightness) / .1f, 0, 1);
        }

        // interpolate between spineRight
        Vector3 finalKneeOut = spineOut;
        if (kneeInterp > 0 || hipInterp > 0)
            finalKneeOut = Vector3.Lerp(
                Vector3.Lerp(kneeOut, spineOut, kneeInterp), 
                Vector3.Lerp(hipOut, spineOut, hipInterp), kneeInterp / (kneeInterp + hipInterp));

        return finalKneeOut;
    }

    protected Vector3 GetElbowOut(Vector3 shoulderForward, Vector3 elbowForward, Vector3 spineToHipForward)
    {
        // Use the most reliable kneeOut (if knee is bent, use that, if hips are bent, use that, otherwise, just use spine out)
        Vector3 spineOut = bones[jointMap2boneIndex[KinectInterop.JointType.SpineBase]].right;

        float elbowInterp = 0;
        float shoulderInterp = 0;
        Vector3 elbowOut = spineOut;
        Vector3 shoulderOut = spineOut;
        //spineOutty = spineOut;

        float elbowStraightness = Vector3.Dot(shoulderForward.normalized, elbowForward.normalized);
        if (elbowStraightness < 0.95f)
        {
            elbowOut = Vector3.Cross(elbowForward, shoulderForward).normalized;
            //elbowOutty = elbowOut;
            elbowInterp = Mathf.Clamp((0.95f - elbowStraightness) / .1f, 0, 1);
        }

        float shoulderStraightness = Vector3.Dot(shoulderForward.normalized, spineToHipForward.normalized);
        if (shoulderStraightness < 0.95f)
        {
            shoulderOut = Vector3.Cross(shoulderForward, spineToHipForward).normalized;
            //shOutty = shoulderOut;
            if (Vector3.Dot(shoulderOut, spineOut) < 0) // if shoulderout is facing inward, invert it.
                shoulderOut *= -1.0f;
            shoulderInterp = Mathf.Clamp((0.95f - shoulderStraightness) / .1f, 0, 1);
        }

        // favor elbowInterp over shoulderInterp heavily if it is a high value.
        float shoulderInterpWeight = Mathf.Lerp(1, 0, elbowInterp);

        //Debug.Log("elbowStraightness=" + elbowStraightness);
        //Debug.Log("elbowInterp=" + elbowInterp);
        //Debug.Log("shoulderStraightness=" + shoulderStraightness);
        //Debug.Log("shoulderInterp=" + shoulderInterp);

        // interpolate between spineRight
        Vector3 finalElbowOut = spineOut;
        if (elbowInterp > 0 || shoulderInterp > 0)
            finalElbowOut = Vector3.Lerp(
                Vector3.Lerp(spineOut, shoulderOut, shoulderInterp),
                Vector3.Lerp(spineOut, elbowOut, elbowInterp), elbowInterp / (elbowInterp + shoulderInterp*shoulderInterpWeight));

        //finalOutty = finalElbowOut;
        return finalElbowOut;
    }

    protected Vector3 GetElbowStraightDown(Vector3 shoulderPos, Vector3 shoulderForward, Vector3 elbowForward, ref float interp)
    {
        Vector3 spineDown = bones[jointMap2boneIndex[KinectInterop.JointType.SpineMid]].up * -1.0f;
        bool isFlipped = false;
        Vector3 spineIn = GetSpineIn(shoulderPos, ref isFlipped);
        float shoulderStraightness = Vector3.Dot(spineDown.normalized, shoulderForward.normalized);
        float elbowStraightness = Vector3.Dot(shoulderForward.normalized, elbowForward.normalized);

        Debug.Log("shoulderStraightness = " + shoulderStraightness);
        Debug.Log("elbowStraightness = " + elbowStraightness);
        float avgStraightness = (shoulderStraightness + elbowStraightness) / 2.0f;
        interp = 1f - Mathf.Clamp((1f - avgStraightness) / 0.1f, 0, 1);
        Debug.Log("interp = " + interp);

        Vector3 shoulderOut = Vector3.Cross(spineDown, shoulderForward);
        Vector3 shoulderDown = Vector3.Cross(shoulderForward, shoulderOut);
        return shoulderDown;
    }

    protected Vector3 GetSpineIn(Vector3 shoulderPos, ref bool isFlipped)
    {
        Vector3 spineIn = bones[jointMap2boneIndex[KinectInterop.JointType.SpineMid]].right * -1.0f;
        Vector3 shoulderToSpine = GetRawJointWorldPos(KinectInterop.JointType.SpineMid) - shoulderPos;
        isFlipped = false;
        if (Vector3.Dot(spineIn, shoulderToSpine) < 0)
        {
            spineIn *= -1.0f; // flip it
            isFlipped = true;
        }
        return spineIn;
    }

    protected Vector3 GetShoulderDown(Vector3 shoulderPos, Vector3 shoulderForward, Vector3 elbowForward)
    {
        // First determine default spineOut position and set as default sho
        Vector3 spineDown = bones[jointMap2boneIndex[KinectInterop.JointType.SpineMid]].up * -1.0f;
        bool isFlipped = false;
        Vector3 spineIn = GetSpineIn(shoulderPos, ref isFlipped);

        float shoulderInterp = 0;
        float elbowInterp = 0;
        Vector3 shoulderDown = spineIn;
        Vector3 elbowDown = spineIn;

        // Is the shoulder not too straight with spine down? If so, we can calculate a better shoulderDown
        float shoulderStraightness = Vector3.Dot(spineDown.normalized, shoulderForward.normalized);
        //Debug.Log("shoulderStraightness = " + shoulderStraightness);
        if (shoulderStraightness < 0.99f)
        {
            Vector3 shoulderOut = Vector3.Cross(spineDown, shoulderForward);
            shoulderDown = Vector3.Cross(shoulderForward, shoulderOut);
            shoulderInterp = Mathf.Clamp((0.99f - shoulderStraightness) / .05f, 0, 1);
        }

        float elbowStraightness = Vector3.Dot(shoulderForward.normalized, elbowForward.normalized);
        if (elbowStraightness < 0.95f)
        {
            elbowDown = Vector3.Cross(shoulderForward, elbowForward);
            if (isFlipped)
                elbowDown *= -1.0f;
            elbowInterp = Mathf.Clamp((0.95f - elbowStraightness) / .1f, 0, 1);
        }

        //Debug.Log("elbowInterp = " + elbowInterp);
        //Debug.Log("shoulderInterp = " + shoulderInterp);
        // interpolate between spineRight
        Vector3 finalShoulderDown = spineIn;
        if (shoulderInterp > 0 || elbowInterp > 0)
        {
            if (shoulderInterp > 0)
                finalShoulderDown = Vector3.Slerp(finalShoulderDown, shoulderDown, shoulderInterp);
            if (elbowInterp > 0)
                finalShoulderDown = Vector3.Slerp(finalShoulderDown, elbowDown, elbowInterp / 2.0f); // elbowDown weight is halved.
        }

        //shLeftForward = shoulderForward;
        //shElbowForward = elbowForward;
        //shElbowDown = elbowDown;
        //shPos = shoulderPos;
        //shShoulderDown = shoulderDown;
        //shSpineIn = spineIn;
        //shFinalDown = finalShoulderDown;

        return finalShoulderDown;
    }

    //protected void CheckLimitShoulderAngle(Transform bone)
    //{
    //    float angle = bone.localEulerAngles.y % 360f;
    //    if (angle > 180f) {
    //        if (angle < (360f - shoulderAngleRange))
    //            angle = 360f - shoulderAngleRange;
    //    }
    //    else if (angle < 180f)
    //    {
    //        if (angle > shoulderAngleRange)
    //            angle = shoulderAngleRange;
    //    }

    //    if (angle != bone.localEulerAngles.y)
    //        bone.localEulerAngles = new Vector3(bone.localEulerAngles.x, angle, bone.localEulerAngles.z);

    //}

    // Apply the rotations tracked by kinect to a special joint
    // Clavicle left and right are boneIndex 25 and 26 respectively
    protected void TransformSpecialBone(Int64 userId, KinectInterop.JointType joint, KinectInterop.JointType jointParent, int boneIndex, Vector3 baseDir, bool flip)
	{
        
		Transform boneTransform = bones[boneIndex];
		if(boneTransform == null || kinectManager == null)
			return;
		
		if(!kinectManager.IsJointTracked(userId, (int)joint) || 
		   !kinectManager.IsJointTracked(userId, (int)jointParent))
		{
			return;
		}

		if(boneIndex >= 27 && boneIndex <= 30)
		{
			// fingers or thumbs
			if(fingerOrientations && !externalHandRotations)
			{
				TransformSpecialBoneFingers(userId, (int)joint, boneIndex, flip);
			}

			return;
		}
		
		// if the user is turned, tracking of special bones may be incorrect 
		bool userTurned = kinectManager.IsUserTurnedAround(userId);
		if (userTurned) 
		{
			return;
		}
		
		Vector3 jointDir = kinectManager.GetJointDirection(userId, (int)joint, userTurned, true);
		Quaternion jointRotation = jointDir != Vector3.zero ? Quaternion.FromToRotation(baseDir, jointDir) : Quaternion.identity;
		
		if(!flip)
		{
			Vector3 mirroredAngles = jointRotation.eulerAngles;
			mirroredAngles.y = -mirroredAngles.y;
			mirroredAngles.z = -mirroredAngles.z;

			jointRotation = Quaternion.Euler(mirroredAngles);
		}
		
		if(jointRotation != Quaternion.identity)
		{
			// Smoothly transition to the new rotation
			Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);
			
			if(externalRootMotion)
			{
				newRotation = transform.rotation * newRotation;
			}
			
			if(smoothFactor != 0f)
				boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
			else
				boneTransform.rotation = newRotation;
		}

        // Adjust clavicle based on how raised the arms are relative to spine.
        if(boneIndex == 26 || boneIndex == 25)
        {
            boneTransform.Rotate(boneTransform.right, GetShoulderVerticalOffsetAngle(joint));
        }

    }

    Vector3 GetShoulderVerticalOffset(KinectInterop.JointType joint)
    {
        if (joint != KinectInterop.JointType.ShoulderLeft &&
            joint != KinectInterop.JointType.ShoulderRight)
        {
            Debug.LogError("Invalid joint type for method GetShoulderVerticalOffset!");
            return Vector3.zero;
        }

        float angle = GetShoulderVerticalOffsetAngle(joint);

        int spineBoneIdx = jointMap2boneIndex[KinectInterop.JointType.SpineShoulder];
        Transform spineTransform = bones[spineBoneIdx];
        return spineTransform.up * 0.1f * (angle / 30.0f);
    }

    float GetShoulderVerticalOffsetAngle(KinectInterop.JointType joint)
    {
        if (joint != KinectInterop.JointType.ShoulderLeft &&
            joint != KinectInterop.JointType.ShoulderRight)
        {
            Debug.LogError("Invalid joint type for method GetShoulderVerticalOffsetAngle!");
            return 0;
        }

        int spineBoneIdx = jointMap2boneIndex[KinectInterop.JointType.SpineShoulder];
        Transform spineTransform = bones[spineBoneIdx];

        // Get left shoulder and elbow positions relative to spine, so it's easy to check height value (Y)
        KinectInterop.JointType elbowJoint = joint == KinectInterop.JointType.ShoulderLeft ? KinectInterop.JointType.ElbowLeft : KinectInterop.JointType.ElbowRight;
        KinectInterop.JointType shoulderJoint = joint == KinectInterop.JointType.ShoulderLeft ? KinectInterop.JointType.ShoulderLeft : KinectInterop.JointType.ShoulderRight;
        Vector3 shoulderLocalPos = spineTransform.InverseTransformPoint(GetRawJointWorldPos(shoulderJoint));
        Vector3 elbowLocalPos = spineTransform.InverseTransformPoint(GetRawJointWorldPos(elbowJoint));

        //Debug.Log("shoulder.y" + shoulderLeftLocalPos.y);
        //Debug.Log("elbow.y" + elbowLeftLocalPos.y);
        //boneTransform.rotation = initialRotations[boneIndex];
        //boneTransform.localRotation = localRotations[boneIndex];
        if (elbowLocalPos.y > shoulderLocalPos.y)
        {
            //boneTransform.localEulerAngles += new Vector3(-30, 0, 0);
            // 
            //Vector3 spineUp = spineTransform.up;
            Vector3 elbowFlatPos = elbowLocalPos;
            elbowFlatPos.y = shoulderLocalPos.y;
            float angle = Vector3.Angle(elbowFlatPos, elbowLocalPos);
         //   Debug.Log(joint + " Angle = " + angle);
            //boneTransform.Rotate(boneTransform.right, angle);
            return angle;
        }
        return 0;
    }

	// Apply the rotations tracked by kinect to fingers (one joint = multiple bones)
	protected void TransformSpecialBoneFingers(Int64 userId, int joint, int boneIndex, bool flip)
	{
		// check for hand grips
		if(joint == (int)KinectInterop.JointType.HandTipLeft || joint == (int)KinectInterop.JointType.ThumbLeft)
		{
			if(lastLeftHandEvent == InteractionManager.HandEventType.Grip)
			{
				if(!bLeftFistDone && !kinectManager.IsUserTurnedAround(userId))
				{
					float angleSign = !mirroredMovement /**(boneIndex == 27 || boneIndex == 29)*/ ? -1f : -1f;
					float angleRot = angleSign * 60f;
					
					TransformSpecialBoneFist(boneIndex, angleRot);
					bLeftFistDone = (boneIndex >= 29);
				}
				
				return;
			}
			else if(bLeftFistDone && lastLeftHandEvent == InteractionManager.HandEventType.Release)
			{
				TransformSpecialBoneUnfist(boneIndex);
				bLeftFistDone = !(boneIndex >= 29);
			}
		}
		else if(joint == (int)KinectInterop.JointType.HandTipRight || joint == (int)KinectInterop.JointType.ThumbRight)
		{
			if(lastRightHandEvent == InteractionManager.HandEventType.Grip)
			{
				if(!bRightFistDone && !kinectManager.IsUserTurnedAround(userId))
				{
					float angleSign = !mirroredMovement /**(boneIndex == 27 || boneIndex == 29)*/ ? -1f : -1f;
					float angleRot = angleSign * 60f;
					
					TransformSpecialBoneFist(boneIndex, angleRot);
					bRightFistDone = (boneIndex >= 29);
				}

				return;
			}
			else if(bRightFistDone && lastRightHandEvent == InteractionManager.HandEventType.Release)
			{
				TransformSpecialBoneUnfist(boneIndex);
				bRightFistDone = !(boneIndex >= 29);
			}
		}

		// get the animator component
		//Animator animatorComponent = GetComponent<Animator>();
		if(!animatorComponent)
			return;
		
		// Get Kinect joint orientation
		Quaternion jointRotation = kinectManager.GetJointOrientation(userId, joint, flip);
		if(jointRotation == Quaternion.identity)
			return;

		// calculate the new orientation
		Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);

		if(externalRootMotion)
		{
			newRotation = transform.rotation * newRotation;
		}

		// get the list of bones
		//List<HumanBodyBones> alBones = flip ? specialIndex2MultiBoneMap[boneIndex] : specialIndex2MirrorBoneMap[boneIndex];
		List<HumanBodyBones> alBones = specialIndex2MultiBoneMap[boneIndex];
		
		// Smoothly transition to the new rotation
		for(int i = 0; i < alBones.Count; i++)
		{
			Transform boneTransform = animatorComponent.GetBoneTransform(alBones[i]);
			if(!boneTransform)
				continue;

			if(smoothFactor != 0f)
				boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
			else
				boneTransform.rotation = newRotation;
		}
	}

	// Apply the rotations needed to transform fingers to fist
	protected void TransformSpecialBoneFist(int boneIndex, float angle)
	{
//		// do fist only for fingers
//		if(boneIndex != 27 && boneIndex != 28)
//			return;

		// get the animator component
		//Animator animatorComponent = GetComponent<Animator>();
		if(!animatorComponent)
			return;
		
		// get the list of bones
		List<HumanBodyBones> alBones = specialIndex2MultiBoneMap[boneIndex];

		for(int i = 0; i < alBones.Count; i++)
		{
			if(i < 1 && (boneIndex == 29 || boneIndex == 30))  // skip the first two thumb bones
				continue;
			
			HumanBodyBones bone = alBones[i];
			Transform boneTransform = animatorComponent.GetBoneTransform(bone);

			// set the fist rotation
			if(boneTransform && fingerBoneLocalAxes[bone] != Vector3.zero)
			{
				Quaternion qRotFinger = Quaternion.AngleAxis(angle, fingerBoneLocalAxes[bone]);
				boneTransform.localRotation = fingerBoneLocalRotations[bone] * qRotFinger;
			}
		}

	}
	
	// Apply the initial rotations fingers
	protected void TransformSpecialBoneUnfist(int boneIndex)
	{
//		// do fist only for fingers
//		if(boneIndex != 27 && boneIndex != 28)
//			return;
		
		// get the animator component
		//Animator animatorComponent = GetComponent<Animator>();
		if(!animatorComponent)
			return;
		
		// get the list of bones
		List<HumanBodyBones> alBones = specialIndex2MultiBoneMap[boneIndex];
		
		for(int i = 0; i < alBones.Count; i++)
		{
			HumanBodyBones bone = alBones[i];
			Transform boneTransform = animatorComponent.GetBoneTransform(bone);

			// set the initial rotation
			if(boneTransform)
			{
				boneTransform.localRotation = fingerBoneLocalRotations[bone];
			}
		}
	}
	
	// Moves the avatar - gets the tracked position of the user and applies it to avatar.
	protected bool MoveAvatar(Int64 UserID)
	{
		if((moveRate == 0f) || !kinectManager ||
		   !kinectManager.IsJointTracked(UserID, (int)KinectInterop.JointType.SpineBase))
		{
            return false;
		}

        if(!kinectManager.IsUserPositionValid(UserID))
        {
            gameObject.SetActive(false);
            return false;
        }
		
		// get the position of user's spine base
		Vector3 trans = kinectManager.GetUserPosition(UserID);
		if(flipLeftRight)
			trans.x = -trans.x;

		// use the color overlay position if needed
		if(posRelativeToCamera && posRelOverlayColor)
		{
			if(backgroundPlane && planeRectSet)
			{
				// get the plane overlay position
				trans = kinectManager.GetJointPosColorOverlay(UserID, (int)KinectInterop.JointType.SpineBase, planeRect);
				trans.z = backgroundPlane.position.z - posRelativeToCamera.transform.position.z - 0.1f;  // 10cm offset
			}
			else 
			{
				Rect backgroundRect = posRelativeToCamera.pixelRect;
				PortraitBackground portraitBack = PortraitBackground.Instance;

				if(portraitBack && portraitBack.enabled)
				{
					backgroundRect = portraitBack.GetBackgroundRect();
				}

				trans = kinectManager.GetJointPosColorOverlay(UserID, (int)KinectInterop.JointType.SpineBase, posRelativeToCamera, backgroundRect);
                if (trans.Equals(Vector3.zero))
                    return false;
            }

			if(flipLeftRight)
				trans.x = -trans.x;
		}

		// invert the z-coordinate, if needed
		if(posRelativeToCamera && posRelInvertedZ)
		{
			trans.z = -trans.z;
		}
		
		if(!offsetCalibrated)
		{
			offsetCalibrated = true;
			
			offsetPos.x = trans.x;  // !mirroredMovement ? trans.x * moveRate : -trans.x * moveRate;
			offsetPos.y = trans.y;  // trans.y * moveRate;
			offsetPos.z = !mirroredMovement && !posRelativeToCamera ? -trans.z : trans.z;  // -trans.z * moveRate;

			if(posRelativeToCamera)
			{
				Vector3 cameraPos = posRelativeToCamera.transform.position;
				Vector3 bodyRootPos = bodyRoot != null ? bodyRoot.position : transform.position;
				Vector3 hipCenterPos = bodyRoot != null ? bodyRoot.position : (bones != null && bones.Length > 0 && bones[0] != null ? bones[0].position : Vector3.zero);

				float yRelToAvatar = 0f;
				if(verticalMovement)
				{
					yRelToAvatar = (trans.y - cameraPos.y) - (hipCenterPos - bodyRootPos).magnitude;
				}
				else
				{
					yRelToAvatar = bodyRootPos.y - cameraPos.y;
				}

				Vector3 relativePos = new Vector3(trans.x, yRelToAvatar, trans.z);
				Vector3 newBodyRootPos = cameraPos + relativePos;

//				if(offsetNode != null)
//				{
//					newBodyRootPos += offsetNode.transform.position;
//				}

				if(bodyRoot != null)
				{
					bodyRoot.position = newBodyRootPos;
				}
				else
				{
					transform.position = newBodyRootPos;
				}

				bodyRootPosition = newBodyRootPos;
			}
		}
	
		// transition to the new position
		Vector3 targetPos = bodyRootPosition + Kinect2AvatarPos(trans, verticalMovement);

		if(isRigidBody && !verticalMovement)
		{
			// workaround for obeying the physics (e.g. gravity falling)
			targetPos.y = bodyRoot != null ? bodyRoot.position.y : transform.position.y;
		}

		if (verticalMovement && verticalOffset != 0f && 
			bones[0] != null && bones[3] != null) 
		{
			Vector3 dirSpine = bones[3].position - bones[0].position;
			targetPos += dirSpine.normalized * verticalOffset;
		}

		if (forwardOffset != 0f && 
			bones[0] != null && bones[3] != null && bones[5] != null && bones[11] != null) 
		{
			Vector3 dirSpine = (bones[3].position - bones[0].position).normalized;
			Vector3 dirShoulders = (bones[11].position - bones[5].position).normalized;
			Vector3 dirForward = Vector3.Cross(dirShoulders, dirSpine).normalized;

			targetPos += dirForward * forwardOffset;
		}

		if(groundedFeet)
		{
			// keep the current correction
			float fLastTgtY = targetPos.y;
			targetPos.y += fFootDistance;

			float fNewDistance = GetDistanceToGround();
			float fNewDistanceTime = Time.time;

//			Debug.Log(string.Format("PosY: {0:F2}, LastY: {1:F2},  TgrY: {2:F2}, NewDist: {3:F2}, Corr: {4:F2}, Time: {5:F2}", bodyRoot != null ? bodyRoot.position.y : transform.position.y,
//				fLastTgtY, targetPos.y, fNewDistance, fFootDistance, fNewDistanceTime));
			
			if(Mathf.Abs(fNewDistance) >= 0.01f && Mathf.Abs(fNewDistance - fFootDistanceInitial) >= maxFootDistanceGround)
			{
				if((fNewDistanceTime - fFootDistanceTime) >= maxFootDistanceTime)
				{
					fFootDistance += (fNewDistance - fFootDistanceInitial);
					fFootDistanceTime = fNewDistanceTime;

					targetPos.y = fLastTgtY + fFootDistance;

//					Debug.Log(string.Format("   >> change({0:F2})! - Corr: {1:F2}, LastY: {2:F2},  TgrY: {3:F2} at time {4:F2}", 
//								(fNewDistance - fFootDistanceInitial), fFootDistance, fLastTgtY, targetPos.y, fFootDistanceTime));
				}
			}
			else
			{
				fFootDistanceTime = fNewDistanceTime;
			}
		}
		
		if(bodyRoot != null)
		{
			bodyRoot.position = smoothFactor != 0f ? 
				Vector3.Lerp(bodyRoot.position, targetPos, smoothFactor * Time.deltaTime) : targetPos;
		}
		else
		{
			transform.position = smoothFactor != 0f ? 
				Vector3.Lerp(transform.position, targetPos, smoothFactor * Time.deltaTime) : targetPos;
		}

        return true;
	}
	
	// Set model's arms to be in T-pose
	protected void SetModelArmsInTpose()
	{
		Vector3 vTposeLeftDir = transform.TransformDirection(Vector3.left);
		Vector3 vTposeRightDir = transform.TransformDirection(Vector3.right);

		Transform transLeftUarm = GetBoneTransform(GetBoneIndexByJoint(KinectInterop.JointType.ShoulderLeft, false)); // animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
		Transform transLeftLarm = GetBoneTransform(GetBoneIndexByJoint(KinectInterop.JointType.ElbowLeft, false)); // animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
		Transform transLeftHand = GetBoneTransform(GetBoneIndexByJoint(KinectInterop.JointType.WristLeft, false)); // animator.GetBoneTransform(HumanBodyBones.LeftHand);
		
		if(transLeftUarm != null && transLeftLarm != null)
		{
			Vector3 vUarmLeftDir = transLeftLarm.position - transLeftUarm.position;
			float fUarmLeftAngle = Vector3.Angle(vUarmLeftDir, vTposeLeftDir);
			
			if(Mathf.Abs(fUarmLeftAngle) >= 5f)
			{
				Quaternion vFixRotation = Quaternion.FromToRotation(vUarmLeftDir, vTposeLeftDir);
				transLeftUarm.rotation = vFixRotation * transLeftUarm.rotation;
			}
			
			if(transLeftHand != null)
			{
				Vector3 vLarmLeftDir = transLeftHand.position - transLeftLarm.position;
				float fLarmLeftAngle = Vector3.Angle(vLarmLeftDir, vTposeLeftDir);
				
				if(Mathf.Abs(fLarmLeftAngle) >= 5f)
				{
					Quaternion vFixRotation = Quaternion.FromToRotation(vLarmLeftDir, vTposeLeftDir);
					transLeftLarm.rotation = vFixRotation * transLeftLarm.rotation;
				}
			}
		}
		
		Transform transRightUarm = GetBoneTransform(GetBoneIndexByJoint(KinectInterop.JointType.ShoulderRight, false)); // animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
		Transform transRightLarm = GetBoneTransform(GetBoneIndexByJoint(KinectInterop.JointType.ElbowRight, false)); // animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
		Transform transRightHand = GetBoneTransform(GetBoneIndexByJoint(KinectInterop.JointType.WristRight, false)); // animator.GetBoneTransform(HumanBodyBones.RightHand);
		
		if(transRightUarm != null && transRightLarm != null)
		{
			Vector3 vUarmRightDir = transRightLarm.position - transRightUarm.position;
			float fUarmRightAngle = Vector3.Angle(vUarmRightDir, vTposeRightDir);
			
			if(Mathf.Abs(fUarmRightAngle) >= 5f)
			{
				Quaternion vFixRotation = Quaternion.FromToRotation(vUarmRightDir, vTposeRightDir);
				transRightUarm.rotation = vFixRotation * transRightUarm.rotation;
			}
			
			if(transRightHand != null)
			{
				Vector3 vLarmRightDir = transRightHand.position - transRightLarm.position;
				float fLarmRightAngle = Vector3.Angle(vLarmRightDir, vTposeRightDir);
				
				if(Mathf.Abs(fLarmRightAngle) >= 5f)
				{
					Quaternion vFixRotation = Quaternion.FromToRotation(vLarmRightDir, vTposeRightDir);
					transRightLarm.rotation = vFixRotation * transRightLarm.rotation;
				}
			}
		}
		
	}
	
    protected void SetModelLegsVertical()
    {
        Transform hipLeftBone = bones[jointMap2boneIndex[KinectInterop.JointType.HipLeft]];
        Transform hipRightBone = bones[jointMap2boneIndex[KinectInterop.JointType.HipRight]];
        Transform kneeLeftBone = bones[jointMap2boneIndex[KinectInterop.JointType.KneeLeft]];
        Transform kneeRightBone = bones[jointMap2boneIndex[KinectInterop.JointType.KneeRight]];
        Transform ankleLeftBone = bones[jointMap2boneIndex[KinectInterop.JointType.AnkleLeft]];
        Transform ankleRightBone = bones[jointMap2boneIndex[KinectInterop.JointType.AnkleRight]];

        // Straighten legs on XY plane
        {
            // Isolate angle difference in XY plane by setting same Z value
            Vector3 kneeLeftBonePos = kneeLeftBone.position;
            kneeLeftBonePos.z = hipLeftBone.position.z;

            float angleBetween = Vector3.Angle(kneeLeftBonePos - hipLeftBone.position, Vector3.down);
            //Debug.Log("angleBetweenLHip = " + angleBetween);
            hipLeftBone.localEulerAngles += new Vector3(0, 0, angleBetween);
        }

        {
            // Isolate angle difference in XY plane by setting same Z value
            Vector3 kneeRightBonePos = kneeRightBone.position;
            kneeRightBonePos.z = hipRightBone.position.z;

            float angleBetween = Vector3.Angle(kneeRightBonePos - hipRightBone.position, Vector3.down);
            //Debug.Log("angleBetweenRHip = " + angleBetween);
            hipRightBone.localEulerAngles += new Vector3(0, 0, -angleBetween);
        }

        // Straighten legs on YZ plane
        {
            // Isolate angle difference in YZ plane by setting same X value
            Vector3 kneeLeftBonePos = kneeLeftBone.position;
            kneeLeftBonePos.x = hipLeftBone.position.x;

            float angleBetween = Vector3.Angle(kneeLeftBonePos - hipLeftBone.position, Vector3.down);
            //Debug.Log("angleBetweenLHip = " + angleBetween);
            hipLeftBone.localEulerAngles += new Vector3(angleBetween, 0, 0);
        }

        {
            // Isolate angle difference in YZ plane by setting same X value
            Vector3 kneeRightBonePos = kneeRightBone.position;
            kneeRightBonePos.x = hipRightBone.position.x;

            float angleBetween = Vector3.Angle(kneeRightBonePos - hipRightBone.position, Vector3.down);
            //Debug.Log("angleBetweenRHip = " + angleBetween);
            hipRightBone.localEulerAngles += new Vector3(angleBetween, 0, 0);
        }

        // Straighten knees on YZ plane
        {
            // Isolate angle difference in YZ plane by setting same X value
            Vector3 ankleLeftBonePos = ankleLeftBone.position;
            ankleLeftBonePos.x = kneeLeftBone.position.x;

            float angleBetween = Vector3.Angle(ankleLeftBonePos - kneeLeftBone.position, Vector3.down);
            //Debug.Log("angleBetweeLRKnee = " + angleBetween);
            kneeLeftBone.localEulerAngles += new Vector3(angleBetween, 0, 0);
        }

        {
            // Isolate angle difference in YZ plane by setting same X value
            Vector3 ankleRightBonePos = ankleRightBone.position;
            ankleRightBonePos.x = kneeRightBone.position.x;

            float angleBetween = Vector3.Angle(ankleRightBonePos - kneeRightBone.position, Vector3.down);
            //Debug.Log("angleBetweenRKnee = " + angleBetween);
            kneeRightBone.localEulerAngles += new Vector3(angleBetween, 0, 0);
        }

    }

	// If the bones to be mapped have been declared, map that bone to the model.
	protected virtual void MapBones()
	{
//		// make OffsetNode as a parent of model transform.
//		offsetNode = new GameObject(name + "Ctrl") { layer = transform.gameObject.layer, tag = transform.gameObject.tag };
//		offsetNode.transform.position = transform.position;
//		offsetNode.transform.rotation = transform.rotation;
//		offsetNode.transform.parent = transform.parent;
		
//		// take model transform as body root
//		transform.parent = offsetNode.transform;
//		transform.localPosition = Vector3.zero;
//		transform.localRotation = Quaternion.identity;
		
		//bodyRoot = transform;

		// get bone transforms from the animator component
		//Animator animatorComponent = GetComponent<Animator>();
				
		for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
		{
			if (!boneIndex2MecanimMap.ContainsKey(boneIndex)) 
				continue;
			
			bones[boneIndex] = animatorComponent ? animatorComponent.GetBoneTransform(boneIndex2MecanimMap[boneIndex]) : null;
		}

		// map finger bones, too
		fingerBones = new Transform[fingerIndex2MecanimMap.Count];

		for (int boneIndex = 0; boneIndex < fingerBones.Length; boneIndex++)
		{
			if (!fingerIndex2MecanimMap.ContainsKey(boneIndex)) 
				continue;

			fingerBones[boneIndex] = animatorComponent ? animatorComponent.GetBoneTransform(fingerIndex2MecanimMap[boneIndex]) : null;
		}
	}
	
	// Capture the initial rotations of the bones
	protected void GetInitialRotations()
	{
		// save the initial rotation
		if(offsetNode != null)
		{
			offsetNodePos = offsetNode.transform.position;
			offsetNodeRot = offsetNode.transform.rotation;
		}

		initialPosition = transform.position;
		initialRotation = transform.rotation;

//		initialHipsPosition = bones[0] ? bones[0].localPosition : Vector3.zero;
//		initialHipsRotation = bones[0] ? bones[0].localRotation : Quaternion.identity;

//		if(offsetNode != null)
//		{
//			initialRotation = Quaternion.Inverse(offsetNodeRot) * initialRotation;
//		}

		transform.rotation = Quaternion.identity;

		// save the body root initial position
		if(bodyRoot != null)
		{
			bodyRootPosition = bodyRoot.position;
		}
		else
		{
			bodyRootPosition = transform.position;
		}

		if(offsetNode != null)
		{
			bodyRootPosition = bodyRootPosition - offsetNodePos;
		}
		
		// save the initial bone rotations
		for (int i = 0; i < bones.Length; i++)
		{
			if (bones[i] != null)
			{
				initialRotations[i] = bones[i].rotation;
				localRotations[i] = bones[i].localRotation;
                initialPositions[i] = bones[i].position;
                initialLocalPositions[i] = bones[i].localPosition;
			}
		}

		// get finger bones' local rotations
		//Animator animatorComponent = GetComponent<Animator>();
		foreach(int boneIndex in specialIndex2MultiBoneMap.Keys)
		{
			List<HumanBodyBones> alBones = specialIndex2MultiBoneMap[boneIndex];
			//Transform handTransform = animatorComponent.GetBoneTransform((boneIndex == 27 || boneIndex == 29) ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
			
			for(int b = 0; b < alBones.Count; b++)
			{
				HumanBodyBones bone = alBones[b];
				Transform boneTransform = animatorComponent ? animatorComponent.GetBoneTransform(bone) : null;

				// get the finger's 1st transform
				Transform fingerBaseTransform = animatorComponent ? animatorComponent.GetBoneTransform(alBones[b - (b % 3)]) : null;
				//Vector3 vBoneDirParent = handTransform && fingerBaseTransform ? (handTransform.position - fingerBaseTransform.position).normalized : Vector3.zero;

				// get the finger's 2nd transform
				Transform baseChildTransform = fingerBaseTransform && fingerBaseTransform.childCount > 0 ? fingerBaseTransform.GetChild(0) : null;
				Vector3 vBoneDirChild = baseChildTransform && fingerBaseTransform ? (baseChildTransform.position - fingerBaseTransform.position).normalized : Vector3.zero;
				Vector3 vOrthoDirChild = Vector3.Cross(vBoneDirChild, Vector3.up).normalized;

				if(boneTransform)
				{
					fingerBoneLocalRotations[bone] = boneTransform.localRotation;

					if (vBoneDirChild != Vector3.zero) 
					{
						fingerBoneLocalAxes[bone] = boneTransform.InverseTransformDirection(vOrthoDirChild).normalized;
					} 
					else 
					{
						fingerBoneLocalAxes [bone] = Vector3.zero;
					}

//					Transform bparTransform = boneTransform ? boneTransform.parent : null;
//					Transform bchildTransform = boneTransform && boneTransform.childCount > 0 ? boneTransform.GetChild(0) : null;
//
//					// get the finger base transform (1st joint)
//					Transform fingerBaseTransform = animatorComponent.GetBoneTransform(alBones[b - (b % 3)]);
//					Vector3 vBoneDir2 = (handTransform.position - fingerBaseTransform.position).normalized;
//
//					// set the fist rotation
//					if(boneTransform && fingerBaseTransform && handTransform)
//					{
//						Vector3 vBoneDir = bchildTransform ? (bchildTransform.position - boneTransform.position).normalized :
//							(bparTransform ? (boneTransform.position - bparTransform.position).normalized : Vector3.zero);
//
//						Vector3 vOrthoDir = Vector3.Cross(vBoneDir2, vBoneDir).normalized;
//						fingerBoneLocalAxes[bone] = boneTransform.InverseTransformDirection(vOrthoDir);
//					}
				}
			}
		}

		// Restore the initial rotation
		transform.rotation = initialRotation;

        initialShoulderWidth = (bones[jointMap2boneIndex[KinectInterop.JointType.ShoulderLeft]].position -
            bones[jointMap2boneIndex[KinectInterop.JointType.ShoulderRight]].position).magnitude;
        initialHipWidth = (bones[jointMap2boneIndex[KinectInterop.JointType.HipLeft]].position -
            bones[jointMap2boneIndex[KinectInterop.JointType.HipRight]].position).magnitude;
        initialTorsoHeight = (
            Vector3.Lerp(
                bones[jointMap2boneIndex[KinectInterop.JointType.ShoulderLeft]].position,
                bones[jointMap2boneIndex[KinectInterop.JointType.ShoulderRight]].position, 0.5f) -
            Vector3.Lerp(
                bones[jointMap2boneIndex[KinectInterop.JointType.HipLeft]].position,
                bones[jointMap2boneIndex[KinectInterop.JointType.HipRight]].position, 0.5f)
                ).magnitude;

        //Debug.Log("initialShoulderWidth = " + initialShoulderWidth);
        //Debug.Log("initialHipWidth = " + initialHipWidth);
        //Debug.Log("initialTorsoHeight = " + initialTorsoHeight);
    }

    // Converts kinect joint rotation to avatar joint rotation, depending on joint initial rotation and offset rotation
    protected Quaternion Kinect2AvatarRot(Quaternion jointRotation, int boneIndex)
	{
		Quaternion newRotation = jointRotation * initialRotations[boneIndex];
		//newRotation = initialRotation * newRotation;

//		if(offsetNode != null)
//		{
//			newRotation = offsetNode.transform.rotation * newRotation;
//		}
//		else
		if (!externalRootMotion)  // fix by Mathias Parger
		{
			newRotation = initialRotation * newRotation;
		}
		
		return newRotation;
	}
	
	// Converts Kinect position to avatar skeleton position, depending on initial position, mirroring and move rate
	protected Vector3 Kinect2AvatarPos(Vector3 jointPosition, bool bMoveVertically)
	{
		float xPos = (jointPosition.x - offsetPos.x) * moveRate;
		float yPos = (jointPosition.y - offsetPos.y) * moveRate;
		float zPos = !mirroredMovement && !posRelativeToCamera ? (-jointPosition.z - offsetPos.z) * moveRate : (jointPosition.z - offsetPos.z) * moveRate;
		
		Vector3 newPosition = new Vector3(xPos, bMoveVertically ? yPos : 0f, zPos);

		Quaternion posRotation = mirroredMovement ? Quaternion.Euler (0f, 180f, 0f) * initialRotation : initialRotation;
		newPosition = posRotation * newPosition;

		if(offsetNode != null)
		{
			//newPosition += offsetNode.transform.position;
			newPosition = offsetNode.transform.position;
		}
		
		return newPosition;
	}

	// returns distance from the given transform to the underlying object. The player must be in IgnoreRaycast layer.
	protected virtual float GetTransformDistanceToGround(Transform trans)
	{
		if(!trans)
			return 0f;

//		RaycastHit hit;
//		if(Physics.Raycast(trans.position, Vector3.down, out hit, 2f, raycastLayers))
//		{
//			return -hit.distance;
//		}
//		else if(Physics.Raycast(trans.position, Vector3.up, out hit, 2f, raycastLayers))
//		{
//			return hit.distance;
//		}
//		else
//		{
//			if (trans.position.y < 0)
//				return -trans.position.y;
//			else
//				return 1000f;
//		}

		return -trans.position.y;
	}

	// returns the lower distance distance from left or right foot to the ground, or 1000f if no LF/RF transforms are found
	protected virtual float GetDistanceToGround()
	{
		if(leftFoot == null && rightFoot == null)
		{
//			Animator animatorComponent = GetComponent<Animator>();
//
//			if(animatorComponent)
//			{
//				leftFoot = animatorComponent.GetBoneTransform(HumanBodyBones.LeftFoot);
//				rightFoot = animatorComponent.GetBoneTransform(HumanBodyBones.RightFoot);
//			}

			leftFoot = GetBoneTransform(GetBoneIndexByJoint(KinectInterop.JointType.FootLeft, false));
			rightFoot = GetBoneTransform(GetBoneIndexByJoint(KinectInterop.JointType.FootRight, false));

			if (leftFoot == null || rightFoot == null) 
			{
				leftFoot = GetBoneTransform(GetBoneIndexByJoint(KinectInterop.JointType.AnkleLeft, false));
				rightFoot = GetBoneTransform(GetBoneIndexByJoint(KinectInterop.JointType.AnkleRight, false));
			}
		}

		float fDistMin = 1000f;
		float fDistLeft = leftFoot ? GetTransformDistanceToGround(leftFoot) : fDistMin;
		float fDistRight = rightFoot ? GetTransformDistanceToGround(rightFoot) : fDistMin;
		fDistMin = Mathf.Abs(fDistLeft) < Mathf.Abs(fDistRight) ? fDistLeft : fDistRight;

		if(fDistMin == 1000f)
		{
			fDistMin = 0f; // fFootDistanceInitial;
		}

//		Debug.Log (string.Format ("LFootY: {0:F2}, Dist: {1:F2}, RFootY: {2:F2}, Dist: {3:F2}, Min: {4:F2}", leftFoot ? leftFoot.position.y : 0f, fDistLeft,
//						rightFoot ? rightFoot.position.y : 0f, fDistRight, fDistMin));

		return fDistMin;
	}

//	protected void OnCollisionEnter(Collision col)
//	{
//		Debug.Log("Collision entered");
//	}
//
//	protected void OnCollisionExit(Collision col)
//	{
//		Debug.Log("Collision exited");
//	}
	
	// dictionaries to speed up bones' processing
	// the author of the terrific idea for kinect-joints to mecanim-bones mapping
	// along with its initial implementation, including following dictionary is
	// Mikhail Korchun (korchoon@gmail.com). Big thanks to this guy!
	protected static readonly Dictionary<int, HumanBodyBones> boneIndex2MecanimMap = new Dictionary<int, HumanBodyBones>
	{
		{0, HumanBodyBones.Hips},
		{1, HumanBodyBones.Spine},
        {2, HumanBodyBones.Chest},
		{3, HumanBodyBones.Neck},
		{4, HumanBodyBones.Head},
		
		{5, HumanBodyBones.LeftUpperArm},
		{6, HumanBodyBones.LeftLowerArm},
		{7, HumanBodyBones.LeftHand},
//		{8, HumanBodyBones.LeftIndexProximal},
//		{9, HumanBodyBones.LeftIndexIntermediate},
//		{10, HumanBodyBones.LeftThumbProximal},
		
		{11, HumanBodyBones.RightUpperArm},
		{12, HumanBodyBones.RightLowerArm},
		{13, HumanBodyBones.RightHand},
//		{14, HumanBodyBones.RightIndexProximal},
//		{15, HumanBodyBones.RightIndexIntermediate},
//		{16, HumanBodyBones.RightThumbProximal},
		
		{17, HumanBodyBones.LeftUpperLeg},
		{18, HumanBodyBones.LeftLowerLeg},
		{19, HumanBodyBones.LeftFoot},
//		{20, HumanBodyBones.LeftToes},
		
		{21, HumanBodyBones.RightUpperLeg},
		{22, HumanBodyBones.RightLowerLeg},
		{23, HumanBodyBones.RightFoot},
//		{24, HumanBodyBones.RightToes},
		
		{25, HumanBodyBones.LeftShoulder},
		{26, HumanBodyBones.RightShoulder},
		{27, HumanBodyBones.LeftIndexProximal},
		{28, HumanBodyBones.RightIndexProximal},
		{29, HumanBodyBones.LeftThumbProximal},
		{30, HumanBodyBones.RightThumbProximal},
	};
	
	protected static readonly Dictionary<int, KinectInterop.JointType> boneIndex2JointMap = new Dictionary<int, KinectInterop.JointType>
	{
		{0, KinectInterop.JointType.SpineBase},
		{1, KinectInterop.JointType.SpineMid},
		{2, KinectInterop.JointType.SpineShoulder},
		{3, KinectInterop.JointType.Neck},
		{4, KinectInterop.JointType.Head},
		
		{5, KinectInterop.JointType.ShoulderLeft},
		{6, KinectInterop.JointType.ElbowLeft},
		{7, KinectInterop.JointType.WristLeft},
		{8, KinectInterop.JointType.HandLeft},
		
		{9, KinectInterop.JointType.HandTipLeft},
		{10, KinectInterop.JointType.ThumbLeft},
		
		{11, KinectInterop.JointType.ShoulderRight},
		{12, KinectInterop.JointType.ElbowRight},
		{13, KinectInterop.JointType.WristRight},
		{14, KinectInterop.JointType.HandRight},
		
		{15, KinectInterop.JointType.HandTipRight},
		{16, KinectInterop.JointType.ThumbRight},
		
		{17, KinectInterop.JointType.HipLeft},
		{18, KinectInterop.JointType.KneeLeft},
		{19, KinectInterop.JointType.AnkleLeft},
		{20, KinectInterop.JointType.FootLeft},
		
		{21, KinectInterop.JointType.HipRight},
		{22, KinectInterop.JointType.KneeRight},
		{23, KinectInterop.JointType.AnkleRight},
		{24, KinectInterop.JointType.FootRight},
	};
	
	protected static readonly Dictionary<int, List<KinectInterop.JointType>> specIndex2JointMap = new Dictionary<int, List<KinectInterop.JointType>>
	{
		{25, new List<KinectInterop.JointType> {KinectInterop.JointType.ShoulderLeft, KinectInterop.JointType.SpineShoulder} },
		{26, new List<KinectInterop.JointType> {KinectInterop.JointType.ShoulderRight, KinectInterop.JointType.SpineShoulder} },
		{27, new List<KinectInterop.JointType> {KinectInterop.JointType.HandTipLeft, KinectInterop.JointType.HandLeft} },
		{28, new List<KinectInterop.JointType> {KinectInterop.JointType.HandTipRight, KinectInterop.JointType.HandRight} },
		{29, new List<KinectInterop.JointType> {KinectInterop.JointType.ThumbLeft, KinectInterop.JointType.HandLeft} },
		{30, new List<KinectInterop.JointType> {KinectInterop.JointType.ThumbRight, KinectInterop.JointType.HandRight} },
	};
	
	protected static readonly Dictionary<int, KinectInterop.JointType> boneIndex2MirrorJointMap = new Dictionary<int, KinectInterop.JointType>
	{
		{0, KinectInterop.JointType.SpineBase},
		{1, KinectInterop.JointType.SpineMid},
		{2, KinectInterop.JointType.SpineShoulder},
		{3, KinectInterop.JointType.Neck},
		{4, KinectInterop.JointType.Head},
		
		{5, KinectInterop.JointType.ShoulderRight},
		{6, KinectInterop.JointType.ElbowRight},
		{7, KinectInterop.JointType.WristRight},
		{8, KinectInterop.JointType.HandRight},
		
		{9, KinectInterop.JointType.HandTipRight},
		{10, KinectInterop.JointType.ThumbRight},
		
		{11, KinectInterop.JointType.ShoulderLeft},
		{12, KinectInterop.JointType.ElbowLeft},
		{13, KinectInterop.JointType.WristLeft},
		{14, KinectInterop.JointType.HandLeft},
		
		{15, KinectInterop.JointType.HandTipLeft},
		{16, KinectInterop.JointType.ThumbLeft},
		
		{17, KinectInterop.JointType.HipRight},
		{18, KinectInterop.JointType.KneeRight},
		{19, KinectInterop.JointType.AnkleRight},
		{20, KinectInterop.JointType.FootRight},
		
		{21, KinectInterop.JointType.HipLeft},
		{22, KinectInterop.JointType.KneeLeft},
		{23, KinectInterop.JointType.AnkleLeft},
		{24, KinectInterop.JointType.FootLeft},
	};
	
	protected static readonly Dictionary<int, List<KinectInterop.JointType>> specIndex2MirrorMap = new Dictionary<int, List<KinectInterop.JointType>>
	{
		{25, new List<KinectInterop.JointType> {KinectInterop.JointType.ShoulderRight, KinectInterop.JointType.SpineShoulder} },
		{26, new List<KinectInterop.JointType> {KinectInterop.JointType.ShoulderLeft, KinectInterop.JointType.SpineShoulder} },
		{27, new List<KinectInterop.JointType> {KinectInterop.JointType.HandTipRight, KinectInterop.JointType.HandRight} },
		{28, new List<KinectInterop.JointType> {KinectInterop.JointType.HandTipLeft, KinectInterop.JointType.HandLeft} },
		{29, new List<KinectInterop.JointType> {KinectInterop.JointType.ThumbRight, KinectInterop.JointType.HandRight} },
		{30, new List<KinectInterop.JointType> {KinectInterop.JointType.ThumbLeft, KinectInterop.JointType.HandLeft} },
	};
	
	protected static readonly Dictionary<KinectInterop.JointType, int> jointMap2boneIndex = new Dictionary<KinectInterop.JointType, int>
	{
		{KinectInterop.JointType.SpineBase, 0},
		{KinectInterop.JointType.SpineMid, 1},
		{KinectInterop.JointType.SpineShoulder, 2},
		{KinectInterop.JointType.Neck, 3},
		{KinectInterop.JointType.Head, 4},
		
		{KinectInterop.JointType.ShoulderLeft, 5},
		{KinectInterop.JointType.ElbowLeft, 6},
		{KinectInterop.JointType.WristLeft, 7},
		{KinectInterop.JointType.HandLeft, 8},
		
		{KinectInterop.JointType.HandTipLeft, 9},
		{KinectInterop.JointType.ThumbLeft, 10},
		
		{KinectInterop.JointType.ShoulderRight, 11},
		{KinectInterop.JointType.ElbowRight, 12},
		{KinectInterop.JointType.WristRight, 13},
		{KinectInterop.JointType.HandRight, 14},
		
		{KinectInterop.JointType.HandTipRight, 15},
		{KinectInterop.JointType.ThumbRight, 16},
		
		{KinectInterop.JointType.HipLeft, 17},
		{KinectInterop.JointType.KneeLeft, 18},
		{KinectInterop.JointType.AnkleLeft, 19},
		{KinectInterop.JointType.FootLeft, 20},
		
		{KinectInterop.JointType.HipRight, 21},
		{KinectInterop.JointType.KneeRight, 22},
		{KinectInterop.JointType.AnkleRight, 23},
		{KinectInterop.JointType.FootRight, 24},
	};
	
	protected static readonly Dictionary<KinectInterop.JointType, int> mirrorJointMap2boneIndex = new Dictionary<KinectInterop.JointType, int>
	{
		{KinectInterop.JointType.SpineBase, 0},
		{KinectInterop.JointType.SpineMid, 1},
		{KinectInterop.JointType.SpineShoulder, 2},
		{KinectInterop.JointType.Neck, 3},
		{KinectInterop.JointType.Head, 4},
		
		{KinectInterop.JointType.ShoulderRight, 5},
		{KinectInterop.JointType.ElbowRight, 6},
		{KinectInterop.JointType.WristRight, 7},
		{KinectInterop.JointType.HandRight, 8},
		
		{KinectInterop.JointType.HandTipRight, 9},
		{KinectInterop.JointType.ThumbRight, 10},
		
		{KinectInterop.JointType.ShoulderLeft, 11},
		{KinectInterop.JointType.ElbowLeft, 12},
		{KinectInterop.JointType.WristLeft, 13},
		{KinectInterop.JointType.HandLeft, 14},
		
		{KinectInterop.JointType.HandTipLeft, 15},
		{KinectInterop.JointType.ThumbLeft, 16},
		
		{KinectInterop.JointType.HipRight, 17},
		{KinectInterop.JointType.KneeRight, 18},
		{KinectInterop.JointType.AnkleRight, 19},
		{KinectInterop.JointType.FootRight, 20},
		
		{KinectInterop.JointType.HipLeft, 21},
		{KinectInterop.JointType.KneeLeft, 22},
		{KinectInterop.JointType.AnkleLeft, 23},
		{KinectInterop.JointType.FootLeft, 24},
	};


	protected static readonly Dictionary<int, List<HumanBodyBones>> specialIndex2MultiBoneMap = new Dictionary<int, List<HumanBodyBones>>
	{
		{27, new List<HumanBodyBones> {  // left fingers
				HumanBodyBones.LeftIndexProximal,
				HumanBodyBones.LeftIndexIntermediate,
				HumanBodyBones.LeftIndexDistal,
				HumanBodyBones.LeftMiddleProximal,
				HumanBodyBones.LeftMiddleIntermediate,
				HumanBodyBones.LeftMiddleDistal,
				HumanBodyBones.LeftRingProximal,
				HumanBodyBones.LeftRingIntermediate,
				HumanBodyBones.LeftRingDistal,
				HumanBodyBones.LeftLittleProximal,
				HumanBodyBones.LeftLittleIntermediate,
				HumanBodyBones.LeftLittleDistal,
			}},
		{28, new List<HumanBodyBones> {  // right fingers
				HumanBodyBones.RightIndexProximal,
				HumanBodyBones.RightIndexIntermediate,
				HumanBodyBones.RightIndexDistal,
				HumanBodyBones.RightMiddleProximal,
				HumanBodyBones.RightMiddleIntermediate,
				HumanBodyBones.RightMiddleDistal,
				HumanBodyBones.RightRingProximal,
				HumanBodyBones.RightRingIntermediate,
				HumanBodyBones.RightRingDistal,
				HumanBodyBones.RightLittleProximal,
				HumanBodyBones.RightLittleIntermediate,
				HumanBodyBones.RightLittleDistal,
			}},
		{29, new List<HumanBodyBones> {  // left thumb
				HumanBodyBones.LeftThumbProximal,
				HumanBodyBones.LeftThumbIntermediate,
				HumanBodyBones.LeftThumbDistal,
			}},
		{30, new List<HumanBodyBones> {  // right thumb
				HumanBodyBones.RightThumbProximal,
				HumanBodyBones.RightThumbIntermediate,
				HumanBodyBones.RightThumbDistal,
			}},
	};


	protected static readonly Dictionary<int, HumanBodyBones> fingerIndex2MecanimMap = new Dictionary<int, HumanBodyBones>
	{
		{0, HumanBodyBones.LeftThumbProximal},
		{1, HumanBodyBones.LeftThumbIntermediate},
		{2, HumanBodyBones.LeftThumbDistal},

		{3, HumanBodyBones.LeftIndexProximal},
		{4, HumanBodyBones.LeftIndexIntermediate},
		{5, HumanBodyBones.LeftIndexDistal},

		{6, HumanBodyBones.LeftMiddleProximal},
		{7, HumanBodyBones.LeftMiddleIntermediate},
		{8, HumanBodyBones.LeftMiddleDistal},

		{9, HumanBodyBones.LeftRingProximal},
		{10, HumanBodyBones.LeftRingIntermediate},
		{11, HumanBodyBones.LeftRingDistal},

		{12, HumanBodyBones.LeftLittleProximal},
		{13, HumanBodyBones.LeftLittleIntermediate},
		{14, HumanBodyBones.LeftLittleDistal},

		{15, HumanBodyBones.RightThumbProximal},
		{16, HumanBodyBones.RightThumbIntermediate},
		{17, HumanBodyBones.RightThumbDistal},

		{18, HumanBodyBones.RightIndexProximal},
		{19, HumanBodyBones.RightIndexIntermediate},
		{20, HumanBodyBones.RightIndexDistal},

		{21, HumanBodyBones.RightMiddleProximal},
		{22, HumanBodyBones.RightMiddleIntermediate},
		{23, HumanBodyBones.RightMiddleDistal},

		{24, HumanBodyBones.RightRingProximal},
		{25, HumanBodyBones.RightRingIntermediate},
		{26, HumanBodyBones.RightRingDistal},

		{27, HumanBodyBones.RightLittleProximal},
		{28, HumanBodyBones.RightLittleIntermediate},
		{29, HumanBodyBones.RightLittleDistal}
	};

    public void SaveConfigData()
    {
        AvatarControllerConfigData acConfigData = AvatarControllerConfigData.Instance;
        string acName = this.name;
        // Clean up the name in case this is a (Clone) object.
        string cloneStr = "(Clone)";
        int cloneIdx = acName.IndexOf(cloneStr);
        if (cloneIdx >= 0)
            acName = acName.Remove(cloneIdx);

        acConfigData.entries.Remove(acName);
        acConfigData.entries.Add(acName, new AvatarControllerEntry(this));
        acConfigData.Save();
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
        acConfigData.Load();
        LoadConfigData();
    }

    public void LoadConfigData()
    {
        AvatarControllerConfigData acConfigData = AvatarControllerConfigData.Instance;
        string acName = this.name;

        // Clean up the name in case this is a (Clone) object.
        string cloneStr = "(Clone)";
        int cloneIdx = acName.IndexOf(cloneStr);
        if (cloneIdx >= 0)
            acName = acName.Remove(cloneIdx);

        AvatarControllerEntry data = null;
        if (acConfigData.entries.ContainsKey(acName))
        {
            data = acConfigData.entries[acName];
        }
        else
        {
            Debug.LogWarning("Unable to find tuning values for " + acName + ", attempting nonLive values");
            // Attempt to load non-live tuning values if this is a live model
            string liveStr = "_live";
            int liveIdx = acName.IndexOf(liveStr);
            if (liveIdx >= 0)
                acName = acName.Remove(liveIdx);

            if (acConfigData.entries.ContainsKey(acName))
            {
                data = acConfigData.entries[acName];
            }
            else
            {
                // Load default male or female tuning values.
                string[] parts = this.name.Split('_');
                string gender = parts[2].ToLower();
                if (gender.StartsWith("f"))
                {
                    Debug.LogWarning("Loading default female tuning values");
                    data = acConfigData.entries["mr_sun_f_nina"];
                }
                else if (gender.StartsWith("m"))
                {
                    Debug.LogWarning("Loading default male tuning values");
                    data = acConfigData.entries["mr_sun_m_anthony"];
                }
            }
        }

        if (data != null)
            data.PopulateTo(this);
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(AvatarController))]
public class AvatarControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AvatarController myScript = (AvatarController)target;
        if (GUILayout.Button("Save Config Data"))
        {
            myScript.SaveConfigData();
        }
        if (GUILayout.Button("Load Config Data"))
        {
            myScript.LoadConfigData();
        }
    }
}
#endif


