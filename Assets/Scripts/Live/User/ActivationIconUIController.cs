using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivationIconUIController : MonoBehaviour {

    public float followSpeed = 1f;
    public float rotateSpeed = 1f;
    public Vector3 IconOffset = new Vector3(0f, 200f, -60f);
    public float scalingPosSmoothing = 80f;
    public float maxScale = 4f;
    public float minScale = 0.5f;
    public float maxRotation = 30f;

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

    public bool SetUITransform(long userID)
    {
        Vector3 userScreenPos = GetUserScreenPos(userID);
        Vector3 newIconPos = userScreenPos + IconOffset;

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
}
