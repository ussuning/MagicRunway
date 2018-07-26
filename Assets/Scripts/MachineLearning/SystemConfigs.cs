using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemConfigs
{
    public static bool CollectUserRotation = false;
    public static bool CollectJointDirectionData = false;
    public static bool CollectJointOrientationData = true;
    /*
    public static KinectInterop.JointType[] DetectedJoints = {
        KinectInterop.JointType.SpineBase, KinectInterop.JointType.SpineMid, KinectInterop.JointType.Neck, KinectInterop.JointType.Head,
        KinectInterop.JointType.ShoulderLeft, KinectInterop.JointType.ElbowLeft, KinectInterop.JointType.WristLeft, KinectInterop.JointType.HandLeft,
        KinectInterop.JointType.ShoulderRight, KinectInterop.JointType.ElbowRight, KinectInterop.JointType.WristRight, KinectInterop.JointType.HandRight,
        KinectInterop.JointType.HipLeft, KinectInterop.JointType.KneeLeft, KinectInterop.JointType.AnkleLeft, KinectInterop.JointType.FootLeft,
        KinectInterop.JointType.HipRight, KinectInterop.JointType.KneeRight, KinectInterop.JointType.AnkleRight, KinectInterop.JointType.FootRight,
        KinectInterop.JointType.SpineShoulder, KinectInterop.JointType.HandTipLeft, KinectInterop.JointType.HandTipRight
    };
    */
    public static KinectInterop.JointType[] DetectedJoints = {
        /*KinectInterop.JointType.SpineBase, KinectInterop.JointType.SpineMid, KinectInterop.JointType.Neck, KinectInterop.JointType.Head,*/
        KinectInterop.JointType.ShoulderLeft, KinectInterop.JointType.ElbowLeft,
        KinectInterop.JointType.WristLeft, KinectInterop.JointType.HandLeft,
        KinectInterop.JointType.ShoulderRight, KinectInterop.JointType.ElbowRight,
        KinectInterop.JointType.WristRight, KinectInterop.JointType.HandRight,
        KinectInterop.JointType.HipLeft, KinectInterop.JointType.KneeLeft,
        /*KinectInterop.JointType.AnkleLeft, KinectInterop.JointType.FootLeft,*/
        KinectInterop.JointType.HipRight, KinectInterop.JointType.KneeRight,
        /*KinectInterop.JointType.AnkleRight, KinectInterop.JointType.FootRight,*/
        /*KinectInterop.JointType.SpineShoulder, KinectInterop.JointType.HandTipLeft, KinectInterop.JointType.HandTipRight*/
    };

    public static float PoseEstimationTimeFrame = 0.1f; //Time between each pose estimation (to remove outlier estimations)
    public static float PosingTime = 0.5f;
    public static float ComboPoseTime = 2f;
}