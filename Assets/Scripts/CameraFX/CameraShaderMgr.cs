using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CameraShaderController
{
    public CameraShader ShaderScript;
    public float fadeInSpeed;
    public float fadeOutSpeed;

    //Normalized intensity [0, 1]
    private float curIntensity;
    private float targetIntensity;
    public float TargetIntensity
    {
        set
        {
            targetIntensity = value;
        }
    }

    public void ZeroIntensity()
    {
        ShaderScript.Intensity = curIntensity = targetIntensity = 0f;
        //ShaderScript.enabled = false;
    }

    public void UpdateIntensity()
    {
        if (curIntensity < targetIntensity)
        {
            curIntensity += fadeInSpeed * Time.deltaTime;
            if (curIntensity > targetIntensity)
                curIntensity = targetIntensity;
            ShaderScript.Intensity = curIntensity * ShaderScript.MaxIntensity;
        }
        else if (curIntensity > targetIntensity)
        {
            curIntensity -= fadeOutSpeed * Time.deltaTime;
            if (curIntensity < targetIntensity)
                curIntensity = targetIntensity;
            ShaderScript.Intensity = curIntensity * ShaderScript.MaxIntensity;
        }

        //ShaderScript.enabled = !(curIntensity == ShaderScript.MinIntensity);
    }
}

public class CameraShaderMgr : MonoBehaviour {

    //public CameraShaderController[] shaderControllers;

    //void Start ()
    //{
    //    for(int i=0; i<shaderControllers.Length; i++)
    //    {
    //        shaderControllers[i].ZeroIntensity();
    //    }
    //}

    //void Update()
    //{
    //    for (int i = 0; i < shaderControllers.Length; i++)
    //    {
    //        shaderControllers[i].UpdateIntensity();
    //    }
    //}

    //void OnEnable()
    //{
    //    EventMsgDispatcher.Instance.registerEvent(EventDef.High_Combo_Detected, OnHighComboDetected);
    //    EventMsgDispatcher.Instance.registerEvent(EventDef.Combo_Broken_Detected, OnComboBroken);
    //}

    //void OnDisable()
    //{
    //    EventMsgDispatcher.Instance.unRegisterEvent(EventDef.High_Combo_Detected, OnHighComboDetected);
    //    EventMsgDispatcher.Instance.unRegisterEvent(EventDef.Combo_Broken_Detected, OnComboBroken);
    //}

    //public void OnHighComboDetected(object [] param)
    //{
    //    int comboNum = (int)param[0];

    //    for (int i = 0; i < shaderControllers.Length; i++)
    //    {
    //        float targetIntensity = FeedbackMgr.Instance.GetComboFeedback(comboNum).intensity;//(float)((comboNum - 4) / 3);
    //        if (targetIntensity > 1f)
    //            targetIntensity = 1f;
    //        shaderControllers[i].TargetIntensity = targetIntensity;
    //    }
    //}

    //public void OnComboBroken(object [] param)
    //{
    //    for (int i = 0; i < shaderControllers.Length; i++)
    //    {
    //        shaderControllers[i].TargetIntensity = 0f;
    //    }
    //}
}
