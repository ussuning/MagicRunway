using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPoseRecognizingAgent : Agent {
    
    private KinectManager kinectMgr;
    private long KinectUserId;

    private BrainDataManager brainMgr;
    private int poseID;

    private bool isPoseMatched = false;

    private float poseConfidence;

    PoseParameter pose;

    public void Init(long userID, int PoseID)
    {
        kinectMgr = KinectManager.Instance;
        brainMgr = BrainDataManager.Instance;
        KinectUserId = userID;
        poseID = PoseID;

        pose = brainMgr.GetPoseInfo(poseID);

        if (agentParameters == null)
            agentParameters = new AgentParameters();
    }

    void Update()
    {
        isPoseMatched = !PoseMgr.Instance.IsInNewPoseCooldown && poseConfidence >= pose.min_confidence;

        if (isPoseMatched)
        {
            EventMsgDispatcher.Instance.TriggerEvent(EventDef.User_Pose_Detected, KinectUserId, poseConfidence);
            isPoseMatched = false;
            //Debug.Log(string.Format("User {0} : Agent {1}: action = {2} @ {3}", this.name, poseID, poseConfidence, poseCDTimeEllapsed));
        }
    }

    public override void InitializeAgent()
    {
    }

    public override void CollectObservations()
    {
        if (kinectMgr.IsUserInKinectView(KinectUserId))
        {
            for (int i = 0; i < pose.num_joint_detections; i++)
            {
                int JointIdx = pose.joint_ids[i];
                if (kinectMgr.IsJointTracked(KinectUserId, JointIdx))
                {
                    //Vector3 JointOrientation = kinectMgr.GetJointOrientation(KinectUserId, JointIdx).eulerAngles;
                    //JointOrientation = NormalizeAngles(JointOrientation);
                    //AddVectorObs(JointOrientation);

                    //Vector3 JointDirection = kinectMgr.GetJointDirection(KinectUserId, JointIdx);
                    //AddVectorObs(JointDirection);

                    Quaternion JointOrientation = kinectMgr.GetJointOrientation(KinectUserId, JointIdx);
                    AddVectorObs(JointOrientation);
                }
            }
        }
        else
        {
            for (int i = 0; i < pose.num_joint_detections; i++)
            {
                //AddVectorObs(Vector3.zero);
                AddVectorObs(Quaternion.identity);
            }
        }
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        poseConfidence = vectorAction[0];
       // Debug.Log(string.Format("User {0} : Agent {1}: action = {2}", this.name, poseID, poseConfidence));
    }

    public override void AgentOnDone()
    {
        //Destroy(gameObject);
    }

    public override void AgentReset()
    {
        isPoseMatched = false;
    }
    //Normalizing to [-1, 1]
    Vector3 NormalizeAngles(Vector3 angles)
    {
        return (angles / 180f) - Vector3.one;
    }
}
