using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseFX : MonoBehaviour {

    public GameObject[] partileFX;
    public Sprite[] poseThumbnails;

    SpriteRenderer poseImage;
    TextMesh comboText;

    void Awake ()
    {
        poseImage = GetComponentInChildren<SpriteRenderer>();
        comboText = GetComponentInChildren<TextMesh>();
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
        int combo = (int)param;
        int poseIdx = (int)paramEx;

        UpdateComboParticles(combo);
        UpdateDetectedPoseImage(poseIdx, combo);
    }

    void UpdateComboParticles(int combo)
    {
        GameObject particleGO;
        if (combo <= partileFX.Length)
            particleGO = (GameObject)Instantiate(partileFX[combo - 1], transform.parent.position + new Vector3(0, 1, 0), Quaternion.identity);
        else
            particleGO = (GameObject)Instantiate(partileFX[partileFX.Length - 1], transform.parent.position + new Vector3(0, 1, 0), Quaternion.identity);
    }

    void UpdateDetectedPoseImage(int poseIdx, float combo = 0f)
    {
        CancelInvoke("ClearDetection");

        poseImage.sprite = poseThumbnails[poseIdx];
        poseImage.enabled = true;

        if (combo > 1)
            comboText.text = string.Format("x{0} COMBO", combo.ToString());

        Invoke("ClearDetection", 2f);
    }

    void ClearDetection()
    {
        poseImage.enabled = false;
        comboText.text = "";
    }
}
