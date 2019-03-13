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
                    ac.SetBoneScale(wristJoint, Vector3.one);
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
                    ac.SetBoneScale(ankleBone, Vector3.one);
                }
                break;
        }

        //boneTransform.localScale = Vector3.Lerp(oldScale, boneTransform.localScale, 0.65f);

    }
}
