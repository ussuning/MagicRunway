using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboFX : MonoBehaviour {

    public bool steelingMode = true;

    public Text ComboNumText;
    public Text ComboLabel;

    private long comboHoldingUser;

    int debug_comboNum = 0;
    public void debugComboText()
    {
        debug_comboNum++;
        SetComboText(0L, debug_comboNum);
    }
     
    void Start ()
    {
        comboHoldingUser = 0L;
        ComboNumText.enabled = false;
        ComboLabel.enabled = false;
    }

    void OnEnable()
    {
        EventMsgDispatcher.Instance.registerEvent(EventDef.User_Combo_Detected, OnUserComboDetected);
    }

    void OnDisable()
    {
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.User_Combo_Detected, OnUserComboDetected);
    }

    public void OnUserComboDetected(object param, object paramEx)
    {
        long userID = (long)param;
        int comboNum = (int)paramEx;

        SetComboText(userID, comboNum);
    }

    void SetComboText(long userID, int comboNum)
    {
        comboHoldingUser = userID;

        CancelInvoke("ClearComboText");

        ComboNumText.text = comboNum.ToString();

        ComboNumText.enabled = true;
        ComboLabel.enabled = true;

        if (!steelingMode)
            FlyToUser(comboHoldingUser);

        float comboTime = PoseMgr.Instance.GetComboInfo(comboNum).combo_time;
        Invoke("ClearComboText", comboTime);
    }

    void ClearComboText()
    {
        if(steelingMode)
            FlyToUser(comboHoldingUser);

        ComboNumText.enabled = false;
        ComboLabel.enabled = false;
    }

    void FlyToUser(long userID)
    {
        GameObject userComboNum = Instantiate(ComboNumText.gameObject, this.transform);
        FlyingText ft = userComboNum.AddComponent<FlyingText>();

        GameObject userScoreBoxGO = UserManager.Instance.getUserScoreBoxById(userID);
        ft.ActivateFlying(userScoreBoxGO.transform.position);
    }
}
