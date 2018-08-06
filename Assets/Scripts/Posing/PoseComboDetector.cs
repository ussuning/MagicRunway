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
        long userID = (long)param;

        if (poseTimeEllapsed <= PoseMgr.Instance.GetComboInfo(ComboNum).pose_time)
        {
            combo++;
            if (combo > 1)
                EventMsgDispatcher.Instance.TriggerEvent(EventDef.User_Combo_Detected, userID, combo);
            PoseMgr.Instance.GenerateNewPose();
            poseTimeEllapsed = 0f;
        }
    }

    void Start()
    {
        combo = 0;
        poseTimeEllapsed = 0f;

        PoseMgr.Instance.GenerateNewPose();
        poseTimeEllapsed = 0f;
    }

    void Update()
    {
        poseTimeEllapsed += Time.deltaTime;

        if (poseTimeEllapsed > PoseMgr.Instance.GetComboInfo(ComboNum).pose_time)
        {
            combo = 0;
            PoseMgr.Instance.GenerateNewPose();
            poseTimeEllapsed = 0f;
        }
    }
}
