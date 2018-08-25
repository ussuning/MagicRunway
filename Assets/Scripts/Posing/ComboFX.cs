using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboFX : MonoBehaviour {

    public Text ComboNumText;
    public Text ComboLabel;

    private long comboHoldingUser;
    private int lastComboNum;

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
        EventMsgDispatcher.Instance.registerEvent(EventDef.Combo_Broken_Detected, OnComboBroken);
    }

    void OnDisable()
    {
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.User_Combo_Detected, OnUserComboDetected);
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.Combo_Broken_Detected, OnComboBroken);
    }

    public void OnUserComboDetected(object param, object paramEx, object paramEx2)
    {
        long userID = (long)param;
        int comboNum = (int)paramEx;

        SetComboText(userID, comboNum);
    }

    public void OnComboBroken(object param, object paramEx, object paramEx2)
    {
        ClearComboText();
    }

    void SetComboText(long userID, int comboNum)
    {
        comboHoldingUser = userID;
        lastComboNum = comboNum;

        ComboNumText.text = comboNum.ToString();

        ComboNumText.enabled = true;
        ComboLabel.enabled = true;
    }

    void ClearComboText()
    {
        FlyToUser(comboHoldingUser);

        ComboNumText.enabled = false;
        ComboLabel.enabled = false;
    }

    void FlyToUser(long userID)
    {
        GameObject userComboNum = Instantiate(ComboNumText.gameObject, this.transform);
        FlyingText ft = userComboNum.AddComponent<FlyingText>();

        GameObject userScoreBoxGO = UserManager.Instance.getUserScoreBoxById(userID);
        if (userScoreBoxGO)
            ft.ActivateFlying(userScoreBoxGO.transform.position);
        else
            Destroy(userComboNum);
    }
}
