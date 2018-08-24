﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickmanPosing : MonoBehaviour {

    private Animator anim;

    void Awake ()
    {
        anim = GetComponent<Animator>();
    }

    void OnEnable()
    {
        EventMsgDispatcher.Instance.registerEvent(EventDef.New_Pose_Generated, OnNewPoseGenerated);
    }

    void OnDisable()
    {
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.New_Pose_Generated, OnNewPoseGenerated);
    }

    public void OnNewPoseGenerated(object param, object paramEx, object paramEx2)
    {
        int poseID = (int)param;
        if (anim)
            anim.SetInteger("pose", poseID);
    }
}
