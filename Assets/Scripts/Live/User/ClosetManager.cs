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

    void OnEnable ()
    {
        EventMsgDispatcher.Instance.registerEvent(EventDef.User_Gender_Selected, OnUserGenderSelected);
    }

    void OnDisable()
    {
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.User_Gender_Selected, OnUserGenderSelected);
    }

    public void OnUserGenderSelected(object[] param)
    {
        long userID = (long)param[0];
        User.Gender userGender = (User.Gender)param[1];

        List<Outfit> userOutfits = userGender == User.Gender.Female ? outfits.femaleOutfits : outfits.maleOutfits;
        if (userClosets.Count == 0)
        {
            ClosetLeft.SetCloset(userID, userGender, userOutfits);
            userClosets.Add(ClosetLeft);
        }
        else if (userClosets.Count == 1)
        {
            ClosetRight.SetCloset(userID, userGender, userOutfits);
            userClosets.Add(ClosetRight);
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

    public void OnUserLost(long userID)
    {
        foreach(Closet c in userClosets)
        {
            if(c.OwnerID == userID)
            {
                userClosets.Remove(c);
                c.Clear();
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
                        long closetLOwnerID = ClosetLeft.OwnerID;
                        User.Gender closetLGender = ClosetLeft.OwnerGender;
                        List<Outfit> closetLOutfits = ClosetLeft.Outfits;
                        int closetLOutfitPageIndex = ClosetLeft.OutfitPageIndex;
                        ClosetLeft.ResetCloset();
                        ClosetLeft.SetCloset(ClosetRight.OwnerID, ClosetRight.OwnerGender, ClosetRight.Outfits, ClosetRight.OutfitPageIndex);
                        ClosetRight.ResetCloset();
                        ClosetRight.SetCloset(closetLOwnerID, closetLGender, closetLOutfits, closetLOutfitPageIndex);
                    }
                }
            }
        }
    }
}



