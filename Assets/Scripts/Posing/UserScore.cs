using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserScore : MonoBehaviour {

    public Text UserName;
    public GameObject ScoreTextPrefab;
    public PoseFeedbackLightraysFX lightRaysFX;

    long userID;

    public void DebugScoreText()
    {
        GenerateEffects(0.9f);
    }

    public void Init(long user)
    {
        userID = user;
        UserName.text = string.Format("Player  {0}", KinectManager.Instance.GetUserIndexById(userID) + 1);
    }

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
        long matchedUserID = (long)param;
        float poseConfidence = (float)paramEx;
        if (matchedUserID == userID)
        {
            GenerateEffects(poseConfidence);
        }
    }

    public void GenerateEffects(float poseConfidence)
    {
        lightRaysFX.StartFX();
        GenerateScoreText(poseConfidence);
    }

    public void GenerateScoreText(float poseConfidence)
    {
        GameObject scoreTextGO = Instantiate(ScoreTextPrefab, this.transform);
        PoseFeedbackTextFX textFX = scoreTextGO.GetComponent<PoseFeedbackTextFX>();
        textFX.ActivateTextFX(poseConfidence);
    }
}
