using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboFX : MonoBehaviour {

    public float textLifeSpan = 2f;

    public Text ComboNumText;
    public Text ComboLabel;

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

        SetComboText(comboNum);
    }

    void SetComboText(int comboNum)
    {
        CancelInvoke("ClearComboText");

        ComboNumText.text = comboNum.ToString();

        ComboNumText.enabled = true;
        ComboLabel.enabled = true;

        Invoke("ClearComboText", textLifeSpan);
    }

    void ClearComboText()
    {
        ComboNumText.enabled = false;
        ComboLabel.enabled = false;
    }
}
