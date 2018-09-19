using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosetInfo
{
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

    private Closet closet;
    public Closet Closet
    {
        set
        {
            closet = value;
            closet.SetCloset(GetDisplayedOutfits());
        }
        get
        {
            return closet;
        }
    }

    private List<Outfit> outfits;
    private int outfitPageIdx = 0;

    public ClosetInfo(long userID, User.Gender userGender, Outfits allOutfits)
    {
        ownerId = userID;
        ownerGender = userGender;

        switch(userGender)
        {
            case User.Gender.Male:
                outfits = allOutfits.maleOutfits;
                break;
            case User.Gender.Female:
                outfits = allOutfits.femaleOutfits;
                break;
        }

        outfitPageIdx = 0;
    }

    public void Clear()
    {
        closet.ClearCloset();
        ownerId = 0L;
        ownerGender = User.Gender.None;
        outfits.Clear();
        outfitPageIdx = 0;  
    }

    private List<Outfit> GetDisplayedOutfits()
    {
        List<Outfit> dOutfits = new List<Outfit>();
        for(int i=0; i<ClosetManager.NUMBER_CLOSET_ITEMS; i++)
        {
            dOutfits.Add(outfits[(outfitPageIdx * ClosetManager.NUMBER_CLOSET_ITEMS + i) % outfits.Count]);
        }

        return dOutfits;
    }
}

public class ClosetManager : MonoBehaviour {

    public static int NUMBER_CLOSET_ITEMS = 4;

    public Closet ClosetLeft;
    public Closet ClosetRight;

    private List<ClosetInfo> userClosets = new List<ClosetInfo>();

    Outfits outfits;

    KinectManager kinect;
    float Dir_lElbow;
    float Dir_rElbow;

    void OnEnable ()
    {
        EventMsgDispatcher.Instance.registerEvent(EventDef.Live_Mode_Set_Up, OnEnterLiveMode);
        EventMsgDispatcher.Instance.registerEvent(EventDef.User_Gender_Selected, OnUserGenderSelected);
        EventMsgDispatcher.Instance.registerEvent(EventDef.Kinect_User_Lost, OnUserLost);
    }

    void OnDisable()
    {
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.Live_Mode_Set_Up, OnEnterLiveMode);
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.User_Gender_Selected, OnUserGenderSelected);
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.Kinect_User_Lost, OnUserLost);
    }

    public void OnEnterLiveMode(object [] param)
    {
        if (!kinect)
            kinect = KinectManager.Instance;

        if (outfits == null)
            outfits = MRData.Instance.outfits;

        ClosetLeft.ClearCloset();
        ClosetRight.ClearCloset();
    }

    public void OnUserGenderSelected(object [] param)
    {
        long userID = (long)param[0];
        User.Gender userGender = (User.Gender)param[1];

        if (userClosets.Count == 0)
        {
            ClosetInfo uCloset = new ClosetInfo(userID, userGender, outfits);
            uCloset.Closet = ClosetLeft;
            userClosets.Add(uCloset);
        }
        else if(userClosets.Count == 1)
        {
            KinectManager kinect = KinectManager.Instance;
            Vector3 user1Pos = kinect.GetUserPosition(userClosets[0].OwnerID);
            Vector3 user2Pos = kinect.GetUserPosition(userID);

            ClosetInfo uCloset = new ClosetInfo(userID, userGender, outfits);
            if (user2Pos.x < user1Pos.x)
            {
                userClosets[0].Closet = ClosetRight;
                uCloset.Closet = ClosetLeft;
            }
            else
            {
                uCloset.Closet = ClosetRight;
            }
            userClosets.Add(uCloset);
        }
    }

    public void OnUserLost(object [] param)
    {
        long userID = (long)param[0];

        foreach(ClosetInfo uc in userClosets)
        {
            if(uc.OwnerID == userID)
            {
                userClosets.Remove(uc);
                uc.Clear();
                break;
            }
        }
    }

    void Start ()
    {
        ClosetLeft.ClearCloset();
        ClosetRight.ClearCloset();
    }

    void Update()
    {
        if (kinect && kinect)
        {
            foreach (ClosetInfo closetInfo in userClosets)
            {
                long userID = closetInfo.OwnerID;
                if(kinect.IsUserInKinectView(userID))
                {
                    if(closetInfo.Closet.ClosetSide == Closet.Side.Left)
                    {
                        Dir_lElbow = Mathf.Lerp(Dir_lElbow, kinect.GetJointDirection(userID, (int)KinectInterop.JointType.ElbowLeft).y, 0.25f);

                        if(Dir_lElbow >= -0.2f && Dir_lElbow < -0.15f)
                        {
                            closetInfo.Closet.OnBottomArrowHover();
                        }
                        else if (Dir_lElbow >= -0.15f && Dir_lElbow < -0.075f)
                        {
                            closetInfo.Closet.OnOutfitItemHover(3);
                        }
                        else if (Dir_lElbow >= -0.075f && Dir_lElbow < 0f)
                        {
                            closetInfo.Closet.OnOutfitItemHover(2);
                        }
                        else if (Dir_lElbow >= 0f && Dir_lElbow < 0.075f)
                        {
                            closetInfo.Closet.OnOutfitItemHover(1);
                        }
                        else if (Dir_lElbow >= 0.075f && Dir_lElbow < 0.15f)
                        {
                            closetInfo.Closet.OnOutfitItemHover(0);
                        }
                        else if (Dir_lElbow >= 0.15f && Dir_lElbow < 2f)
                        {
                            closetInfo.Closet.OnTopArrowHover();
                        }
                        else
                        {
                            closetInfo.Closet.OnUnselectAll();
                        }
                    }
                    else if (closetInfo.Closet.ClosetSide == Closet.Side.Right)
                    {
                        Dir_rElbow = Mathf.Lerp(Dir_lElbow, kinect.GetJointDirection(userID, (int)KinectInterop.JointType.ElbowRight).y, 0.25f);
                        if (Dir_rElbow >= -0.2f && Dir_rElbow < -0.15f)
                        {
                            closetInfo.Closet.OnBottomArrowHover();
                        }
                        else if (Dir_rElbow >= -0.15f && Dir_rElbow < -0.075f)
                        {
                            closetInfo.Closet.OnOutfitItemHover(3);
                        }
                        else if (Dir_rElbow >= -0.075f && Dir_rElbow < 0f)
                        {
                            closetInfo.Closet.OnOutfitItemHover(2);
                        }
                        else if (Dir_rElbow >= 0f && Dir_rElbow < 0.075f)
                        {
                            closetInfo.Closet.OnOutfitItemHover(1);
                        }
                        else if (Dir_rElbow >= 0.075f && Dir_rElbow < 0.15f)
                        {
                            closetInfo.Closet.OnOutfitItemHover(0);
                        }
                        else if (Dir_rElbow >= 0.15f && Dir_rElbow < 2f)
                        {
                            closetInfo.Closet.OnTopArrowHover();
                        }
                        else
                        {
                            closetInfo.Closet.OnUnselectAll();
                        }
                    }
                }
            }
        }
    }
}
