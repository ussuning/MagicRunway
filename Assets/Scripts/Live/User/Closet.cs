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


    private ClosetArrowItem topArrow, bottomArrow;
    private ClosetOutfitItem[] OutfitItem = new ClosetOutfitItem[ClosetManager.NUMBER_CLOSET_ITEMS];

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
                OutfitItem[i-1] = transform.GetChild(i).GetComponent<ClosetOutfitItem>();
            }
        }
    }

    public void SetCloset(List<Outfit> outfits)
    {
        for(int i=0; i<OutfitItem.Length; i++)
        {
            OutfitItem[i].SetOutfit(outfits[i]);
        }

        topArrow.ShowItem();
        bottomArrow.ShowItem();
        isActive = true;
    }

    public void ClearCloset()
    {
        for (int i = 0; i < OutfitItem.Length; i++)
        {
            OutfitItem[i].ClearOutfit();
        }
        topArrow.HideItem();
        bottomArrow.HideItem();
        isActive = false;
    }

}
