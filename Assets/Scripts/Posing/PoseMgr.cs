using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PoseMgr : MonoBehaviour {

    public static PoseMgr Instance;

    public StickmanPosing Stickman;
    public RawImage PoseImage;

    public float ImageFadeSpeed = 1f;

    private bool isImageFadingIN;
    private float imageAlpha;

    public float PoseDuration = 5f;
    public float PoseCD = 0.75f;

    private int curPose = 0;
    private int prevPose = 0;
    private bool isInNewPoseCD = false;
    public bool IsInNewPoseCooldown
    {
        get
        {
            return isInNewPoseCD;
        }
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StopPosing();
    }

    void Update()
    {
        if(isImageFadingIN)
        {
            imageAlpha += Time.deltaTime * ImageFadeSpeed;
            PoseImage.color = new Color(1f, 1f, 1f, imageAlpha);
            if (imageAlpha >= 1f) //Fade in completed
            {
                isImageFadingIN = false;
                GenerateNewPose();
            }
        }
    }

    public void StartPosing()
    {
        PoseImage.enabled = true;
        isImageFadingIN = true;
    }

    public void GenerateNewPose()
    {
        CancelInvoke("OnNoPoseMatched");

        int newPose = 0;
        do
        {
            newPose = UnityEngine.Random.Range(1, 1 + BrainDataManager.Instance.NumPoses);
        } while (newPose == curPose || newPose == prevPose);

        //Debug
        //int newPose = curPose + 1;
        //if (newPose == BrainDataManager.Instance.NumPoses + 1)
        //    newPose = 1;

        prevPose = curPose;
        curPose = newPose;

        StartCoroutine(SetNewPoseCooldown());

        Stickman.OnNewPoseGenerated(curPose);

        object[] param = { curPose };
        EventMsgDispatcher.Instance.TriggerEvent(EventDef.New_Pose_Generated, param);

        Invoke("OnNoPoseMatched", PoseDuration);
    }

    public void StopPosing()
    {
        CancelInvoke("GenerateNewPose");

        curPose = 0;
        prevPose = 0;
        isInNewPoseCD = false;

        Stickman.OnNewPoseGenerated(curPose);

        object[] param = { curPose };
        EventMsgDispatcher.Instance.TriggerEvent(EventDef.New_Pose_Generated, param);

        isImageFadingIN = false;
        imageAlpha = 0f;
        PoseImage.color = new Color(1f, 1f, 1f, imageAlpha);
        PoseImage.enabled = false;
    }

    private void OnNoPoseMatched()
    {
        PoseMatchingManager.Instance.OnNoPoseMatched();
        GenerateNewPose();
    }

    IEnumerator SetNewPoseCooldown()
    {
        isInNewPoseCD = true;
        yield return new WaitForSeconds(PoseCD);
        isInNewPoseCD = false;
    }
}
