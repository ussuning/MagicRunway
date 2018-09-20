using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClosetItem : MonoBehaviour {

    public float HoverToSelectTime = 1.25f;
    public Color UnselectedColor;
    public Color HoverColor;
    public Color SelectedColor;

    public Image ItemImage;

    protected Closet closet;
    public Closet Closet
    {
        set
        {
            closet = value;
        }
    }

    protected bool isSelected = false;
    protected bool isHover = false;
    private float hoverDuration = 0f;
    private Color HoverToSelectTransitionSpeed;

    void Start ()
    {
        HoverToSelectTransitionSpeed = (SelectedColor - HoverColor) / HoverToSelectTime; 
        OnItemUnselected();
    }

    void Update ()
    {
        if(isHover && !isSelected)
        {
            hoverDuration += Time.deltaTime;

            ItemImage.color += Time.deltaTime * HoverToSelectTransitionSpeed;

            if (hoverDuration >= HoverToSelectTime)
            {
                OnItemSelected();
                hoverDuration = 0f;
            }
        }
    }

    public void ShowItem()
    {
        ItemImage.enabled = true;
    }

    public void HideItem()
    {
        ItemImage.enabled = false;
    }

    public void OnItemHover()
    {
        if (!isHover)
            ItemImage.color = HoverColor;
        isHover = true;
    }

    public virtual void OnItemSelected()
    {
        ItemImage.color = SelectedColor;
        isSelected = true;
    }

    public void OnItemUnselected ()
    {
        ItemImage.color = UnselectedColor;
        isSelected = false;
        isHover = false;
        hoverDuration = 0f;
    }
}
