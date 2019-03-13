using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AvatarControllerTuner : MonoBehaviour
{
    internal AvatarController ac;

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

    float lastShoulderAdjustWidthFactor = 0.0f;
    float lastHipAdjustWidthFactor = 0.0f;

    bool lastArmsRaised = false;
    bool needTuningReset = false;


    public void CalibrateHipShoulders()
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

        Vector3 shoulderLeft = ac.GetRawJointWorldPos(KinectInterop.JointType.ShoulderLeft);
        Vector3 shoulderRight = ac.GetRawJointWorldPos(KinectInterop.JointType.ShoulderRight);
        Vector3 elbowLeft = ac.GetRawJointWorldPos(KinectInterop.JointType.ElbowLeft);
        Vector3 elbowRight = ac.GetRawJointWorldPos(KinectInterop.JointType.ElbowRight);
        Transform spineTransform = ac.GetBone(KinectInterop.JointType.SpineMid);
        bool leftArmRaised = spineTransform.InverseTransformPoint(elbowLeft).y > spineTransform.InverseTransformPoint(shoulderLeft).y;
        bool rightArmRaised = spineTransform.InverseTransformPoint(elbowRight).y > spineTransform.InverseTransformPoint(shoulderRight).y;
        bool armsRaised = leftArmRaised || rightArmRaised;
        //if (armsRaised)
        //    Debug.Log("Arms Raised @ " + Time.time);
        if (armsRaised == false && lastArmsRaised == true)
            needTuningReset = true;

        lastArmsRaised = armsRaised;

        float facingCamera = Vector3.Dot(spineTransform.forward.normalized, -ac.posRelativeToCamera.transform.forward.normalized);
        //Debug.Log("facingCamera = " + facingCamera);

        Vector3 hipLeft = ac.GetRawJointWorldPos(KinectInterop.JointType.HipLeft);
        Vector3 hipRight = ac.GetRawJointWorldPos(KinectInterop.JointType.HipRight);
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
        float camToUserDistSqrd = (spineTransform.position - ac.posRelativeToCamera.transform.position).sqrMagnitude;
        float minDistSqrd = 1f; //1 meter
        float maxDistSqrd = 16f; // 4 meters

        if ((leftArmRaised && rightArmRaised) == false &&// Raised arms creates distortion
            facingCamera > 0.9f && // rotating shoulders away from camera creates distortion also
            (camToUserDistSqrd > minDistSqrd && camToUserDistSqrd < maxDistSqrd)) // distance can create inaccurate measurements
        {
            hipWidthFactor = currHipWidthFactor * hipAdjustWidthFactor;
            shoulderWidthFactor = shoulderAdjustWidthFactor;
        }
    }

    internal void InitializeSkeletalInfo()
    {
        initialShoulderWidth = (ac.GetBone(KinectInterop.JointType.ShoulderLeft).position -
            ac.GetBone(KinectInterop.JointType.ShoulderRight).position).magnitude;
        initialHipWidth = (ac.GetBone(KinectInterop.JointType.HipLeft).position -
            ac.GetBone(KinectInterop.JointType.HipRight).position).magnitude;

        //Debug.Log("initialShoulderWidth = " + initialShoulderWidth);
        //Debug.Log("initialHipWidth = " + initialHipWidth);
        //Debug.Log("initialTorsoHeight = " + initialTorsoHeight);
    }

    internal void PreTranslateBones(Dictionary<int, KinectInterop.JointType> boneJoints)
    {
        foreach (KeyValuePair<int, KinectInterop.JointType> boneJoint in boneJoints)
            doPreTranslateBones(boneJoint.Value, boneJoint.Key);
    }

    protected void doPreTranslateBones(KinectInterop.JointType joint, int boneIndex)
    {
        Transform boneTransform = ac.GetBone(boneIndex);

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
            //    boneTransform.position = ac.GetRawJointWorldPos(joint);
            //    Debug.Log("WristLeft = " + boneTransform.position);
            //    break;
            //case KinectInterop.JointType.WristRight:
            //    boneTransform.position = ac.GetRawJointWorldPos(joint);
            //    Debug.Log("WristRight = " + boneTransform.position);
            //    break;
            //case KinectInterop.JointType.HandLeft:
            //case KinectInterop.JointType.HandRight:
            //case KinectInterop.JointType.HandTipLeft:
            //case KinectInterop.JointType.HandTipRight:
            //// Don't override position. Just use what kinect gives us. Edit: this is a bad idea -HH
            //break;
            default:
                boneTransform.position = ac.GetRawJointWorldPos(joint);
                break;
        }

        Vector3 shoulderLeft = ac.GetRawJointWorldPos(KinectInterop.JointType.ShoulderLeft);
        Vector3 shoulderRight = ac.GetRawJointWorldPos(KinectInterop.JointType.ShoulderRight);
        Vector3 shoulderCenter = ac.GetRawJointWorldPos(KinectInterop.JointType.SpineShoulder);
        Vector3 hipLeft = ac.GetRawJointWorldPos(KinectInterop.JointType.HipLeft);
        Vector3 hipRight = ac.GetRawJointWorldPos(KinectInterop.JointType.HipRight);
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
                hipCenter = hipCenter + ac.GetBone(KinectInterop.JointType.SpineBase).up * hipUpwardOffset;
                boneTransform.position = hipCenter + dirHipFromCenter * hipWidthFactor;
                break;
            case KinectInterop.JointType.KneeLeft:
            case KinectInterop.JointType.KneeRight:
                // Move them down if thighs are straight down because kinect seems to shift knees up when legs are straight down.

                Transform hipJoint = ac.GetParentBone(boneTransform);
                Vector3 thighForward = boneTransform.position - hipJoint.position;  // kneePos - hipPos
                float straightness = Vector3.Dot(thighForward.normalized, -ac.bodyRoot.up);

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
                Vector3 shoulderUp = ac.GetBone(KinectInterop.JointType.SpineMid).up;
                Vector3 spineCenterToShoulder = boneTransform.position - shoulderCenter;
                // Get closest point along spine
                float dot = Vector3.Dot(shoulderUp, spineCenterToShoulder);
                Vector3 closestPtOnSpine = shoulderCenter + shoulderUp * dot;
                Vector3 shoulderOut = boneTransform.position - closestPtOnSpine;
                //Vector3 dirShoulderFromCenter = (joint == KinectInterop.JointType.ShoulderLeft) ? shoulderLeft - shoulderCenter : shoulderRight - shoulderCenter;
                boneTransform.position = closestPtOnSpine + shoulderOut * shoulderWidthFactor;
                //boneTransform.position += GetShoulderVerticalOffset(joint);
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
    }

    
    internal void ScaleTorso()
    {

        /*
        if (hipWidthFactor == 0f || shoulderWidthFactor == 0f)
            return;

        float hipScaleX = hipWidthFactor;
        float shoulderScaleX = shoulderWidthFactor;
        SetBoneScale(HipCenter, new Vector3(hipScaleX, HipCenter.localScale.y, hipScaleX * hipZFactor));
        //Debug.Log("HipCenter.lossyScale " + HipCenter.lossyScale);

        // Unscale so that knee/ankles are normal (Vector3.one)
        //resetJointScale(ref FootLeft);//.localScale = new Vector3(1f / KneeLeft.parent.lossyScale.x, 1f / KneeLeft.parent.lossyScale.y, 1f / KneeLeft.parent.lossyScale.z);
        //resetJointScale(ref FootRight);//.localScale = new Vector3(1f / KneeRight.parent.lossyScale.x, 1f / KneeRight.parent.lossyScale.y, 1f / KneeRight.parent.lossyScale.z);
        //Debug.Log("KneeLeft.lossyScale " + KneeLeft.lossyScale);
        //Spine.localScale = new Vector3(hipWidthFactor, 1, 1);
        Vector3 hipToShoudlerCenter = ShoulderCenter.position - HipCenter.position;
        foreach (Transform spineSegment in new Transform[] { Spine, SpineMid })
            if (spineSegment != null)
            {
                Vector3 hipToSpine = spineSegment.position - HipCenter.position;

                float dot = Mathf.Clamp01(Vector3.Dot(hipToSpine, hipToShoudlerCenter));
                Debug.Log("dot" + spineSegment.name + " = " + dot);
                float spineScaleX = Mathf.Lerp(hipWidthFactor, shoulderWidthFactor, dot);
                float spineZFactor = Mathf.Lerp(hipZFactor, 1f, dot);
                SetBoneScale(spineSegment, new Vector3(spineScaleX, spineSegment.localScale.y, spineScaleX * spineZFactor));
            }
        //float midScaleX = (hipWidthFactor + shoulderWidthFactor) / 2.0f;
        //float midScaleZ = (hipWidthFactor * hipZFactor + 1f) / 2.0f;
        //resetJointScale(ref SpineMid);
        //SetBoneScale(SpineMid, new Vector3(SpineMid.localScale.x * midScaleX, SpineMid.localScale.y, SpineMid.localScale.z * midScaleZ));
        //Debug.Log("SpineMid.lossyScale " + SpineMid.lossyScale);

        resetJointScale(ref ShoulderCenter);
        SetBoneScale(ShoulderCenter, new Vector3(ShoulderCenter.localScale.x * shoulderScaleX, ShoulderCenter.localScale.y, ShoulderCenter.localScale.z));
        //Debug.Log("ShoulderCenter.lossyScale " + ShoulderCenter.lossyScale);
        //Debug.Log("hipWidthFactor " + hipWidthFactor);
        //Debug.Log("shoulderWidthFactor " + shoulderWidthFactor);
        for (int i = 0; i < ShoulderCenter.childCount; i++)
        {
            Transform child = ShoulderCenter.GetChild(i);
            resetJointScale(ref child);
        }
        */
    }

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
[CustomEditor(typeof(AvatarControllerTuner))]
public class AvatarControllerTunerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AvatarControllerTuner myScript = (AvatarControllerTuner)target;
        if (GUILayout.Button("Save Config Data"))
        {
            myScript.SaveConfigData();
        }
        if (GUILayout.Button("Load Config Data"))
        {
            myScript.LoadConfigData();
        }
        GUILayout.Label("hipWidthFactor=" + myScript.hipWidthFactor);
        GUILayout.Label("shoulderWidthFactor=" + myScript.shoulderWidthFactor);
    }

    //void OnSceneGUI()
    //{
    //    AvatarController ac = (AvatarController)target;
    //    //Handles.color = Color.red;
    //    //Handles.DrawLine(ac.elbowPos, ac.elbowPos + ac.elbowOutty.normalized);
    //    Handles.color = Color.green;
    //    Handles.DrawLine(ac.shPos, ac.shPos + ac.shOutty.normalized);
    //    //Handles.color = Color.blue;
    //    //Handles.DrawLine(ac.elbowPos, ac.elbowPos + ac.spineOutty.normalized);


    //    //Handles.color = Color.magenta;
    //    //Handles.DrawLine(ac.elbowPos, ac.elbowPos + ac.finalOutty.normalized);
    //    //Handles.color = Color.cyan;
    //    //Handles.DrawLine(ac.elbowPos, ac.elbowPos + ac.elbowForward.normalized);
    //    //Handles.color = Color.yellow;
    //    //Handles.DrawDottedLine(ac.shPos, ac.shPos + ac.shElbowForward.normalized, 5f);

    //}
}
#endif
