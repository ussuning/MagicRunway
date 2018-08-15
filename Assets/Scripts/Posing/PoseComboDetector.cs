using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PoseComboDetector : MonoBehaviour {

    private int combo = 0;
    public int ComboNum
    {
        get
        {
            if (combo > PoseMgr.Instance.ComboCount - 1)
                return PoseMgr.Instance.ComboCount - 1;
            return combo;
        }
    }

    private float poseTimeEllapsed = 0f;
    private long lastComboOwner = 0L;

    void OnEnable()
    {
        EventMsgDispatcher.Instance.registerEvent(EventDef.User_Pose_Detected, OnUserPoseMatched);
    }

    void OnDisable()
    {
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.User_Pose_Detected, OnUserPoseMatched);
    }

    public void OnUserPoseMatched(object param, object paramEx)
    {
        lastComboOwner = (long)param;

        if (poseTimeEllapsed <= PoseMgr.Instance.GetComboInfo(ComboNum).combo_time)
        {
            combo++;
            if (combo > 1)
                EventMsgDispatcher.Instance.TriggerEvent(EventDef.User_Combo_Detected, lastComboOwner, combo);
            if (combo > 4)
                EventMsgDispatcher.Instance.TriggerEvent(EventDef.High_Combo_Detected, combo);
        }
        else
        {
            combo = 1;
        }
        GenerateNewPose();

        CancelInvoke("ClearCombo");
        Invoke("ClearCombo", PoseMgr.Instance.GetComboInfo(ComboNum).combo_time);
    }

    void Start()
    {
        combo = 0;
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
        EventMsgDispatcher.Instance.TriggerEvent(EventDef.Combo_Broken_Detected, lastComboOwner, combo);
        combo = 0;
    }
}
