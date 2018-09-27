using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserScore : MonoBehaviour {

    public Text UserName;
    public ScoreStar[] Stars;

    private int userIdx;
    public int UserIndex
    {
        get
        {
            return userIdx;
        }
    }
    public long UserID
    {
        get
        {
            return KinectManager.Instance.GetUserIdByIndex(userIdx);
        }
    }

    private int numConsecutivePoseMatches = 0;
    private int score = 0;

    void OnEnable()
    {
        EventMsgDispatcher.Instance.registerEvent(EventDef.User_Pose_Matched, OnPoseMatched);
    }

    void OnDisable()
    {
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.User_Pose_Matched, OnPoseMatched);
    }

    public void OnPoseMatched(object[] param)
    {
        int matched_userIdx = (int)param[0];
        int poseID = (int)param[1];
        float pose_confidence = (float)param[2];

        if (matched_userIdx == userIdx)
        {
            numConsecutivePoseMatches++;
            if(numConsecutivePoseMatches > score)
            {
                SetStar(score, 1f);
                
                score++;
                numConsecutivePoseMatches = 0;
            }
            else
            {
                SetStar(score, (float)numConsecutivePoseMatches / (score + 1));
            }
        }
        else
        {
            SetStar(score, 0f);
            numConsecutivePoseMatches = 0;
        }
    }

    public void init(int userIdx)
    {
        this.userIdx = userIdx;
        this.score = 0;
        this.numConsecutivePoseMatches = 0;

        if (UserName)
            UserName.text = string.Format("P{0}", userIdx + 1);

        ResetStars();
    }

    private void SetStar(int starIdx, float amount)
    {
        if(starIdx < Stars.Length)
            Stars[starIdx].SetTargetFillAmount(amount);
    }

    private void ResetStars()
    {
        for (int i = 0; i < Stars.Length; i++)
        {
            Stars[i].ResetStar();
        }
    }
}
