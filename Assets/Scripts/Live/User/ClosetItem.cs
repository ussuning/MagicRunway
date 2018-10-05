using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClosetItem : MonoBehaviour {

    public float HoverToSelectTime = 1.25f;

    public Image ItemImage;
    public Image SelectedFillImage;

    public Animator Animator;
    public bool isAnimatorDebug = false;

    protected Closet closet;
    public Closet Closet
    {
        set
        {
            closet = value;
        }
    }
    public float TopBound
    {
        get
        {
            //return (ItemImage.rectTransform.position.y + (Screen.height/2160f * 200f)/2f)/Screen.height - 0.5f;
            return ItemImage.rectTransform.position.y/Screen.height + 0.0785f - 0.5f;
        }
    }

    public float BottomBound
    {
        get
        {
            //return (ItemImage.rectTransform.position.y - (Screen.height / 2160f * 200f) / 2f) / Screen.height - 0.5f;
            return ItemImage.rectTransform.position.y/Screen.height - 0.0785f - 0.5f;
        }
    }

    protected bool isSelected = false;
    protected bool isHover = false;
    private float hoverDuration = 0f;
    private float HoverToSelectTransitionSpeed;

    public float hoverProgress
    {
        get { return Mathf.Clamp01(hoverDuration / HoverToSelectTime); }
    } 

    void Start ()
    {
        Animator = GetComponentInChildren<Animator>();
        HoverToSelectTransitionSpeed = SelectedFillImage.rectTransform.sizeDelta.x / HoverToSelectTime;

        OnItemUnselected();
    }

    void Update ()
    {
        //Debug.Log(string.Format("{0}    Top: {1},  Bottom: {2}", name, TopBound, BottomBound));

        if(isHover && !isSelected)
        {
            //hoverDuration += Time.deltaTime; // Do this in FixedUpdate for smoothness.
            SelectedFillImage.fillAmount = hoverProgress;

            if (hoverProgress >= 1f)
            {
                OnItemSelected();
                //hoverDuration = 0f;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isHover && !isSelected)
        {
            hoverDuration += Time.fixedDeltaTime;
        }
    }

    public void ShowItem()
    {
        SelectedFillImage.enabled = true;
        ItemImage.enabled = true;
    }

    public void HideItem()
    {
        OnItemUnselected();
        SelectedFillImage.enabled = false;
        ItemImage.enabled = false;
    }

    public void OnItemHover()
    {
        if (isHover == false)
        {
            isHover = true;
            hoverDuration = 0f;
            if (Animator != null)
                Animator.SetTrigger("onHoverStart");
        }
    }

    public virtual void OnItemSelected()
    {
        SelectedFillImage.fillAmount = 1f;
        isSelected = true;
    }

    public void OnItemUnselected ()
    {
        if (isSelected || isHover)
        {
            SelectedFillImage.fillAmount = 0f;
            isSelected = false;
            isHover = false;
            if (isAnimatorDebug)
                Debug.Log("animator Debug");

            if (Animator != null)
                Animator.SetTrigger("onHoverEnd");
        }
    }
}
