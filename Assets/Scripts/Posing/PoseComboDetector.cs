using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PoseComboDetector : MonoBehaviour {

    private List<int> comboIDs = new List<int>();
    public int ComboNum
    {
        get
        {
            return comboIDs.Count;
        }
    }

    private float poseTimeEllapsed = 0f;
    private long lastComboOwner = 0L;
    private int lastMatchedPose = 0;

    void OnEnable()
    {
        EventMsgDispatcher.Instance.registerEvent(EventDef.User_Pose_Detected, OnUserPoseMatched);
        EventMsgDispatcher.Instance.registerEvent(EventDef.Combo_Replay_End, OnComboReplayEnded);
    }

    void OnDisable()
    {
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.User_Pose_Detected, OnUserPoseMatched);
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.Combo_Replay_End, OnComboReplayEnded);
    }

    public void OnUserPoseMatched(object [] param)
    {
        lastComboOwner = (long)param[0];
        lastMatchedPose = (int)param[1];

        bool generatePoseImmediately = true;
        if (poseTimeEllapsed <= PoseMgr.Instance.GetComboInfo(ComboNum).combo_time)
        {
            comboIDs.Add(lastMatchedPose);
            if (comboIDs.Count > 1)
            {
                object[] combo_param = { lastComboOwner, comboIDs.Count};
                EventMsgDispatcher.Instance.TriggerEvent(EventDef.User_Combo_Detected, combo_param);
            }
            if (comboIDs.Count >= 4)
            {
                object[] highCombo_param = { comboIDs.Count };
                EventMsgDispatcher.Instance.TriggerEvent(EventDef.High_Combo_Detected, highCombo_param);
            }
            if(comboIDs.Count >= 5)
            {
                generatePoseImmediately = false;
                CancelInvoke("ReplayCombo");
                Invoke("ReplayCombo", PoseMgr.Instance.GetComboInfo(ComboNum).combo_time);
            }
        }
        else
        {
            comboIDs.Clear();
            comboIDs.Add(lastMatchedPose);
        }
        if(generatePoseImmediately)
            GenerateNewPose();

        CancelInvoke("ClearCombo");
        Invoke("ClearCombo", PoseMgr.Instance.GetComboInfo(ComboNum).combo_time);
    }

    public void OnComboReplayEnded(object[] param)
    {
        GenerateNewPose();
    }

    void Start()
    {
        comboIDs.Clear();
        poseTimeEllapsed = 0f;

        GenerateNewPose();
    }

    void Update()
    {
        if (!ReplayStickmanController.Instance.IsReplaying)
        {
            poseTimeEllapsed += Time.deltaTime;
            if (poseTimeEllapsed > PoseMgr.Instance.GetComboInfo(ComboNum).pose_time)
            {
                GenerateNewPose();
            }
        }
    }

    void GenerateNewPose()
    {
        PoseMgr.Instance.GenerateNewPose();
        poseTimeEllapsed = 0f;
    }

    void ReplayCombo()
    {
        object[] param = { lastComboOwner, comboIDs };
        EventMsgDispatcher.Instance.TriggerEvent(EventDef.Combo_Replay_Start, param);
    }

    void ClearCombo()
    {
        object[] param = { lastComboOwner, comboIDs };
        EventMsgDispatcher.Instance.TriggerEvent(EventDef.Combo_Broken_Detected, param);
        comboIDs.Clear();
    }
}
