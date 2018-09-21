using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PoseMgr : MonoBehaviour {

    public static PoseMgr Instance;

    public RawImage PoseImage;

    public float ImageFadeSpeed = 1f;

    private bool isImageFadingIN;
    private float imageAlpha;


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
        HidePoseImage();
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
        int newPose = 0;
        do
        {
            newPose = UnityEngine.Random.Range(1, 1 + BrainDataManager.Instance.NumPoses);
        } while (newPose == curPose || newPose == prevPose);

        prevPose = curPose;
        curPose = newPose;

        StartCoroutine(SetNewPoseCooldown());

        object[] param = { curPose };
        EventMsgDispatcher.Instance.TriggerEvent(EventDef.New_Pose_Generated, param);
    }

    public void HidePoseImage()
    {
        isImageFadingIN = false;
        imageAlpha = 0f;
        PoseImage.color = new Color(1f, 1f, 1f, imageAlpha);
        PoseImage.enabled = false;
    }

    IEnumerator SetNewPoseCooldown()
    {
        isInNewPoseCD = true;
        yield return new WaitForSeconds(PoseCD);
        isInNewPoseCD = false;
    }
}
