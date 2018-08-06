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
        GenerateEffects();
    }

    public void Init(long user)
    {
        userID = user;
        UserName.text = user.ToString();
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
        float poseScore = (float)paramEx;
        if (matchedUserID == userID)
        {
            GenerateEffects(poseScore);
        }
    }

    public void GenerateEffects(float poseScore = 0f)
    {
        lightRaysFX.StartFX();
        GenerateScoreText(poseScore);
    }

    public void GenerateScoreText(float poseScore = 0f)
    {
        GameObject scoreTextGO = Instantiate(ScoreTextPrefab, this.transform);
        PoseFeedbackTextFX textFX = scoreTextGO.GetComponent<PoseFeedbackTextFX>();
        textFX.ActivateTextFX(poseScore);
    }
}
