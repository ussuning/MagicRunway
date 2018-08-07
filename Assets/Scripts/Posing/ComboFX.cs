using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboFX : MonoBehaviour {

    public float textLifeSpan = 2f;

    public Text ComboNumText;
    public Text ComboLabel;

    int debug_comboNum = 0;
    public void debugComboText()
    {
        debug_comboNum++;
        SetComboText(0L, debug_comboNum);
    }
     
    void Start ()
    {
        ClearComboText();
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
        CancelInvoke("ClearComboText");

        ComboNumText.text = comboNum.ToString();

        ComboNumText.enabled = true;
        ComboLabel.enabled = true;

        FlyToUser(userID);

        Invoke("ClearComboText", textLifeSpan);
    }

    void ClearComboText()
    {
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
