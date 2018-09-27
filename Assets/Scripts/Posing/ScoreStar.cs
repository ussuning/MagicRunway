using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreStar : MonoBehaviour {

    public Image StarImage;

    public ParticleSystem [] StarPartciles;

    public float animateSpeed = 0.75f;

    private float targetFillAmount = 0f;
    private float curFillAmount = 0f;

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
            if (curFillAmount > targetFillAmount)
                curFillAmount = targetFillAmount;

            if (curFillAmount > 1f)
                curFillAmount = 1f;

            StarImage.fillAmount = curFillAmount;

            if (curFillAmount == 1f)
            {
                PlayParticles();
            }
        }
        else if(curFillAmount > targetFillAmount)
        {
            curFillAmount -= Time.deltaTime * animateSpeed;
            if (curFillAmount < targetFillAmount)
                curFillAmount = targetFillAmount;

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

    private void PlayParticles()
    {
        foreach (ParticleSystem p in StarPartciles)
        {
            p.Stop();
            p.Play();
        }
    }
}

