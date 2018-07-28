using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPoseRecognizingAgent : Agent {

    public float minConfidence = 0.75f;

    private KinectManager manager;
    private long KinectUserId;

    private int poseID;

    private Dictionary<int, int> PoseCountsWithinTimeFrame = new Dictionary<int, int>();
    //Change to private later
    public int estPoseIdx;
    public int curPoseIdx;
    public int prevPoseIdx;

    private float estimationTimeEllapsed = 0f;
    private float posingTimeEllapsed = 0f;

    private float estPoseConfidence;

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
        posingTimeEllapsed += Time.deltaTime;

        EstimatePose();
        UpdatePose();
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
                if (manager.IsJointTracked(KinectUserId, pose.joint_ids[i]))
                {
                    int JointIdx = (int)SystemConfigs.DetectedJoints[i];
                    Vector3 JointOrientation = manager.GetJointOrientation(KinectUserId, JointIdx).eulerAngles;
                    JointOrientation = NormalizeAngles(JointOrientation);
                    AddVectorObs(JointOrientation);
                }
            }
        }
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        //Debug.Log(string.Format("Pose {0}: {1}", poseID, vectorAction[0]));
        int newPoseIdx = Mathf.RoundToInt(vectorAction[0]);
        if (newPoseIdx < 0)
            newPoseIdx = 0;
        if (PoseCountsWithinTimeFrame.ContainsKey(newPoseIdx))
            PoseCountsWithinTimeFrame[newPoseIdx]++;
        else
            PoseCountsWithinTimeFrame.Add(newPoseIdx, 1);
    }

    public override void AgentOnDone()
    {
        //Destroy(gameObject);
    }

    public override void AgentReset()
    {
        estPoseIdx = 0;
        prevPoseIdx = 0;
        posingTimeEllapsed = 0f;
    }

    void EstimatePose()
    {
        if (estimationTimeEllapsed > SystemConfigs.PoseEstimationTimeFrame)
        {
            if (PoseCountsWithinTimeFrame.Count > 0)
            {
                int maxCounts = 0;
                int probablePoseIdx = 0;
                int totalCounts = 0;
                foreach (int poseIdx in PoseCountsWithinTimeFrame.Keys)
                {
                    if (PoseCountsWithinTimeFrame[poseIdx] > maxCounts)
                    {
                        probablePoseIdx = poseIdx;
                        maxCounts = PoseCountsWithinTimeFrame[poseIdx];
                    }
                    totalCounts += PoseCountsWithinTimeFrame[poseIdx];
                }

                estPoseConfidence = (float)maxCounts / totalCounts;
                if (probablePoseIdx > 0 && estPoseConfidence > minConfidence)
                {
                    estPoseIdx = probablePoseIdx;
                }
                else
                {
                    estPoseIdx = 0;
                }
            }

            estimationTimeEllapsed = 0f;
            PoseCountsWithinTimeFrame.Clear();
        }
    }

    void UpdatePose()
    {
        if (estPoseIdx != 0 /*&& estPoseIdx != 10*/)
        {
            if (curPoseIdx != estPoseIdx && posingTimeEllapsed > SystemConfigs.PosingTime)
            {
                prevPoseIdx = curPoseIdx;
                curPoseIdx = estPoseIdx;

                posingTimeEllapsed = 0f;

                EventMsgDispatcher.Instance.TriggerEvent(EventDef.User_Pose_Detected, curPoseIdx, curPoseIdx);
            }
        }
        else
        {
            prevPoseIdx = curPoseIdx;
            curPoseIdx = estPoseIdx;
            posingTimeEllapsed = 0f;
        }
    }

    //Normalizing to [-1, 1]
    Vector3 NormalizeAngles(Vector3 angles)
    {
        return (angles / 180f) - Vector3.one;
    }
}
