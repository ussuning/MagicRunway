using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseRecognizingAgent : Agent {

    public UnityEngine.UI.Text DebugText;

    private KinectManager manager;
    private long KinectUserId;

    private int estPoseIdx;
    private int curPoseIdx;
    private int prevPoseIdx;

    private float estimationTimeEllapsed = 0f;
    private Dictionary<int, int> PoseCountsWithinTimeFrame = new Dictionary<int, int>();

    private float posingTimeEllapsed = 0f;

    float estPoseConfidence;

    void Start()
    {
        if (manager == null)
            manager = KinectManager.Instance;
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
                if (probablePoseIdx > 0 && estPoseConfidence > 0.25f)
                {
                    //prevPoseIdx = estPoseIdx;
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
        DebugText.text = string.Format("Cur Pose: {0}, confidence {1}, Prev Pose: {2}", estPoseIdx, estPoseConfidence, prevPoseIdx);
        if (estPoseIdx > 0 && /*estPoseIdx != prevPoseIdx &&*/ posingTimeEllapsed > SystemConfigs.PosingTime)
        {
            posingTimeEllapsed = 0f;

            int combo = 1;
            EventMsgDispatcher.Instance.TriggerEvent(EventDef.User_Pose_Detected, combo, estPoseIdx);

            prevPoseIdx = estPoseIdx;
        }
    }

    public override void InitializeAgent()
    {
        manager = KinectManager.Instance;
    }

    public override void CollectObservations()
    {
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

        //DebugText.text = string.Format("est Pose: {0}", newPoseIdx);
    }

    public override void AgentOnDone()
    {
        //Destroy(gameObject);
    }

    public override void AgentReset()
    {
        estPoseIdx = 0;
        posingTimeEllapsed = 0f;
    }

    //Normalizing to [-1, 1]
    Vector3 NormalizeAngles(Vector3 angles)
    {
        return (angles / 180f) - Vector3.one;
    }
}
