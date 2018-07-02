using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseRecognizingAgent : Agent {

    private KinectManager manager;
    private long KinectUserId;

    private int estPoseIdx;

    public override void InitializeAgent()
    {

    }

    public override void CollectObservations()
    {
        if (manager.IsUserInKinectView(KinectUserId))
        {
            if (SystemConfigs.CollectUserRotation)
            {
                Vector3 userRot = manager.GetUserOrientation(KinectUserId, false).eulerAngles;
                userRot = NormalizeAngles(userRot);
                AddVectorObs(userRot);
            }
            if (SystemConfigs.CollectJointDirectionData)
            {
                for (int i = 0; i < SystemConfigs.DetectedJoints.Length; i++)
                {
                    if (manager.IsJointTracked(KinectUserId, (int)SystemConfigs.DetectedJoints[i]))
                    {
                        int JointIdx = (int)SystemConfigs.DetectedJoints[i];
                        Vector3 JointDirection = manager.GetJointDirection(KinectUserId, JointIdx);
                        AddVectorObs(JointDirection);
                    }
                }
            }
            if (SystemConfigs.CollectJointOrientationData)
            {
                for (int i = 0; i < SystemConfigs.DetectedJoints.Length; i++)
                {
                    if (manager.IsJointTracked(KinectUserId, (int)SystemConfigs.DetectedJoints[i]))
                    {
                        int JointIdx = (int)SystemConfigs.DetectedJoints[i];
                        Vector3 JointOrientation = manager.GetJointOrientation(KinectUserId, JointIdx).eulerAngles;
                        JointOrientation = NormalizeAngles(JointOrientation);
                        AddVectorObs(JointOrientation);
                    }
                }
            }
        }
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {

    }

    public override void AgentOnDone()
    {
        Destroy(gameObject);
    }

    public override void AgentReset()
    {
        estPoseIdx = 0;
    }

    //Normalizing to [-1, 1]
    Vector3 NormalizeAngles(Vector3 angles)
    {
        return (angles / 180f) - Vector3.one;
    }
}
