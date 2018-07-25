using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseRecognizingAgent : Agent {

    public float minConfidence = 0.5f;

    private KinectManager manager;
    private long KinectUserId;

    private Dictionary<int, int> PoseCountsWithinTimeFrame = new Dictionary<int, int>();
    //Change to private later
    public int estPoseIdx;
    public int curPoseIdx;
    public int prevPoseIdx;
  
    public int combo = 1;

    private float estimationTimeEllapsed = 0f;
    private float posingTimeEllapsed = 0f;
    private float prevPoseTime = 0f;

    private float estPoseConfidence;

    //void Start()
    //{
    //    if (manager == null)
    //        manager = KinectManager.Instance;
    //}

    public void Init(long userID)
    {
        manager = KinectManager.Instance;
        KinectUserId = userID;
    }

    void Update ()
    {
        estimationTimeEllapsed += Time.deltaTime;
        posingTimeEllapsed += Time.deltaTime;

        if(estimationTimeEllapsed > SystemConfigs.PoseEstimationTimeFrame)
        {
            if(PoseCountsWithinTimeFrame.Count > 0)
            {
                int maxCounts = 0;
                int probablePoseIdx = 0;
                int totalCounts = 0;
                foreach(int poseIdx in PoseCountsWithinTimeFrame.Keys)
                {
                    if(PoseCountsWithinTimeFrame[poseIdx] > maxCounts)
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

    void LateUpdate()
    {
        if(estPoseIdx != 0 && estPoseIdx != 10)
        {
            if (curPoseIdx != estPoseIdx)
            {
                if (posingTimeEllapsed > SystemConfigs.PosingTime)
                {
                    if (Time.time - prevPoseTime <= SystemConfigs.ComboPoseTime)
                    {
                        combo++;
                    }
                    else
                    {
                        combo = 1;
                    }

                    prevPoseIdx = curPoseIdx;
                    curPoseIdx = estPoseIdx;

                    prevPoseTime = Time.time;
                    posingTimeEllapsed = 0f;

                    EventMsgDispatcher.Instance.TriggerEvent(EventDef.User_Pose_Detected, combo, curPoseIdx);
                    Debug.Log(string.Format("Pose Strike: x{0} Combo, Pose {1} ({2})", combo, curPoseIdx, estPoseConfidence));
                }
                else
                {
                    Debug.Log(string.Format("Short: {0} posingTimeEllapsed = {1}", estPoseIdx, posingTimeEllapsed));
                }
            }
            else
            {
                Debug.Log(string.Format("Same Pose: curPose: {0}, estNewPose: {1}", curPoseIdx, estPoseIdx));
            }
        }
        else
        {
            prevPoseIdx = curPoseIdx;
            curPoseIdx = estPoseIdx;
            posingTimeEllapsed = 0f;
            combo = 0;
        }
    }

    public override void InitializeAgent()
    {
        manager = KinectManager.Instance;
        if(KinectUserId == 0)
            KinectUserId = manager.GetPrimaryUserID();
    }

    public override void CollectObservations()
    {
        if(KinectUserId == 0)
            KinectUserId = manager.GetPrimaryUserID(); //Delete later
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

    //Normalizing to [-1, 1]
    Vector3 NormalizeAngles(Vector3 angles)
    {
        return (angles / 180f) - Vector3.one;
    }
}
