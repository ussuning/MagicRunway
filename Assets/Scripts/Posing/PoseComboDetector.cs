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
    }

    void OnDisable()
    {
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.User_Pose_Detected, OnUserPoseMatched);
    }

    public void OnUserPoseMatched(object param, object paramEx, object paramEx2)
    {
        lastComboOwner = (long)param;
        lastMatchedPose = (int)paramEx2;

        if (poseTimeEllapsed <= PoseMgr.Instance.GetComboInfo(ComboNum).combo_time)
        {
            comboIDs.Add(lastMatchedPose);
            if (comboIDs.Count > 1)
                EventMsgDispatcher.Instance.TriggerEvent(EventDef.User_Combo_Detected, lastComboOwner, comboIDs.Count);
            if (comboIDs.Count > 4)
                EventMsgDispatcher.Instance.TriggerEvent(EventDef.High_Combo_Detected, comboIDs.Count);
        }
        else
        {
            comboIDs.Clear();
            comboIDs.Add(lastMatchedPose);
        }
        GenerateNewPose();

        CancelInvoke("ClearCombo");
        Invoke("ClearCombo", PoseMgr.Instance.GetComboInfo(ComboNum).combo_time);
    }

    void Start()
    {
        comboIDs.Clear();
        poseTimeEllapsed = 0f;

        GenerateNewPose();
    }

    void Update()
    {
        poseTimeEllapsed += Time.deltaTime;
        if (poseTimeEllapsed > PoseMgr.Instance.GetComboInfo(ComboNum).pose_time)
        {
            GenerateNewPose();
        }
    }

    void GenerateNewPose()
    {
        PoseMgr.Instance.GenerateNewPose();
        poseTimeEllapsed = 0f;
    }

    void ClearCombo()
    {
        EventMsgDispatcher.Instance.TriggerEvent(EventDef.Combo_Broken_Detected, lastComboOwner, comboIDs);
        comboIDs.Clear();
    }
}
