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

    Image lightRaysImage;
    Material lightRaysMat;

    public void StartFX()
    {
        curAlpha = 1f;
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
        lightRaysMat.SetColor("_Color1", new Color(LightRaysColor1.r, LightRaysColor1.g, LightRaysColor1.b, curAlpha));
        lightRaysMat.SetColor("_Color2", new Color(LightRaysColor2.r, LightRaysColor2.g, LightRaysColor2.b, curAlpha));
    }
}
