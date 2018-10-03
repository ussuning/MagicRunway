using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageProgress : MonoBehaviour {

    public Image BackgroundImage;
    public Image ForegroundImage;

    private float initBackgroundAlpha;
    private float initForegroundAlpha;

    private float curAlpha;

    void Start ()
    {
        initBackgroundAlpha = BackgroundImage.color.a;
        initForegroundAlpha = ForegroundImage.color.a;
    }
	
	public void SetProgressValue(float v)
    {
        if (curAlpha == 1f)
        {
            ForegroundImage.fillAmount = v;
        }
    }

    public void SetImageAlpha(float a)
    {
        curAlpha = a;
        BackgroundImage.color = new Color(BackgroundImage.color.r, BackgroundImage.color.g, BackgroundImage.color.b, a * initBackgroundAlpha);
        ForegroundImage.color = new Color(ForegroundImage.color.r, ForegroundImage.color.g, ForegroundImage.color.b, a * initForegroundAlpha);
    }
}
