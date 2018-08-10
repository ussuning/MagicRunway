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

    void Awake ()
    {
        TweenToControl = GetComponent<EasyTween>();
        text = GetComponent<Text>();
    }

    public void ActivateTextFX(float poseConfidence)
    {
        initScale = TweenToControl.rectTransform.localScale;
        enlargedScale = initScale * TargetScale;

        initPos = TweenToControl.rectTransform.anchoredPosition;
        targetPos = TweenToControl.rectTransform.anchoredPosition + 1000f * Vector2.down;

        PoseFeedback pf = FeedbackMgr.Instance.GetPoseFeedback(poseConfidence);
        string feedbackString = pf.feedback_text;
        Color feedbackColor = new Color(pf.feedback_color_r, pf.feedback_color_g, pf.feedback_color_b, 1f);
        SetText(feedbackString, feedbackColor);

        Destroy(this.gameObject, TweenToControl.GetAnimationDuration() + 0.5f);
    }

    public void SetText(string txt, Color txtColor)
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
}
