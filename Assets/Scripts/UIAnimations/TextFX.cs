using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextFX : MonoBehaviour {

    public Text txt;
    public float ShakeSpeed = 1f;
    public float targetShrinkSize = 0.8f;

    private Transform trans;

    bool isShakingText = false;
    float shakingTimeEllapsed = 0f;
    float curScale;

    void Awake ()
    {
        trans = txt.transform;
    }

    void Start ()
    {
        curScale = trans.localScale.x;
    }

    void OnEnable ()
    {
        isShakingText = true;
        shakingTimeEllapsed = 0f;
    }

    void Update ()
    {
        if(isShakingText)
        {
            shakingTimeEllapsed += Time.deltaTime;
            if(shakingTimeEllapsed < 0.05f)
            {
                curScale = Mathf.Lerp(curScale, targetShrinkSize, ShakeSpeed * Time.deltaTime);
            }
            else
            {
                curScale = Mathf.Lerp(curScale, 1f, ShakeSpeed * Time.deltaTime);

                if (Mathf.Abs(curScale - trans.localScale.x) <= 0.000001f)
                {
                    curScale = 1f;
                    isShakingText = false;
                }
            }   
            trans.localScale = new Vector3(curScale, curScale, curScale);
        }
        else
        {
            shakingTimeEllapsed = 0f;
        }
    }
	
}
