using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPoseRecognizingAgent : Agent {
    
    private KinectManager kinectMgr;
    private BrainDataManager brainMgr;

    private int userIdx;
    private long userId
    {
        get
        {
            return kinectMgr.GetUserIdByIndex(userIdx);
        }
    }

    private int poseID;

    private bool isPoseMatched = false;
    private bool isInCooldown = false;

    private float poseConfidence;

    PoseParameter pose;

    public void Init(int userIndex, int PoseID)
    {
        this.kinectMgr = KinectManager.Instance;
        this.brainMgr = BrainDataManager.Instance;

        this.userIdx = userIndex;
        this.poseID = PoseID;

        pose = brainMgr.GetPoseInfo(poseID);

        if (agentParameters == null)
            agentParameters = new AgentParameters();
    }

    void OnDisable()
    {
        isInCooldown = false;
    }

    void Update()
    { 
        if (!PoseMgr.Instance.IsInNewPoseCooldown && !isInCooldown)
        {
            isPoseMatched = poseConfidence >= pose.min_confidence;
            if (isPoseMatched)
            {
                object[] param = { userIdx, poseID, poseConfidence };
                EventMsgDispatcher.Instance.TriggerEvent(EventDef.User_Pose_Matched, param);

                PoseMgr.Instance.GenerateNewPose();

                isPoseMatched = false;
                isInCooldown = true;
            }
        }
    }

    public override void InitializeAgent()
    {
    }

    public override void CollectObservations()
    {
        if (kinectMgr.IsUserInKinectView(userId))
        {
            for (int i = 0; i < pose.num_joint_detections; i++)
            {
                int JointIdx = pose.joint_ids[i];
                if (kinectMgr.IsJointTracked(userId, JointIdx))
                {
                    Quaternion JointOrientation = kinectMgr.GetJointOrientation(userId, JointIdx);
                    AddVectorObs(JointOrientation);
                }
            }
        }
        else
        {
            for (int i = 0; i < pose.num_joint_detections; i++)
            {
                AddVectorObs(Quaternion.identity);
            }
        }
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        poseConfidence = vectorAction[0];
        //Debug.Log(string.Format("User {0} : Agent {1}: action = {2}", this.name, poseID, poseConfidence));
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
