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
    public GameObject MaleIcon;
    public GameObject FemaleIcon;

    public bool fadeIcons = true;
    public bool shiftIcons = true;
    public Side side = Side.Left;

    public float flashTime = 2f;

    private float initBackgroundAlpha;
    private float initForegroundAlpha;
    private float initImagePos;
    private float shiftedImagePos;

    private float curAlpha;
    private float curShift;

    private float timeSinceFlash = 0f;
    private User.Gender shownGender = User.Gender.Female;

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

    void Update()
    {
        timeSinceFlash += Time.deltaTime;
        if (timeSinceFlash >= flashTime)
        {
            if (shownGender == User.Gender.Male)
                shownGender = User.Gender.Female;
            else if (shownGender == User.Gender.Female)
                shownGender = User.Gender.Male;

            ShowIcon(shownGender);
        }
    }

    public void SetProgressValue(float v, User.Gender gender = User.Gender.None)
    {
        if ((fadeIcons && curAlpha == 1f) || (shiftIcons && curShift == 1f))
        {
            ForegroundImage.fillAmount = v;
        }

        if(gender != User.Gender.None)
        {
            shownGender = gender;
            ShowIcon(gender);
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

        if(a == 0f)
            shownGender = User.Gender.Female;
    }
    
    void SetImageShift(float s)
    {
        curShift = s;
        rectTrans.anchoredPosition = new Vector2(s * (initImagePos - shiftedImagePos) + shiftedImagePos, rectTrans.anchoredPosition.y);

        if (s == 0)
            shownGender = User.Gender.Female;
    }

    void ShowIcon(User.Gender gender)
    {
        if (gender == User.Gender.Male)
        {
            if (MaleIcon)
                MaleIcon.SetActive(true);
            if (FemaleIcon)
                FemaleIcon.SetActive(false);
        }
        else if (gender == User.Gender.Female)
        {
            if (MaleIcon)
                MaleIcon.SetActive(false);
            if (FemaleIcon)
                FemaleIcon.SetActive(true);
        }
        timeSinceFlash = 0f;
    }
}
