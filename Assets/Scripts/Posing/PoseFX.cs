using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseFX : MonoBehaviour {

    public GameObject partileFX;

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
        int combo = (int)param;
        int poseIdx = (int)paramEx;

        UpdateComboParticles();
    }

    void UpdateComboParticles()
    {
        GameObject particleGO;
        if (partileFX)
            particleGO = (GameObject)Instantiate(partileFX, transform.position, Quaternion.identity);
    } 
}
