using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreStar : MonoBehaviour {

    public Image StarImage;

    public float animateSpeed = 0.75f;

    private float targetFillAmount = 0f;
    private float curFillAmount = 0f;

    public RectTransform rectTransform
    {
        get
        {
            return StarImage.rectTransform;
        }
    }

    void Awake()
    {
        if(!StarImage)
            StarImage = GetComponent<Image>();
    }

    void Start ()
    {
        ResetStar();
    }
	
	void Update ()
    {
        if (curFillAmount < targetFillAmount)
        {
            curFillAmount += Time.deltaTime * animateSpeed;
            if (curFillAmount > 1f)
                curFillAmount = 1f;

            StarImage.fillAmount = curFillAmount;
        }
        else if(curFillAmount > targetFillAmount)
        {
            curFillAmount -= Time.deltaTime * animateSpeed;
            if (curFillAmount < 0f)
                curFillAmount = 0f;

            StarImage.fillAmount = curFillAmount;
        }
	}

    public void SetTargetFillAmount(float amt)
    {
        targetFillAmount = amt;
    }

    public void ResetStar()
    {
        StarImage.fillAmount = curFillAmount = targetFillAmount = 0f;
    }
}

