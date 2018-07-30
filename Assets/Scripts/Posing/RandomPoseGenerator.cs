using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPoseGenerator : MonoBehaviour {

    public bool isRandom = true;
    public int targetPoseID = 1;
    public int curPoseID;

    void Start ()
    {
        curPoseID = 0;
        EventMsgDispatcher.Instance.TriggerEvent(EventDef.New_Pose_Generated, curPoseID);
    }

	void Update () {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isRandom)
            {
                int newPoseID = 0;
                do
                {
                    newPoseID = Random.Range(1, 1 + BrainDataManager.Instance.NumPoses);
                } while (newPoseID == curPoseID);
                curPoseID = newPoseID;

                EventMsgDispatcher.Instance.TriggerEvent(EventDef.New_Pose_Generated, curPoseID);
            }
            else
            {
                curPoseID = targetPoseID;

                EventMsgDispatcher.Instance.TriggerEvent(EventDef.New_Pose_Generated, curPoseID);
            }
        }
	}
}
