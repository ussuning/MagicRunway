using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShaderMgr : MonoBehaviour {

    public float fadeInSpeed = 0.5f;
    public float fadeOutSpeed = 1f;
    public CameraFilterPack_Glow_Glow_Color GlowColor;

    float curGlowIntensity = 0f;

    void Start ()
    {
        GlowColor.Intensity = curGlowIntensity = 0f;
    }

    void Update ()
    {
        //GlowColor.Intensity = Mathf.Lerp(GlowColor.Intensity, curGlowIntensity, transitionSpeed);
        if (GlowColor.Intensity < curGlowIntensity)
        {
            GlowColor.Intensity += fadeInSpeed * Time.deltaTime;
            if (GlowColor.Intensity > curGlowIntensity)
                GlowColor.Intensity = curGlowIntensity;
        }
        else if(GlowColor.Intensity > curGlowIntensity)
        {
            GlowColor.Intensity -= fadeOutSpeed * Time.deltaTime;
            if (GlowColor.Intensity < curGlowIntensity)
                GlowColor.Intensity = curGlowIntensity;
        }
    }

    void OnEnable()
    {
        EventMsgDispatcher.Instance.registerEvent(EventDef.High_Combo_Detected, OnHighComboDetected);
        EventMsgDispatcher.Instance.registerEvent(EventDef.Combo_Broken_Detected, OnComboBroken);
    }

    void OnDisable()
    {
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.High_Combo_Detected, OnHighComboDetected);
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.Combo_Broken_Detected, OnComboBroken);
    }

    public void OnHighComboDetected(object param, object paramEx)
    {
        int comboNum = (int)param;

        curGlowIntensity = (float)(comboNum - 4);
        if (curGlowIntensity > 3f)
            curGlowIntensity = 3f;
        Debug.Log(string.Format("[CameraShaderMgr] OnHighComboDetected: comboNum {0}  glowIntensity {1}", comboNum, curGlowIntensity));
    }

    public void OnComboBroken(object param, object paramEx)
    {
        curGlowIntensity = 0f;
    }
}
