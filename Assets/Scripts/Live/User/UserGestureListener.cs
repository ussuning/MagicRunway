using UnityEngine;
using System.Collections;
using System;

public class UserGestureListener : MonoBehaviour, KinectGestures.GestureListenerInterface
{
    public int playerIndex = 0;
    public long uid;
    public UnityEngine.UI.Text gestureInfo;
    private bool progressDisplayed;
    private float progressGestureTime;
   
    void Start()
    {
        Debug.Log("User Gesture Listener Start");
    }

    public void initialize(long userId, int userIndex)
    {
        Debug.Log("User Gesture Listener initialized");
        playerIndex = userIndex;
        uid = userId;
    }

    public void UserDetected(long userId, int userIndex)
    {
        Debug.Log("User Gesture Listener, User Detected");

        if (userIndex != playerIndex)
        {
            Debug.Log("UserGestureListener User Detected, Player Index = " + playerIndex);
            return;
        }

        if (gestureInfo != null)
        {
            gestureInfo.text = "Female: Raise Left Hand, Male: Raise Right Hand.";
        }

    }

    public void UserLost(long userId, int userIndex)
    {
        // the gestures are allowed for the primary user only
        if (userIndex != playerIndex)
            return;

        if (gestureInfo != null)
        {
            gestureInfo.text = string.Empty;
        }
    }
    

    public void GestureInProgress(long userId, int userIndex, KinectGestures.Gestures gesture,
                                  float progress, KinectInterop.JointType joint, Vector3 screenPos)
    {
        // the gestures are allowed for the primary user only
        if (userIndex != playerIndex)
            return;

        if ((gesture == KinectGestures.Gestures.ZoomOut || gesture == KinectGestures.Gestures.ZoomIn) && progress > 0.5f)
        {
            if (gestureInfo != null)
            {
                string sGestureText = string.Format("{0} - {1:F0}%", gesture, screenPos.z * 100f);
                gestureInfo.text = sGestureText;

                progressDisplayed = true;
                progressGestureTime = Time.realtimeSinceStartup;
            }
        }
        else if ((gesture == KinectGestures.Gestures.Wheel || gesture == KinectGestures.Gestures.LeanLeft ||
                 gesture == KinectGestures.Gestures.LeanRight) && progress > 0.5f)
        {
            if (gestureInfo != null)
            {
                string sGestureText = string.Format("{0} - {1:F0} degrees", gesture, screenPos.z);
                gestureInfo.text = sGestureText;

                progressDisplayed = true;
                progressGestureTime = Time.realtimeSinceStartup;
            }
        }
        else if (gesture == KinectGestures.Gestures.Run && progress > 0.5f)
        {
            if (gestureInfo != null)
            {
                string sGestureText = string.Format("{0} - progress: {1:F0}%", gesture, progress * 100);
                gestureInfo.text = sGestureText;

                progressDisplayed = true;
                progressGestureTime = Time.realtimeSinceStartup;
            }
        }
    }

    public bool GestureCompleted(long userId, int userIndex, KinectGestures.Gestures gesture,
                                  KinectInterop.JointType joint, Vector3 screenPos)
    {

        Debug.Log("Gesture Completed: " + gesture + " " + userIndex + " " + userId);

        // the gestures are allowed for the primary user only
        if (userIndex != playerIndex)
            return false;

        if (gestureInfo != null)
        {
            string sGestureText = gesture + " detected";
            gestureInfo.text = sGestureText;
        }

        // store gender
        UIManager.Instance.HideGestureGender(true);

        if(gesture == KinectGestures.Gestures.RaiseLeftHand)
        {
            Debug.Log("User " + userId + " is female");
            UserManager.Instance.setGender(userIndex, "female");
            UIManager.Instance.ShowFemaleGender();
            UIManager.Instance.ShowStickManDelay(2.0f);
        }
        else
        {
            Debug.Log("User " + userId + " is male");
            UserManager.Instance.setGender(userIndex, "male");
            UIManager.Instance.ShowMaleGender();
            UIManager.Instance.ShowStickManDelay(2.0f);
        }

        return true;
    }

    public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture,
                                  KinectInterop.JointType joint)
    {
        // the gestures are allowed for the primary user only
        if (userIndex != playerIndex)
            return false;

        if (progressDisplayed)
        {
            progressDisplayed = false;

            if (gestureInfo != null)
            {
                gestureInfo.text = String.Empty;
            }
        }

        return true;
    }

    void Update()
    {
        if (progressDisplayed && ((Time.realtimeSinceStartup - progressGestureTime) > 2f))
        {
            progressDisplayed = false;
            gestureInfo.text = String.Empty;

            Debug.Log("Forced progress to end.");
        }
    }

}
