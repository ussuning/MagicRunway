﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosetManager : MonoBehaviour {

    public static int NUMBER_CLOSET_ITEMS = 4;

    public Closet ClosetLeft;
    public Closet ClosetRight;

    private List<Closet> userClosets = new List<Closet>();

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

        ClosetLeft.Clear();
        ClosetRight.Clear();
    }

    public void OnUserGenderSelected(object [] param)
    {
        long userID = (long)param[0];
        User.Gender userGender = (User.Gender)param[1];

        List<Outfit> userOutfits = userGender == User.Gender.Female ? outfits.femaleOutfits : outfits.maleOutfits;

        if (userClosets.Count == 0)
        {
            ClosetLeft.SetCloset(userID, userGender, userOutfits);
            userClosets.Add(ClosetLeft);
        }
        else if(userClosets.Count == 1)
        {
            KinectManager kinect = KinectManager.Instance;
            Vector3 user1Pos = kinect.GetUserPosition(userClosets[0].OwnerID);
            Vector3 user2Pos = kinect.GetUserPosition(userID);
            
            if (user2Pos.x < user1Pos.x)
            {   
                ClosetRight.SetCloset(ClosetLeft.OwnerID, ClosetLeft.OwnerGender, ClosetLeft.Outfits, ClosetLeft.OutfitPageIndex);
                userClosets[0] = ClosetRight;
                ClosetLeft.Clear();
                ClosetLeft.SetCloset(userID, userGender, userOutfits);
                userClosets.Add(ClosetLeft);
            }
            else
            {
                ClosetRight.SetCloset(userID, userGender, userOutfits);
                userClosets.Add(ClosetRight);
            }
        }
    }

    public void OnUserLost(object [] param)
    {
        long userID = (long)param[0];

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
        if (kinect && kinect)
        {
            foreach (Closet closet in userClosets)
            {
                long userID = closet.OwnerID;
                if(kinect.IsUserInKinectView(userID))
                {
                    if(closet.ClosetSide == Closet.Side.Left)
                    {
                        Dir_lElbow = Mathf.Lerp(Dir_lElbow, kinect.GetJointDirection(userID, (int)KinectInterop.JointType.ElbowLeft).y, 0.25f);

                        if(Dir_lElbow >= -0.2f && Dir_lElbow < -0.15f)
                        {
                            closet.OnBottomArrowHover();
                        }
                        else if (Dir_lElbow >= -0.15f && Dir_lElbow < -0.075f)
                        {
                            closet.OnOutfitItemHover(3);
                        }
                        else if (Dir_lElbow >= -0.075f && Dir_lElbow < 0f)
                        {
                            closet.OnOutfitItemHover(2);
                        }
                        else if (Dir_lElbow >= 0f && Dir_lElbow < 0.075f)
                        {
                            closet.OnOutfitItemHover(1);
                        }
                        else if (Dir_lElbow >= 0.075f && Dir_lElbow < 0.15f)
                        {
                            closet.OnOutfitItemHover(0);
                        }
                        else if (Dir_lElbow >= 0.15f && Dir_lElbow < 2f)
                        {
                            closet.OnTopArrowHover();
                        }
                        else
                        {
                            closet.OnUnselectAll();
                        }
                    }
                    else if (closet.ClosetSide == Closet.Side.Right)
                    {
                        Dir_rElbow = Mathf.Lerp(Dir_lElbow, kinect.GetJointDirection(userID, (int)KinectInterop.JointType.ElbowRight).y, 0.25f);
                        if (Dir_rElbow >= -0.2f && Dir_rElbow < -0.15f)
                        {
                            closet.OnBottomArrowHover();
                        }
                        else if (Dir_rElbow >= -0.15f && Dir_rElbow < -0.075f)
                        {
                            closet.OnOutfitItemHover(3);
                        }
                        else if (Dir_rElbow >= -0.075f && Dir_rElbow < 0f)
                        {
                            closet.OnOutfitItemHover(2);
                        }
                        else if (Dir_rElbow >= 0f && Dir_rElbow < 0.075f)
                        {
                            closet.OnOutfitItemHover(1);
                        }
                        else if (Dir_rElbow >= 0.075f && Dir_rElbow < 0.15f)
                        {
                            closet.OnOutfitItemHover(0);
                        }
                        else if (Dir_rElbow >= 0.15f && Dir_rElbow < 2f)
                        {
                            closet.OnTopArrowHover();
                        }
                        else
                        {
                            closet.OnUnselectAll();
                        }
                    }
                }
            }
        }
    }
}
