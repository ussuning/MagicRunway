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
    public Image SelectedFillImage;

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

    void Start ()
    {
        HoverToSelectTransitionSpeed = SelectedFillImage.rectTransform.sizeDelta.x / HoverToSelectTime;

        //SelectedFillImage.color = SelectedColor;

        OnItemUnselected();
    }

    void Update ()
    {
        //Debug.Log(string.Format("{0}    Top: {1},  Bottom: {2}", name, TopBound, BottomBound));

        if(isHover && !isSelected)
        {
            hoverDuration += Time.deltaTime;

            //if (closet.ClosetSide == Closet.Side.Left)
            //    SelectedFillImage.rectTransform.localPosition += Vector3.right * Time.deltaTime * HoverToSelectTransitionSpeed;
            //else if (closet.ClosetSide == Closet.Side.Right)
            //    SelectedFillImage.rectTransform.localPosition += -Vector3.right * Time.deltaTime * HoverToSelectTransitionSpeed;
            SelectedFillImage.fillAmount = hoverDuration / HoverToSelectTime;

            if (hoverDuration >= HoverToSelectTime)
            {
                OnItemSelected();
                hoverDuration = 0f;
            }
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
        //if (!isHover)
        //    ItemImage.color = HoverColor;
        isHover = true;
    }

    public virtual void OnItemSelected()
    {
        //SelectedFillImage.rectTransform.localPosition = Vector3.zero;
        SelectedFillImage.fillAmount = 1f;
        isSelected = true;
    }

    public void OnItemUnselected ()
    {
        ItemImage.color = UnselectedColor;
        //if (closet.ClosetSide == Closet.Side.Left)
        //    SelectedFillImage.rectTransform.localPosition = new Vector3(-SelectedFillImage.rectTransform.sizeDelta.x, 0f, 0f);
        //else if (closet.ClosetSide == Closet.Side.Right)
        //    SelectedFillImage.rectTransform.localPosition = new Vector3(SelectedFillImage.rectTransform.sizeDelta.x, 0f, 0f);
        SelectedFillImage.fillAmount = 0f;
        isSelected = false;
        isHover = false;
        hoverDuration = 0f;
    }
}
