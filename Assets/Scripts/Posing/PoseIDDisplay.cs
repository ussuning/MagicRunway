using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PoseIDDisplay : MonoBehaviour {

    Text PoseText;

    void Awake ()
    {
        PoseText = GetComponent<Text>();
    }

    void OnEnable()
    {
        EventMsgDispatcher.Instance.registerEvent(EventDef.New_Pose_Generated, OnNewPoseGenerated);
    }

    void OnDisable()
    {
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.New_Pose_Generated, OnNewPoseGenerated);
    }

    public void OnNewPoseGenerated(object param, object paramEx)
    {
        int poseID = (int)param;
        if (PoseText)
            PoseText.text = string.Format("Pose: {0}", poseID);
    }
}
