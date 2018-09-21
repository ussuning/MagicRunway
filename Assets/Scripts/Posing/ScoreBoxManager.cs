using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreBoxManager : MonoBehaviour {

    KinectManager kinect;

    void Start ()
    {
        kinect = KinectManager.Instance;
    }

	void Update ()
    {
        if (kinect && kinect.IsInitialized()) {
            int numScoreBoxes = transform.childCount;
            if (numScoreBoxes == 2)
            {
                Transform boxTransformL = transform.GetChild(0);
                Transform boxTransformR = transform.GetChild(1);

                UserScore uScoreL = boxTransformL.GetComponent<UserScore>();
                UserScore uScoreR = boxTransformR.GetComponent<UserScore>();

                if (kinect.IsUserTracked(uScoreL.UserID) && kinect.IsUserTracked(uScoreR.UserID)) {
                    Vector3 userPosL = kinect.GetUserPosition(uScoreL.UserID);
                    Vector3 userPosR = kinect.GetUserPosition(uScoreR.UserID);
                    if (userPosL.x > userPosR.x)
                    {
                        boxTransformL.parent = null;
                        boxTransformL.SetParent(this.transform);
                    }
                }
            }
        }
	}
}
