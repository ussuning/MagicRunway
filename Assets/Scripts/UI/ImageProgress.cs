using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageProgress : MonoBehaviour {

    public enum Side
    {
        Left,
        Right,
    };

    public Image BackgroundImage;
    public Image ForegroundImage;

    public bool fadeImage = true;
    public bool shiftImage = true;
    public Side side = Side.Left;

    private float initBackgroundAlpha;
    private float initForegroundAlpha;
    private float initImagePos;
    private float shiftedImagePos;

    private float curAlpha;

    void Start ()
    {
        initBackgroundAlpha = BackgroundImage.color.a;
        initForegroundAlpha = ForegroundImage.color.a;

        initImagePos = BackgroundImage.rectTransform.anchoredPosition.x;
        if (side == Side.Left)
            shiftedImagePos = initImagePos - BackgroundImage.rectTransform.sizeDelta.x;
        else if(side == Side.Right)
            shiftedImagePos = initImagePos + BackgroundImage.rectTransform.sizeDelta.x;
    }
	
	public void SetProgressValue(float v)
    {
        if (curAlpha == 1f)
        {
            ForegroundImage.fillAmount = v;
        }
    }

    public void SetImageOut(float a)
    {
        if (fadeImage)
            SetImageAlpha(a);
        if (shiftImage)
            SetImageShift(a);
    }

    void SetImageAlpha(float a)
    {
        curAlpha = a;
        BackgroundImage.color = new Color(BackgroundImage.color.r, BackgroundImage.color.g, BackgroundImage.color.b, a * initBackgroundAlpha);
        ForegroundImage.color = new Color(ForegroundImage.color.r, ForegroundImage.color.g, ForegroundImage.color.b, a * initForegroundAlpha);
    }
    
    void SetImageShift(float s)
    {
        BackgroundImage.rectTransform.anchoredPosition = new Vector2(s * (initImagePos - shiftedImagePos) + shiftedImagePos, BackgroundImage.rectTransform.anchoredPosition.y);
    }
}
