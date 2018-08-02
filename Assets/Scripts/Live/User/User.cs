using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User : MonoBehaviour {

    public int uindex;
    public long uid;
    public string ugender;    // switch to enum later
    public Vector3 position; 
  
    // Use this for initialization
    public User(long id, int index)
    {
        uid = id;
        uindex = index;
    }

    public void Initialize(long id, int index)
    {
        uid = id;
        uindex = index;
    }

    public long getUserId()
    {
        return uid;
    }

    public int getUserIndex()
    {
        return uindex;
    }

    public string getGender()
    {
        return ugender;
    }

    public Vector3 getPosition()
    {
        return position;
    }

    public void setGender(string gender)
    {
        ugender = gender;
    }

    public Vector3 getCurrentPosition()
    {
        KinectManager manager = KinectManager.Instance;

        if (manager && manager.IsInitialized())
        {
            // get the background rectangle (use the portrait background, if available)
            Camera foregroundCamera = Camera.main;
            Rect backgroundRect = foregroundCamera.pixelRect;
            PortraitBackground portraitBack = PortraitBackground.Instance;

            if (portraitBack && portraitBack.enabled)
            {
                backgroundRect = portraitBack.GetBackgroundRect();
            }

            int iJointIndex = (int)KinectInterop.JointType.SpineMid;
            if (manager.IsJointTracked(uid, iJointIndex))
            {
                return manager.GetJointPosColorOverlay(uid, iJointIndex, foregroundCamera, backgroundRect);
            }
        }

        return Vector3.zero;
    }

    void Update()
    {
        // get this user's pos on every tick
        position = getCurrentPosition();
    }

}
