using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosetManager : MonoBehaviour {

    public static ClosetManager Instance;

    public static int NUMBER_CLOSET_ITEMS = 4;

    public Closet ClosetLeft;
    public Closet ClosetRight;

    private List<Closet> userClosets = new List<Closet>();
    private int[] closestUsers = new int[LiveRunwayManager.NUM_OF_ACTIVE_USERS];

    Outfits outfits;

    KinectManager kinect;

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

    public void UpdateClosestUsers(int [] users)
    {
        if (users == null)
            return;

        closestUsers[0] = users.Length >= 1 ? users[0] : -1;
        closestUsers[1] = users.Length >= 2 ? users[1] : -1;
    }

    public void OnUserGenderSelected(int userIdx, User.Gender userGender)
    {
        Closet closet = GetUserCloset(userIdx);
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
            {
                if (userIdx == closestUsers[0] && closestUsers[1] != -1)
                {
                    float user1XPos = kinect.GetUserPosition(kinect.GetUserIdByIndex(userIdx)).x;
                    float user2XPos = kinect.GetUserPosition(kinect.GetUserIdByIndex(closestUsers[1])).x;
                    if(user1XPos > user2XPos)
                    {
                        closet = ClosetRight;
                    }
                    else
                    {
                        closet = ClosetLeft;
                    }
                }
                else if (userIdx == closestUsers[1] && closestUsers[0] != -1)
                {
                    float user1XPos = kinect.GetUserPosition(kinect.GetUserIdByIndex(userIdx)).x;
                    float user2XPos = kinect.GetUserPosition(kinect.GetUserIdByIndex(closestUsers[0])).x;
                    if (user1XPos > user2XPos)
                    {
                        closet = ClosetRight;
                    }
                    else
                    {
                        closet = ClosetLeft;
                    }
                }
                else
                {
                    float xPos = kinect.GetUserPosition(kinect.GetUserIdByIndex(userIdx)).x;
                    if (xPos > 0)
                        closet = ClosetRight;
                    else
                        closet = ClosetLeft;
                }
            }
            else if (userClosets.Count == 1)
            {
                if (ClosetLeft.IsActive)
                    closet = ClosetRight;
                else
                    closet = ClosetLeft;
            }
                

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



