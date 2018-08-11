using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PoseFeedbackLightraysFX : MonoBehaviour {

    public float Lifespan = 1f;
    public Material LightRaysMaterial;
    public Color LightRaysColor1;
    public Color LightRaysColor2;

    private float curAlpha = 0f;

    private float curConfidence = 0f;

    Image lightRaysImage;
    Material lightRaysMat;

    public void StartFX(float confidence)
    {
        curAlpha = 1f;
        curConfidence = confidence;
        UpdateLightRaysAlpha();
    }

    void Awake()
    {
        lightRaysImage = GetComponent<Image>();
        lightRaysMat = Instantiate(LightRaysMaterial);
        lightRaysImage.material = lightRaysMat;
    }

    void Start()
    {
        curAlpha = 0f;
        UpdateLightRaysAlpha();
    }

    void Update ()
    {
        if(curAlpha > 0f)
        {
            curAlpha -= Time.deltaTime/Lifespan;
            if (curAlpha < 0f)
                curAlpha = 0f;
            UpdateLightRaysAlpha();
        }    
    }

    void UpdateLightRaysAlpha()
    {
        if (curAlpha > 0)
        {
            PoseFeedback pf = FeedbackMgr.Instance.GetPoseFeedback(curConfidence);
            Color feedbackColor = new Color(pf.feedback_color_r, pf.feedback_color_g, pf.feedback_color_b, 1f);
            lightRaysMat.SetColor("_Color1", new Color(LightRaysColor1.r, LightRaysColor1.g, LightRaysColor1.b, curAlpha));
            lightRaysMat.SetColor("_Color2", new Color(feedbackColor.r, feedbackColor.g, feedbackColor.b, curAlpha));
        }
        else
        {
            lightRaysMat.SetColor("_Color1", new Color(0f, 0f, 0f, curAlpha));
            lightRaysMat.SetColor("_Color2", new Color(0f, 0f, 0f, curAlpha));
        }
    }
}
