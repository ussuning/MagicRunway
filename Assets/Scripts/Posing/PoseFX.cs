using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseFX : MonoBehaviour {

    public GameObject[] partileFX;
    public Sprite[] poseThumbnails;

    SpriteRenderer poseImage;
    TextMesh confidenceText;

    void Awake ()
    {
        poseImage = GetComponentInChildren<SpriteRenderer>();
        confidenceText = GetComponentInChildren<TextMesh>();
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
        //int combo = (int)param;
        int poseIdx = (int)paramEx;
        float poseConfidence = (float)param;

        //UpdateComboParticles(combo);
        UpdateDetectedPoseImage(poseIdx, poseConfidence);
    }

    void UpdateComboParticles(int combo)
    {
        GameObject particleGO;
        if (combo <= partileFX.Length)
            particleGO = (GameObject)Instantiate(partileFX[combo - 1], transform.parent.position, Quaternion.identity);
        else
            particleGO = (GameObject)Instantiate(partileFX[partileFX.Length - 1], transform.parent.position, Quaternion.identity);
    }

    void UpdateDetectedPoseImage(int poseIdx, float confidence = 0f)
    {
        CancelInvoke("ClearDetection");

        poseImage.sprite = poseThumbnails[poseIdx];
        poseImage.enabled = true;

        confidenceText.text = (Mathf.Round(confidence * 100)).ToString();

        Invoke("ClearDetection", 2f);
    }

    void ClearDetection()
    {
        poseImage.enabled = false;
        confidenceText.text = "";
    }
}
