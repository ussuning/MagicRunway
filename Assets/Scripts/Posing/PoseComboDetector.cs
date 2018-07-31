using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PoseComboDetector : MonoBehaviour {

    public Text ComboText;

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
            UpdateComboText(userID);
            PoseMgr.Instance.GenerateNewPose();
            poseTimeEllapsed = 0f;
        }
    }

    void Start()
    {
        combo = 0;
        poseTimeEllapsed = 0f;
        ClearComboText();
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

    void UpdateComboText(long user)
    {
        CancelInvoke("ClearComboText");

        if (ComboText)
            ComboText.text = string.Format("{0} x COMBO by {1}", combo, user);

        Invoke("ClearComboText", 2f);
    }

    void ClearComboText()
    {
        if (ComboText)
            ComboText.text = "";
    }
}
