using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickmanPosing : MonoBehaviour {

    private Animator anim;

    void Awake ()
    {
        anim = GetComponent<Animator>();
    }

    public void OnNewPoseGenerated(int poseID)
    {
        if (anim)
            anim.SetInteger("pose", poseID);
    }
}
