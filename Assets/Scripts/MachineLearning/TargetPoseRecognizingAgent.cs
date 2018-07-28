using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPoseRecognizingAgent : Agent {

    private KinectManager manager;
    private long KinectUserId;

    private int poseID;

    public bool isPoseMatched = false;
    public int PoseMatchCount;
    private float estimationTimeEllapsed;
    public float poseScore;

    public void Init(long userID, int PoseID)
    {
        manager = KinectManager.Instance;
        KinectUserId = userID;
        poseID = PoseID;
        if (agentParameters == null)
            agentParameters = new AgentParameters();
    }

    void Update()
    {
        estimationTimeEllapsed += Time.deltaTime;

        EstimatePose();
        if (isPoseMatched)
        {
            EventMsgDispatcher.Instance.TriggerEvent(EventDef.User_Pose_Detected, poseID);
            isPoseMatched = false;
        }
    }

    public override void InitializeAgent()
    {
    }

    public override void CollectObservations()
    {
        if (KinectUserId == 0)
            KinectUserId = manager.GetPrimaryUserID(); //Delete later
        if (manager.IsUserInKinectView(KinectUserId))
        {
            PoseParameter pose = BrainDataManager.Instance.GetPoseInfo(poseID);
            for (int i = 0; i < pose.num_joint_detections; i++)
            {
                int JointIdx = pose.joint_ids[i];
                if (manager.IsJointTracked(KinectUserId, JointIdx))
                {
                    Vector3 JointOrientation = manager.GetJointOrientation(KinectUserId, JointIdx).eulerAngles;
                    JointOrientation = NormalizeAngles(JointOrientation);
                    AddVectorObs(JointOrientation);
                }
            }
        }
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        int isMatched = Mathf.RoundToInt(vectorAction[0]);
        if (isMatched >= 1)
            PoseMatchCount++;
        //Debug.Log(string.Format("Agent {0}: action = {1}, isMatched = {2}", poseID, vectorAction[0], isMatched));
    }

    public override void AgentOnDone()
    {
        //Destroy(gameObject);
    }

    public override void AgentReset()
    {
        isPoseMatched = false;
        PoseMatchCount = 0;
        estimationTimeEllapsed = 0f;
    }

    void EstimatePose()
    {
        if (estimationTimeEllapsed > SystemConfigs.PoseEstimationTimeFrame)
        {
            if (PoseMatchCount > 0)
            {
                poseScore = PoseMatchCount / estimationTimeEllapsed;
                isPoseMatched = poseScore >= SystemConfigs.MinPoseConfidence;
            }

            estimationTimeEllapsed = 0f;
            PoseMatchCount = 0;
        }
    }

    //Normalizing to [-1, 1]
    Vector3 NormalizeAngles(Vector3 angles)
    {
        return (angles / 180f) - Vector3.one;
    }
}
