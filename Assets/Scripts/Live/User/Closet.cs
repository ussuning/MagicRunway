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

    private long ownerId;
    public long OwnerID
    {
        get
        {
            return ownerId;
        }
    }

    private User.Gender ownerGender;
    public User.Gender OwnerGender
    {
        get
        {
            return ownerGender;
        }
    }

    private List<Outfit> outfits;
    public List<Outfit> Outfits
    {
        get
        {
            return outfits;
        }
    }

    private int outfitPageIdx = 0;
    public int OutfitPageIndex
    {
        get
        {
            return outfitPageIdx;
        }
    }

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

    private int numberPages = 0;

    void Awake ()
    {
        for(int i=0; i<transform.childCount; i++)
        {
            if(i == 0)
            {
                topArrow = transform.GetChild(i).GetComponent<ClosetArrowItem>();
                topArrow.Closet = this;
            }
            else if(i == transform.childCount - 1)
            {
                bottomArrow = transform.GetChild(i).GetComponent<ClosetArrowItem>();
                bottomArrow.Closet = this;
            }
            else
            {
                OutfitItems[i-1] = transform.GetChild(i).GetComponent<ClosetOutfitItem>();
                OutfitItems[i - 1].Closet = this;
            }
        }
    }
    
    public void SetCloset(long userID, User.Gender userGender, List<Outfit> outfits, int pageIdx = 0)
    {
        this.ownerId = userID;
        this.ownerGender = userGender;
        this.outfits = outfits;
        this.outfitPageIdx = pageIdx;

        numberPages = Mathf.CeilToInt((float)outfits.Count / ClosetManager.NUMBER_CLOSET_ITEMS);

        SetClosetImage(GetDisplayedOutfits());
    }

    public void Clear()
    {
        ClearClosetImage();
        ownerId = 0L;
        ownerGender = User.Gender.None;
        if(outfits != null)
            outfits.Clear();
        outfitPageIdx = 0;
    }

    public void PageUp()
    {
        outfitPageIdx++;
        if (outfitPageIdx >= numberPages)
            outfitPageIdx = 0;

        SetClosetImage(GetDisplayedOutfits());
    }

    public void PageDown()
    {
        outfitPageIdx--;
        if (outfitPageIdx < 0)
            outfitPageIdx = numberPages - 1;
    }

    private void SetClosetImage(List<Outfit> outfits)
    {
        for(int i=0; i<OutfitItems.Length; i++)
        {
            OutfitItems[i].SetOutfit(outfits[i]);
        }

        topArrow.ShowItem();
        bottomArrow.ShowItem();
        isActive = true;
    }

    private void ClearClosetImage()
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

    private List<Outfit> GetDisplayedOutfits()
    {
        List<Outfit> dOutfits = new List<Outfit>();
        for (int i = 0; i < ClosetManager.NUMBER_CLOSET_ITEMS; i++)
        {
            dOutfits.Add(outfits[(outfitPageIdx * ClosetManager.NUMBER_CLOSET_ITEMS + i) % outfits.Count]);
        }

        return dOutfits;
    }

}
