using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPoseGenerator : MonoBehaviour {

    public int curPoseID;

    void Start ()
    {
        curPoseID = 0;
        EventMsgDispatcher.Instance.TriggerEvent(EventDef.New_Pose_Generated, curPoseID);
    }

	void Update () {
        if (Input.GetKeyDown(KeyCode.P))
        {
            int newPoseID = 0;
            do
            {
                newPoseID = Random.Range(1, 1 + BrainDataManager.Instance.NumPoses);
            } while (newPoseID == curPoseID);
            curPoseID = newPoseID;

            EventMsgDispatcher.Instance.TriggerEvent(EventDef.New_Pose_Generated, curPoseID);
        }
	}
}
