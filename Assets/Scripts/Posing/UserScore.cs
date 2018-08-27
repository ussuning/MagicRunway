using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserScore : MonoBehaviour {

    public Text UserName;
    public Text UserScoreText;
    public PoseFeedbackLightraysFX lightRaysFX;
    public GameObject ScoreTextPrefab;

    private float score;

    long userID;

    public void DebugScoreText()
    {
        GenerateEffects(0.9f);
    }

    public void Init(long user)
    {
        userID = user;
        UserName.text = string.Format("Player  {0}", KinectManager.Instance.GetUserIndexById(userID) + 1);
        score = 0;
    }

    public void AddScore(int points)
    {
        score += points;
        UserScoreText.text = score.ToString();
    }

    void OnEnable()
    {
        EventMsgDispatcher.Instance.registerEvent(EventDef.User_Pose_Detected, OnUserPoseMatched);
        EventMsgDispatcher.Instance.registerEvent(EventDef.Combo_Broken_Detected, OnComboScored);
    }

    void OnDisable()
    {
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.User_Pose_Detected, OnUserPoseMatched);
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.Combo_Broken_Detected, OnComboScored);
    }

    public void OnUserPoseMatched(object [] param)
    {
        long matchedUserID = (long)param[0];
        int poseID = (int)param[1];
        float poseConfidence = (float)param[2];
        if (matchedUserID == userID)
        {
            AddScore(ScoreMgr.Instance.SinglePoseScore);
            GenerateEffects(poseConfidence);
        }
    }

    public void OnComboScored(object [] param)
    {
        long lastComboOwner = (long)param[0];
        List<int> comboIDs = (List<int>)param[1];

        int combo = comboIDs.Count;
        if (lastComboOwner == userID)
        {
            AddScore(ScoreMgr.Instance.GetComboScore(combo));
        }
    }

    public void GenerateEffects(float poseConfidence)
    {
        lightRaysFX.StartFX(poseConfidence);
        GenerateScoreText(poseConfidence);
    }

    public void GenerateScoreText(float poseConfidence)
    {
        GameObject scoreTextGO = Instantiate(ScoreTextPrefab, this.transform);
        PoseFeedbackTextFX textFX = scoreTextGO.GetComponent<PoseFeedbackTextFX>();
        textFX.ActivateTextFX(poseConfidence);
    }
}
