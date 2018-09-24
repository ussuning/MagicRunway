using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseAgentSelector : MonoBehaviour
{ 
    int userIdx;
    long userID
    {
        get
        {
            return KinectManager.Instance.GetUserIdByIndex(userIdx);
        }
    }

    TargetPoseRecognizingAgent[] poseAgents;

    public void Init(int userIndex)
    {
        this.userIdx = userIndex;
        generatePoseAgents(userIdx);
    }

    void generatePoseAgents(int userIndex)
    {
        poseAgents = new TargetPoseRecognizingAgent[BrainDataManager.Instance.NumPoses];
        for (int i = 0; i < BrainDataManager.Instance.NumPoses; i++)
        {
            TargetPoseRecognizingAgent tprAgent = gameObject.AddComponent<TargetPoseRecognizingAgent>();
            tprAgent.Init(userIndex, i);
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
        if (!KinectManager.Instance.IsUserTracked(userID))
        {
            activatePoseAgent(-1);
            return;
        }

        int poseID = (int)param[0];
        activatePoseAgent(poseID);
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
