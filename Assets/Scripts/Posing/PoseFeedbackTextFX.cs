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

    void OnEnable()
    {
        initScale = TweenToControl.rectTransform.localScale;
        enlargedScale = initScale * TargetScale;

        initPos = TweenToControl.rectTransform.anchoredPosition;
        targetPos = TweenToControl.rectTransform.anchoredPosition + 1000f * Vector2.down;

        string[] feedbackStrings = { "FANTASTIC", "BEAUTIFUL", "AWESOME", "GOOD JOB" };
        SetText(feedbackStrings[Random.Range(0, feedbackStrings.Length)]);
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
}
