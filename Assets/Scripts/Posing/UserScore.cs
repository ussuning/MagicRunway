using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserScore : MonoBehaviour {

    public Text UserName;
    public Image[] Stars;

    private long userID;
    public long UserID
    {
        get
        {
            return userID;
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
        long matched_userID = (long)param[0];
        int poseID = (int)param[1];
        float pose_confidence = (float)param[2];

        if (matched_userID == userID)
        {
            numConsecutivePoseMatches++;
            if(numConsecutivePoseMatches > score)
            {
                score++;
                numConsecutivePoseMatches = 0;
            }
        }
        else
        {
            numConsecutivePoseMatches = 0;
        }
        UpdateStars();
    }

    public void init(long userID)
    {
        this.userID = userID;
        this.score = 0;
        this.numConsecutivePoseMatches = 0;

        if (UserName)
            UserName.text = string.Format("P{0}", KinectManager.Instance.GetUserIndexById(userID)+1);

        UpdateStars();
    }

    void UpdateStars()
    {
        for (int i = 0; i < Stars.Length; i++)
        {
            Stars[i].fillAmount = (i < score) ? 1f : 0f;
        }

        Stars[score].fillAmount = ((float)numConsecutivePoseMatches / (score + 1));
    }
}
