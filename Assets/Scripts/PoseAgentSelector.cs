using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseAgentSelector : MonoBehaviour
{
    long userID;

    TargetPoseRecognizingAgent[] poseAgents;

    public void Init(long kinectUserID)
    {
        userID = kinectUserID;
        generatePoseAgents(kinectUserID);
        PoseFX fx = GetComponent<PoseFX>();
        if (fx)
            fx.Init(kinectUserID);
    }

    void generatePoseAgents(long userID)
    {
        poseAgents = new TargetPoseRecognizingAgent[BrainDataManager.Instance.NumPoses];
        for (int i = 0; i < BrainDataManager.Instance.NumPoses; i++)
        {
            TargetPoseRecognizingAgent tprAgent = gameObject.AddComponent<TargetPoseRecognizingAgent>();
            tprAgent.Init(userID, i);
            tprAgent.GiveBrain(BrainDataManager.Instance.GetBrain(i));
            tprAgent.enabled = false;
            poseAgents[i] = tprAgent;
        }
    }

    void OnEnable()
    {
        EventMsgDispatcher.Instance.registerEvent(EventDef.New_Pose_Generated, OnNewPoseGenerated);
    }

    void OnDisable()
    {
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.New_Pose_Generated, OnNewPoseGenerated);
    }

    public void OnNewPoseGenerated(object [] param)
    {
        if (!isUserTracked())
        {
            activatePoseAgent(-1);
            return;
        }

        int poseID = (int)param[0];
        activatePoseAgent(poseID);
    }

    bool isUserTracked()
    {
        return userID > 0L && KinectManager.Instance.IsUserTracked(userID) && KinectManager.Instance.IsUserInKinectView(userID);
    }

    void activatePoseAgent(int poseID)
    {
        if (poseAgents == null)
            return;

        for (int i = 0; i < poseAgents.Length; i++)
        {
            if (poseID > 0 && i == poseID - 1 /*&& poseAgents[i].agentParameters != null*/)
                poseAgents[i].enabled = true;
            else
                poseAgents[i].enabled = false;
        }
    }
}
