using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Closet : MonoBehaviour {

    private bool isActive = false;
    public bool IsActive
    {
        get
        {
            return isActive;
        }
    }


    private Image topArrow, bottomArrow;
    private Image[] OutfitItem = new Image[ClosetManager.NUMBER_CLOSET_ITEMS];

	void Awake ()
    {
        for(int i=0; i<transform.childCount; i++)
        {
            if(i == 0)
            {
                topArrow = transform.GetChild(i).GetComponent<Image>();
            }
            else if(i == transform.childCount - 1)
            {
                bottomArrow = transform.GetChild(i).GetComponent<Image>();
            }
            else
            {
                OutfitItem[i-1] = transform.GetChild(i).GetComponent<Image>();
            }
        }
    }

    public void SetCloset(List<Outfit> outfits)
    {
        for(int i=0; i<OutfitItem.Length; i++)
        {
            OutfitItem[i].enabled = true;
            OutfitItem[i].sprite = GetOutfitThumb(outfits[i].icon);
        }
        topArrow.enabled = true;
        bottomArrow.enabled = true;
        isActive = true;
    }

    public void ClearCloset()
    {
        for (int i = 0; i < OutfitItem.Length; i++)
        {
            OutfitItem[i].sprite = null;
            OutfitItem[i].enabled = false;
        }
        topArrow.enabled = false;
        bottomArrow.enabled = false;
        isActive = false;
    }

    private Sprite GetOutfitThumb(string icon)
    {
        if (icon.Contains("."))
        {
            icon = icon.Substring(0, icon.IndexOf("."));
        }
        string iconPath = "Thumbs/" + icon;
        return Resources.Load<Sprite>(iconPath);
    }

}
