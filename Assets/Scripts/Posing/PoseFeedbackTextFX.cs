using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PoseFeedbackTextFX : MonoBehaviour {

    public float TargetScale = 1.5f;
    public AnimationCurve ScalingCurve;
    public AnimationCurve TranslatingCurve;

    private EasyTween TweenToControl;
    private Text text;

    Vector3 initScale;
    Vector3 enlargedScale;

    Vector2 initPos;
    Vector2 targetPos;

    string[] feedbackStrings = { "GOOD JOB", "AWESOME", "BEAUTIFUL", "FANTASTIC", "AMAZING", "PERFECT" };

    void Awake ()
    {
        TweenToControl = GetComponent<EasyTween>();
        text = GetComponent<Text>();
    }

    public void ActivateTextFX(float poseScore)
    {
        initScale = TweenToControl.rectTransform.localScale;
        enlargedScale = initScale * TargetScale;

        initPos = TweenToControl.rectTransform.anchoredPosition;
        targetPos = TweenToControl.rectTransform.anchoredPosition + 1000f * Vector2.down;

        string feedbackString = GetFeedbackString(poseScore);
        SetText(feedbackString);

        Destroy(this.gameObject, TweenToControl.GetAnimationDuration() + 0.5f);
    }

    public void SetText(string txt)
    {
        text.text = txt;
        ScaleText();
    }

    public void ScaleText()
    {
        if (!TweenToControl.IsObjectOpened())
        {
            TweenToControl.SetAnimationScale(TweenToControl.rectTransform.localScale, enlargedScale, ScalingCurve, ScalingCurve);
            TweenToControl.SetAnimationPosition(TweenToControl.rectTransform.anchoredPosition, targetPos, TranslatingCurve, TranslatingCurve);
        }
        else
        {
            TweenToControl.SetAnimationScale(TweenToControl.rectTransform.localScale, initScale, ScalingCurve, ScalingCurve);
            TweenToControl.SetAnimationPosition(TweenToControl.rectTransform.anchoredPosition, initPos, TranslatingCurve, TranslatingCurve);
        }
        TweenToControl.OpenCloseObjectAnimation();
    }

    string GetFeedbackString(float poseScore)
    {
        if (poseScore >= 60f)
            return feedbackStrings[5];
        else if(poseScore >= 50f)
            return feedbackStrings[4];
        else if (poseScore >= 40f)
            return feedbackStrings[3];
        else if (poseScore >= 30f)
            return feedbackStrings[2];
        else if (poseScore >= 20f)
            return feedbackStrings[1];
        else if (poseScore >= 10f)
            return feedbackStrings[0];
        return "";
    }
}
