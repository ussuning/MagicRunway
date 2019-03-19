using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarControllerScaler : MonoBehaviour {
    internal AvatarController ac;

    private float _origUpperArmLength = 0;
    protected float origUpperArmLength // assumes symmetry
    {
        get
        {
            if (_origUpperArmLength == 0)
                _origUpperArmLength = (ac.GetJointInitialPosition(KinectInterop.JointType.ShoulderLeft) -
               ac.GetJointInitialPosition(KinectInterop.JointType.ElbowLeft)).magnitude;
            return _origUpperArmLength;
        }
    }

    private float _origLowerArmLength = 0;
    protected float origLowerArmLength  // assumes symmetry
    {
        get
        {
            if (_origLowerArmLength == 0)
                _origLowerArmLength = (ac.GetJointInitialPosition(KinectInterop.JointType.ElbowLeft) -
                        ac.GetJointInitialPosition(KinectInterop.JointType.WristLeft)).magnitude;
            return _origLowerArmLength;
        }
    }

    private float _origThighLength = 0;
    protected float origThighLength  // assumes symmetry
    {
        get
        {
            if (_origThighLength == 0)
                _origThighLength = (ac.GetJointInitialPosition(KinectInterop.JointType.HipLeft) -
                    ac.GetJointInitialPosition(KinectInterop.JointType.KneeLeft)).magnitude;
            return _origThighLength;
        }
    }

    private float _origShinLength = 0;
    protected float origShinLength  // assumes symmetry
    {
        get
        {
            if (_origShinLength == 0)
                _origShinLength = (ac.GetJointInitialPosition(KinectInterop.JointType.AnkleLeft) -
                    ac.GetJointInitialPosition(KinectInterop.JointType.KneeLeft)).magnitude;
            return _origShinLength;
        }
    }

    // Apply the rotations tracked by kinect to the joints.
    internal void ScaleBone(KinectInterop.JointType joint, int boneIndex)
    {
        Transform boneTransform = ac.GetBone(boneIndex);

        // Correct scaling
        switch (joint)
        {
            case KinectInterop.JointType.ShoulderLeft:
            case KinectInterop.JointType.ShoulderRight:
                {
                    Transform elbowJoint = ac.GetChildBone(boneTransform);
                    float upperArmLength = (elbowJoint.position - boneTransform.position).magnitude;    

                    ac.SetBoneScale(boneTransform,
                        new Vector3(1f, upperArmLength / origUpperArmLength, 1f));
                    
                    Transform wristJoint = ac.GetChildBone(elbowJoint);
                    float lowerArmLength = (wristJoint.position - elbowJoint.position).magnitude;
                    float elbowScaleY = lowerArmLength / origLowerArmLength;
                    ac.SetBoneScale(elbowJoint,
                        new Vector3(
                            1f,
                            elbowScaleY,
                            1f));
                    ac.resetJointScale(wristJoint, true);
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
                    Transform kneeBone = ac.GetChildBone(boneTransform);
                    Transform ankleBone = ac.GetChildBone(kneeBone);

                    Vector3 thighForward = kneeBone.position - boneTransform.position;
                    float thighLength = thighForward.magnitude;

                    //Debug.Log("hipLeftY scale = " + (thighLength / origThighLength));
                    float thighScaleFactor = thighLength / origThighLength;
                    //resetJointScale(ref boneTransform);
                    Transform hipBone = ac.GetParentBone(boneTransform);
                    ac.SetBoneScale(boneTransform,
                        new Vector3(1f, thighScaleFactor, 1f));


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
                    ac.SetBoneScale(kneeBone,
                        new Vector3(1f, shinScaleFactor, 1f));
                    //resetJointScale(ref ankleLeft);
                    ac.resetJointScale(ankleBone, true);
                    //ac.SetBoneScale(ankleBone, Vector3.one);
                }
                break;
        }

        //boneTransform.localScale = Vector3.Lerp(oldScale, boneTransform.localScale, 0.65f);

    }

    internal void ScaleTorso(float hipWidthFactor, float shoulderWidthFactor, float hipZFactor)
    {
        AvatarControllerClassic ac = this.ac as AvatarControllerClassic;
        if (ac == null)
        {
            Debug.LogError("Can't perform ScaleTorso, requires AvatarControllerClassic");
            return;
        }

        if (hipWidthFactor == 0f || shoulderWidthFactor == 0f)
            return;

        float hipScaleX = hipWidthFactor;
        float shoulderScaleX = shoulderWidthFactor;
        ac.SetBoneScale(ac.HipCenter, new Vector3(hipScaleX, 1f, hipScaleX * hipZFactor));

        for (int i= 0; i < ac.HipCenter.childCount; i++)
        {
            Transform child = ac.GetChildBone(ac.HipCenter, i);
            ac.SetBoneScale(child, Vector3.one);
        }
        //Debug.Log("HipCenter.lossyScale " + HipCenter.lossyScale);

        // Unscale so that knee/ankles are normal (Vector3.one)
        //resetJointScale(ref FootLeft);//.localScale = new Vector3(1f / KneeLeft.parent.lossyScale.x, 1f / KneeLeft.parent.lossyScale.y, 1f / KneeLeft.parent.lossyScale.z);
        //resetJointScale(ref FootRight);//.localScale = new Vector3(1f / KneeRight.parent.lossyScale.x, 1f / KneeRight.parent.lossyScale.y, 1f / KneeRight.parent.lossyScale.z);
        //Debug.Log("KneeLeft.lossyScale " + KneeLeft.lossyScale);
        //Spine.localScale = new Vector3(hipWidthFactor, 1, 1);

        //Vector3 hipToShoudlerCenter = ac.ShoulderCenter.position - ac.HipCenter.position;
        //foreach (Transform spineSegment in new Transform[] { ac.Spine, ac.SpineMid })
        //    if (spineSegment != null)
        //    {
        //        Vector3 hipToSpine = spineSegment.position - ac.HipCenter.position;

        //        float dot = Mathf.Clamp01(Vector3.Dot(hipToSpine, hipToShoudlerCenter));
        //        Debug.Log("dot" + spineSegment.name + " = " + dot);
        //        float spineScaleX = Mathf.Lerp(hipWidthFactor, shoulderWidthFactor, dot);
        //        float spineZFactor = Mathf.Lerp(hipZFactor, 1f, dot);
        //        ac.SetBoneScale(spineSegment, new Vector3(spineScaleX, spineSegment.localScale.y, spineScaleX * spineZFactor));
        //    }

        //float midScaleX = (hipWidthFactor + shoulderWidthFactor) / 2.0f;
        //float midScaleZ = (hipWidthFactor * hipZFactor + 1f) / 2.0f;
        //resetJointScale(ref SpineMid);
        //SetBoneScale(SpineMid, new Vector3(SpineMid.localScale.x * midScaleX, SpineMid.localScale.y, SpineMid.localScale.z * midScaleZ));
        //Debug.Log("SpineMid.lossyScale " + SpineMid.lossyScale);

//        ac.resetJointScale(ac.ShoulderCenter);
        ac.SetBoneScale(ac.ShoulderCenter, new Vector3(shoulderScaleX, 1f, shoulderScaleX));
        //Debug.Log("ShoulderCenter.lossyScale " + ShoulderCenter.lossyScale);
        //Debug.Log("hipWidthFactor " + hipWidthFactor);
        //Debug.Log("shoulderWidthFactor " + shoulderWidthFactor);
        for (int i= 0; i< ac.ShoulderCenter.childCount; i++)
        {
            Transform child = ac.GetChildBone(ac.ShoulderCenter, i);
            ac.SetBoneScale(child, Vector3.one);
        }

    }
}
