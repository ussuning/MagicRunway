using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseMatchingManager : MonoBehaviour {

    public GameObject matchedFXParticlesPrefab;

    void OnEnable()
    {
        EventMsgDispatcher.Instance.registerEvent(EventDef.User_Pose_Matched, OnPoseMatched);
    }

    void OnDisable()
    {
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.User_Pose_Matched, OnPoseMatched);
    }

    public void OnPoseMatched(object[] param)
    {
        int userIdx = (int)param[0];
        int poseID = (int)param[1];
        float pose_confidence = (float)param[2];
        GameObject particleGO = (GameObject)Instantiate(matchedFXParticlesPrefab, GetUserScreenPos(userIdx), Quaternion.identity);
    }

    Vector3 GetUserScreenPos(int userIdx)
    {
        KinectManager manager = KinectManager.Instance;

        if (manager && manager.IsInitialized())
        {
            long userID = manager.GetUserIdByIndex(userIdx);

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
