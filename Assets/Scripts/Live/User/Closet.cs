using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Closet : MonoBehaviour {

    public enum Side
    {
        Left = 0,
        Right,
    };

    public Side ClosetSide;

    private bool isActive = false;
    public bool IsActive
    {
        get
        {
            return isActive;
        }
    }

    private ClosetArrowItem topArrow, bottomArrow;
    private ClosetOutfitItem[] OutfitItems = new ClosetOutfitItem[ClosetManager.NUMBER_CLOSET_ITEMS];

	void Awake ()
    {
        for(int i=0; i<transform.childCount; i++)
        {
            if(i == 0)
            {
                topArrow = transform.GetChild(i).GetComponent<ClosetArrowItem>();
            }
            else if(i == transform.childCount - 1)
            {
                bottomArrow = transform.GetChild(i).GetComponent<ClosetArrowItem>();
            }
            else
            {
                OutfitItems[i-1] = transform.GetChild(i).GetComponent<ClosetOutfitItem>();
            }
        }
    }

    public void SetCloset(List<Outfit> outfits)
    {
        for(int i=0; i<OutfitItems.Length; i++)
        {
            OutfitItems[i].SetOutfit(outfits[i]);
        }

        topArrow.ShowItem();
        bottomArrow.ShowItem();
        isActive = true;
    }

    public void ClearCloset()
    {
        for (int i = 0; i < OutfitItems.Length; i++)
        {
            OutfitItems[i].ClearOutfit();
        }
        topArrow.HideItem();
        bottomArrow.HideItem();
        isActive = false;
    }

    public void OnTopArrowHover()
    {
        topArrow.OnItemHover();
        bottomArrow.OnItemUnselected();
        foreach(ClosetOutfitItem outfitItem in OutfitItems)
        {
            outfitItem.OnItemUnselected();
        }
    }

    public void OnBottomArrowHover()
    {
        topArrow.OnItemUnselected();
        bottomArrow.OnItemHover();
        foreach (ClosetOutfitItem outfitItem in OutfitItems)
        {
            outfitItem.OnItemUnselected();
        }
    }

    public void OnOutfitItemHover(int idx)
    {
        topArrow.OnItemUnselected();
        bottomArrow.OnItemUnselected();
        for(int i=0; i< ClosetManager.NUMBER_CLOSET_ITEMS; i++)
        {
            if(i == idx)
            {
                OutfitItems[i].OnItemHover();
            }
            else
            {
                OutfitItems[i].OnItemUnselected();
            }
        }
    }

    public void OnUnselectAll()
    {
        topArrow.OnItemUnselected();
        bottomArrow.OnItemUnselected();
        foreach (ClosetOutfitItem outfitItem in OutfitItems)
        {
            outfitItem.OnItemUnselected();
        }
    }

}
