using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseRecognitionSwitch : MonoBehaviour {

    public Agent agent;

	void OnEnable () {
        EventMsgDispatcher.Instance.registerEvent(EventDef.Kinect_User_ID_Added, OnUserAdded);
	}
	
	void OnDisable () {
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.Kinect_User_ID_Added, OnUserAdded);
    }

    public void OnUserAdded(object param, object paramEx)
    {
        if (agent)
            agent.enabled = true;
    }
}
