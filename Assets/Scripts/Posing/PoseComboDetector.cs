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

    public void OnUserPoseMatched(object [] param)
    {
        lastComboOwner = (long)param[0];
        lastMatchedPose = (int)param[2];

        if (poseTimeEllapsed <= PoseMgr.Instance.GetComboInfo(ComboNum).combo_time)
        {
            comboIDs.Add(lastMatchedPose);
            if (comboIDs.Count > 1)
            {
                object[] combo_param = { lastComboOwner, comboIDs.Count};
                EventMsgDispatcher.Instance.TriggerEvent(EventDef.User_Combo_Detected, combo_param);
            }
            if (comboIDs.Count > 4)
            {
                object[] highCombo_param = { comboIDs.Count };
                EventMsgDispatcher.Instance.TriggerEvent(EventDef.High_Combo_Detected, highCombo_param);
            }
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
        object[] param = { lastComboOwner, comboIDs };
        EventMsgDispatcher.Instance.TriggerEvent(EventDef.Combo_Broken_Detected, param);
        comboIDs.Clear();
    }
}
