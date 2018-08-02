using UnityEngine;
using System.Collections;
using System;

public class UserGestureListener : MonoBehaviour, KinectGestures.GestureListenerInterface
{
    public int playerIndex = 0;
    public long uid;
   
    // singleton instance of the class
    private static UserGestureListener instance = null;

    public static UserGestureListener Instance
    {
        get
        {
            return instance;
        }
    }
  
    public void initialize(long userId, int userIndex)
    {
        Debug.Log("User Gesture Listener initialized");
        playerIndex = userIndex;
        uid = userId;
    }

    public void UserDetected(long userId, int userIndex)
    {
        // do nothing
    }

    public void UserLost(long userId, int userIndex)
    {
        // do nothing
    }
    
    
    public void GestureInProgress(long userId, int userIndex, KinectGestures.Gestures gesture,
                                  float progress, KinectInterop.JointType joint, Vector3 screenPos)
    {
       // do nothing
    }

    public bool GestureCompleted(long userId, int userIndex, KinectGestures.Gestures gesture,
                                  KinectInterop.JointType joint, Vector3 screenPos)
    {

        Debug.Log("Gesture Completed: " + gesture + " " + userIndex + " " + userId);

        // the gestures are allowed for the primary user only
        if (userIndex != playerIndex)
            return false;

        // store gender
        UIManager.Instance.HideGestureGender(true);

        if(gesture == KinectGestures.Gestures.RaiseLeftHand)
        {
            Debug.Log("User " + userId + " is female");
            UserManager.Instance.addGenderIcon(userIndex, "female");
            UserManager.Instance.setGender(userIndex, "female");
            UIManager.Instance.ShowFemaleGender();
            UIManager.Instance.ShowStickManDelay(2.0f);
        }
        else
        {
            Debug.Log("User " + userId + " is male");
            UserManager.Instance.addGenderIcon(userIndex, "male");
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
        {
            return false;
        }

        return true;
    }

    void Awake()
    {
        instance = this;
    }

}
