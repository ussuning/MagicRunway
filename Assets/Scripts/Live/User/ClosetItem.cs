using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClosetItem : MonoBehaviour {

    public delegate void ClosetItemEventHandler(ClosetItem closetItem);
    public event ClosetItemEventHandler OnItemSelectedEvent;

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
    //public float TopBound
    //{
    //    get
    //    {
    //        //return (ItemImage.rectTransform.position.y + (Screen.height/2160f * 200f)/2f)/Screen.height - 0.5f;
    //        return ItemImage.rectTransform.position.y/Screen.height + 0.0785f - 0.5f;
    //    }
    //}

    //public float BottomBound
    //{
    //    get
    //    {
    //        //return (ItemImage.rectTransform.position.y - (Screen.height / 2160f * 200f) / 2f) / Screen.height - 0.5f;
    //        return ItemImage.rectTransform.position.y/Screen.height - 0.0785f - 0.5f;
    //    }
    //}

    protected bool isSelected = false;
    protected bool isHover = false;
    private float hoverDuration = 0f;

    public float hoverProgress
    {
        get { return Mathf.Clamp01(hoverDuration / HoverToSelectTime); }
    } 

    void Start ()
    {
        Animator = GetComponentInChildren<Animator>();

        OnItemUnselected();
    }

    void Update ()
    {
        //Debug.Log(string.Format("{0}    Top: {1},  Bottom: {2}", name, TopBound, BottomBound));

        if(isHover && !isSelected)
        {
            //hoverDuration += Time.deltaTime; // Do this in FixedUpdate for smoothness.
            if (SelectedFillImage != null)
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
        if (SelectedFillImage != null)
            SelectedFillImage.enabled = true;
        if (ItemImage != null)
            ItemImage.enabled = true;
    }

    public void HideItem()
    {
        OnItemUnselected();
        if (SelectedFillImage != null)
            SelectedFillImage.enabled = false;
        if (ItemImage != null)
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
        if (SelectedFillImage != null)
            SelectedFillImage.fillAmount = 1f;
        isSelected = true;

        if (OnItemSelectedEvent != null)
            OnItemSelectedEvent(this);
    }

    public void OnItemUnselected ()
    {
        if (isSelected || isHover)
        {
            if (SelectedFillImage != null)
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
