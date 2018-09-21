using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserScore : MonoBehaviour {

    public Text UserName;
    public GameObject[] Stars;

    private long userID;
    public long UserID
    {
        get
        {
            return userID;
        }
    }

    private int score;

    public void init(long userID)
    {
        this.userID = userID;
        this.score = 0;

        if (UserName)
            UserName.text = string.Format("P{0}", KinectManager.Instance.GetUserIndexById(userID)+1);

        UpdateStars();
    }

    public void AddScore(int s)
    {
        score += s;
        UpdateStars();
    }

    void UpdateStars()
    {
        for (int i = 0; i < Stars.Length; i++)
        {
            Stars[i].SetActive(i < score);
        }
    }
}
