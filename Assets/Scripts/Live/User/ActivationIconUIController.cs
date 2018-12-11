using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivationIconUIController : MonoBehaviour {

    public float followSpeed = 1f;
    public float rotateSpeed = 1f;
    public Vector3 iconOffset = new Vector3(0f, 200f, -60f);
    public float scalingPosSmoothing = 80f;
    public float maxScale = 4f;
    public float minScale = 0.5f;
    public float maxRotation = 30f;

    //[Range(0f, 1f)]
    //public float debugProgressVal = 0f;
    public GameObject progressBarContainer;
    public GameObject progressBarTikPrefab;
    public int numberOfTiks = 20;
    public float tikDistance = 20f;
    public Color ProgressColor = Color.green;
    private GameObject[] progressBarTiks;

    KinectManager manager;
    Camera uiCamera;

    void OnEnable()
    {
        if (!uiCamera)
        {
            GameObject cameraGO = GameObject.Find("/Live runway/FittingRoom/Camera");
            uiCamera = cameraGO.GetComponent<Camera>();
        }

        if (!manager)
        {
            manager = KinectManager.Instance;
        }
    }

    void Start ()
    {
        GenerateTiks();
    }

    public bool SetUITransform(long userID)
    {
        Vector3 userScreenPos = GetUserScreenPos(userID);
        Vector3 newIconPos = userScreenPos + iconOffset;

        float kinectMaxDistance = manager.maxUserDistance;
        float kinectMinDistance = manager.minUserDistance;
        float scale = (kinectMaxDistance - userScreenPos.z) / (kinectMaxDistance - kinectMinDistance);
        scale = Mathf.Clamp(scale * (maxScale - minScale) + minScale, minScale, maxScale);
        transform.localScale = new Vector3(scale, scale, scale);

        newIconPos += Vector3.down * scalingPosSmoothing * (maxScale - scale) / 2f;

        float userRot = manager.GetUserOrientation(userID, false).eulerAngles.y;
        if ((userRot >= 0f && userRot <= maxRotation) || (userRot < 0f && userRot >= -maxRotation))
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, userRot, 0f), rotateSpeed);
        else if (userRot >= 360f - maxRotation && userRot <= 360f)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, userRot - 360f, 0f), rotateSpeed);

        transform.position = Vector3.Lerp(transform.position, newIconPos, followSpeed);

        return true;
    }

    Vector3 GetUserScreenPos(long userID)
    {
        Vector3 userScreenPos = Vector3.zero;
        if (manager && manager.IsInitialized())
        {
            // get the background rectangle (use the portrait background, if available)
            Rect backgroundRect = uiCamera.pixelRect;
            PortraitBackground portraitBack = PortraitBackground.Instance;

            if (portraitBack && portraitBack.enabled)
            {
                backgroundRect = portraitBack.GetBackgroundRect();
            }

            int iJointIndex = (int)KinectInterop.JointType.Head;
            if (manager.IsJointTracked(userID, iJointIndex))
            {
                userScreenPos = manager.GetJointPosColorOverlay(userID, iJointIndex, uiCamera, backgroundRect);
            }
        }

        return userScreenPos;
    }


    /***************************    PROGRESS BAR      ****************************************/

    private void GenerateTiks ()
    {
        progressBarTiks = new GameObject[numberOfTiks];
        for (int i = 0; i < numberOfTiks; i++)
        {
            float degSpacing = i * 360f / numberOfTiks + 90f;
            float radSpacing = degSpacing * Mathf.Deg2Rad;
            Vector3 tikPos = tikDistance * new Vector3(Mathf.Cos(radSpacing), Mathf.Sin(radSpacing), 0f);
            Quaternion tikRot = Quaternion.Euler(0f, 0f, degSpacing - 90);
            GameObject tikGO = Instantiate(progressBarTikPrefab, tikPos, tikRot, progressBarContainer.transform);
            progressBarTiks[numberOfTiks - 1 - i] = tikGO;

            tikGO.name = string.Format("Tik{0}", i);
            tikGO.SetActive(true);
        }
    }

    public void ResetProgress()
    {
        SetProgressValue(0);
    }

    public void SetProgressValue(float v)
    {
        // v = [0, 1]
        int numColoredTiks = Mathf.FloorToInt(numberOfTiks * v);
        for(int i=0; i<numColoredTiks; i++)
        {
            GameObject tikGO = progressBarTiks[i];
            tikGO.GetComponent<Renderer>().material.color = ProgressColor;
        }
        for(int i=numColoredTiks; i<numberOfTiks; i++)
        {
            GameObject tikGO = progressBarTiks[i];
            tikGO.GetComponent<Renderer>().material.color = Color.white;
        }
    }
}
