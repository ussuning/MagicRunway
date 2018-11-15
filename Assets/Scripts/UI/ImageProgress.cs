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

    //public Image BackgroundImage;
    public Image ForegroundImage;

    public bool fadeIcons = true;
    public bool shiftIcons = true;
    public Side side = Side.Left;

    private float initBackgroundAlpha;
    private float initForegroundAlpha;
    private float initImagePos;
    private float shiftedImagePos;

    private float curAlpha;
    private float curShift;

    private RectTransform rectTrans;

    void Awake ()
    {
        rectTrans = GetComponent<RectTransform>();

        //initBackgroundAlpha = BackgroundImage.color.a;
        initForegroundAlpha = ForegroundImage.color.a;

        initImagePos = rectTrans.anchoredPosition.x;
        if (side == Side.Left)
            shiftedImagePos = initImagePos - rectTrans.sizeDelta.x;
        else if(side == Side.Right)
            shiftedImagePos = initImagePos + rectTrans.sizeDelta.x;
    }

    public void SetProgressValue(float v)
    {
        if ((fadeIcons && curAlpha == 1f) || (shiftIcons && curShift == 1f))
        {
            ForegroundImage.fillAmount = v;
        }
    }

    public void SetImageOut(float a)
    {
        if (fadeIcons)
            SetImageAlpha(a);
        if (shiftIcons)
            SetImageShift(a);
    }

    void SetImageAlpha(float a)
    {
        curAlpha = a;
        //BackgroundImage.color = new Color(BackgroundImage.color.r, BackgroundImage.color.g, BackgroundImage.color.b, a * initBackgroundAlpha);
        ForegroundImage.color = new Color(ForegroundImage.color.r, ForegroundImage.color.g, ForegroundImage.color.b, a * initForegroundAlpha);
    }
    
    void SetImageShift(float s)
    {
        curShift = s;
        rectTrans.anchoredPosition = new Vector2(s * (initImagePos - shiftedImagePos) + shiftedImagePos, rectTrans.anchoredPosition.y);
    }
}
