using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseFX : MonoBehaviour {

    public GameObject partileFX;

    private long userID;

    public void Init(long user)
    {
        userID = user;
    }

    void OnEnable()
    {
        EventMsgDispatcher.Instance.registerEvent(EventDef.User_Pose_Detected, OnPoseDetected);
    }

    void OnDisable ()
    {
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.User_Pose_Detected, OnPoseDetected);
    }

    public void OnPoseDetected(object param, object paramEx)
    {
        long matched_userID = (long)param;
        if(matched_userID == userID)
            UpdateComboParticles();
    }

    void UpdateComboParticles()
    {
        GameObject particleGO;
        if (partileFX)
            particleGO = (GameObject)Instantiate(partileFX, GetUserScreenPos(), Quaternion.identity);
    }

    Vector3 GetUserScreenPos()
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
            if (manager.IsJointTracked(userID, iJointIndex))
            {
                return manager.GetJointPosColorOverlay(userID, iJointIndex, foregroundCamera, backgroundRect);
            }      
        }

        return Vector3.zero;
    }

}
