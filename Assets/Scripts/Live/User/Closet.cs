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

    private float ownerPointDir;

    KinectManager kinect;

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

    void Start ()
    {
        if(!kinect)
            kinect = KinectManager.Instance;
    }

    void Update()
    {
        if(isActive)
        {
            if (kinect && kinect.IsInitialized())
            {
                if (kinect.IsUserTracked(ownerId))
                {
                    ownerPointDir = Mathf.Lerp(ownerPointDir, GetPointDirection(ClosetSide, ownerId), 0.25f);
                    if (ownerPointDir >= -0.75f && ownerPointDir < -0.5f)
                    {
                        OnBottomArrowHover();
                    }
                    else if (ownerPointDir >= -0.5f && ownerPointDir < -0.25f)
                    {
                        OnOutfitItemHover(3);
                    }
                    else if (ownerPointDir >= -0.25f && ownerPointDir < 0f)
                    {
                        OnOutfitItemHover(2);
                    }
                    else if (ownerPointDir >= 0f && ownerPointDir < 0.25f)
                    {
                        OnOutfitItemHover(1);
                    }
                    else if (ownerPointDir >= 0.25f && ownerPointDir < 0.5f)
                    {
                        OnOutfitItemHover(0);
                    }
                    else if (ownerPointDir >= 0.5f && ownerPointDir < 0.75f)
                    {
                        OnTopArrowHover();
                    }
                    else
                    {
                        OnUnselectAll();
                    }
                }
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

        SetClosetImage(GetDisplayedOutfits(outfits, outfitPageIdx));

        isActive = true;
    }

    public void ResetCloset()
    {
        ClearClosetImage();
        ownerId = 0L;
        ownerGender = User.Gender.None;
        outfits = null;
        outfitPageIdx = 0;
    }

    public void Clear()
    {
        ClearClosetImage();
        ownerId = 0L;
        ownerGender = User.Gender.None;
        if(outfits != null)
            outfits.Clear();
        outfitPageIdx = 0;
        isActive = false;
    }

    public void PageUp()
    {
        outfitPageIdx++;
        if (outfitPageIdx >= numberPages)
            outfitPageIdx = 0;

        SetClosetImage(GetDisplayedOutfits(outfits, outfitPageIdx));
    }

    public void PageDown()
    {
        outfitPageIdx--;
        if (outfitPageIdx < 0)
            outfitPageIdx = numberPages - 1;

        SetClosetImage(GetDisplayedOutfits(outfits, outfitPageIdx));
    }

    private void SetClosetImage(List<Outfit> outfits)
    {
        for(int i=0; i<OutfitItems.Length; i++)
        {
            OutfitItems[i].SetOutfit(outfits[i]);
        }

        topArrow.ShowArrow();
        bottomArrow.ShowArrow();
    }

    private void ClearClosetImage()
    {
        for (int i = 0; i < OutfitItems.Length; i++)
        {
            OutfitItems[i].ClearOutfit();
        }
        topArrow.HideArrow();
        bottomArrow.HideArrow();
    }

    private void OnTopArrowHover()
    {
        topArrow.OnItemHover();
        bottomArrow.OnItemUnselected();
        foreach(ClosetOutfitItem outfitItem in OutfitItems)
        {
            outfitItem.OnItemUnselected();
        }
    }

    private void OnBottomArrowHover()
    {
        topArrow.OnItemUnselected();
        bottomArrow.OnItemHover();
        foreach (ClosetOutfitItem outfitItem in OutfitItems)
        {
            outfitItem.OnItemUnselected();
        }
    }

    private void OnOutfitItemHover(int idx)
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

    private void OnUnselectAll()
    {
        topArrow.OnItemUnselected();
        bottomArrow.OnItemUnselected();
        foreach (ClosetOutfitItem outfitItem in OutfitItems)
        {
            outfitItem.OnItemUnselected();
        }
    }

    List<Outfit> GetDisplayedOutfits(List<Outfit> displayedOutfits, int displayedPage)
    {
        if (displayedOutfits.Count == 0)
            Debug.Log(string.Format("{0} [Closet] GetDisplayedOutfits(): displayedOutfits.Count = {1},  displayedPage = {2}", gameObject.name, displayedOutfits.Count, displayedPage));
        List<Outfit> dOutfits = new List<Outfit>();
        for (int i = 0; i < ClosetManager.NUMBER_CLOSET_ITEMS; i++)
        {
            dOutfits.Add(displayedOutfits[(displayedPage * ClosetManager.NUMBER_CLOSET_ITEMS + i) % displayedOutfits.Count]);
        }

        return dOutfits;
    }

    float GetPointDirection(Side closetSide, long ownerID)
    {
        if (closetSide == Closet.Side.Left)
        {
            Vector3 lHandPos = kinect.GetJointPosition(ownerID, (int)KinectInterop.JointType.HandLeft);
            Vector3 lShoulderPos = kinect.GetJointPosition(ownerID, (int)KinectInterop.JointType.ShoulderLeft);
            return (lHandPos - lShoulderPos).normalized.y;
        }
        else if (closetSide == Closet.Side.Right)
        {
            Vector3 rHandPos = kinect.GetJointPosition(ownerID, (int)KinectInterop.JointType.HandRight);
            Vector3 rShoulderPos = kinect.GetJointPosition(ownerID, (int)KinectInterop.JointType.ShoulderRight);
            return (rHandPos - rShoulderPos).normalized.y;
        }

        return -1f;
    }

}
