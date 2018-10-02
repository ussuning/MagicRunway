using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosetManager : MonoBehaviour {

    public static ClosetManager Instance;

    public static int NUMBER_CLOSET_ITEMS = 4;

    public Closet ClosetLeft;
    public Closet ClosetRight;

    private List<Closet> userClosets = new List<Closet>();

    Outfits outfits;

    KinectManager kinect;
    float Dir_lElbow;
    float Dir_rElbow;

    void Awake()
    {
        Instance = this;
    }

    public Closet GetUserCloset(int userIdx)
    {
        foreach(Closet c in userClosets)
        {
            if (c.OwnerIndex == userIdx)
                return c;
        }

        return null;
    }

    public void OnUserGenderSelected(int userIdx, User.Gender userGender)
    {
        Closet closet = ClosetManager.Instance.GetUserCloset(userIdx);
        List<Outfit> userOutfits = userGender == User.Gender.Female ? outfits.femaleOutfits : outfits.maleOutfits;

        if (closet)
        {
            if (closet.IsHidden)
            {
                closet.Clear();
                closet.SetCloset(userIdx, userGender, userOutfits);
                closet.Show();
            }    
        }
        else
        {
            if (userClosets.Count == 0)
                closet = ClosetLeft;
            else if (userClosets.Count == 1)
                closet = ClosetRight;

            if (closet)
            {
                closet.SetCloset(userIdx, userGender, userOutfits);
                userClosets.Add(closet);

                if (closet.IsHidden)
                    closet.Show();
            }
        }
    }

    public void OnEnterLiveMode()
    {
        if (!kinect)
            kinect = KinectManager.Instance;

        if (outfits == null)
            outfits = MRData.Instance.outfits;

        ClosetLeft.Clear();
        ClosetRight.Clear();
    }

    public void OnUserLost(int userIdx)
    {
        foreach(Closet c in userClosets)
        {
            if(c.OwnerIndex == userIdx)
            {
                userClosets.Remove(c);
                c.Clear(true);
                break;
            }
        }
    }

    void Start ()
    {
        ClosetLeft.Clear();
        ClosetRight.Clear();
    }

    void Update()
    {
        if (kinect && kinect.IsInitialized())
        {
            if(ClosetLeft.IsActive && ClosetRight.IsActive)
            {
                long userLID = ClosetLeft.OwnerID;
                long userRID = ClosetRight.OwnerID;
                if (kinect.IsUserTracked(userLID) && kinect.IsUserTracked(userRID))
                {
                    Vector3 userLPos = kinect.GetUserPosition(userLID);
                    Vector3 userRPos = kinect.GetUserPosition(userRID);
                    if(userRPos.x < userLPos.x)
                    {
                        int closetLOwnerIndex = ClosetLeft.OwnerIndex;
                        User.Gender closetLGender = ClosetLeft.OwnerGender;
                        List<Outfit> closetLOutfits = ClosetLeft.Outfits;
                        int closetLOutfitPageIndex = ClosetLeft.OutfitPageIndex;
                        ClosetLeft.ResetCloset();
                        ClosetLeft.SetCloset(ClosetRight.OwnerIndex, ClosetRight.OwnerGender, ClosetRight.Outfits, ClosetRight.OutfitPageIndex);
                        ClosetRight.ResetCloset();
                        ClosetRight.SetCloset(closetLOwnerIndex, closetLGender, closetLOutfits, closetLOutfitPageIndex);
                    }
                }
            }
        }
    }
}



