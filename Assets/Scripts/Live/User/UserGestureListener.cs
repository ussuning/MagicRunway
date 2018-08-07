using UnityEngine;
using System.Collections;
using System;

public class UserGestureListener : MonoBehaviour, KinectGestures.GestureListenerInterface
{
    public long uid;
    public int uindex;
   
    // singleton instance of the class
    private static UserGestureListener instance = null;

    public static UserGestureListener Instance
    {
        get
        {
            return instance;
        }
    }
  
    public void Initialize(long userId, int userIndex)
    {
        Debug.Log("User Gesture Listener initialized");
        uindex = userIndex;
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

        if(AppManager.Instance.getMode() == Mode.AUTO)
        {
            return false;
        }
    

        Debug.Log("Gesture Completed: " + gesture + " " + userIndex + " " + userId);

        // the gestures are allowed for the primary user only
        if (userIndex != uindex)
            return false;

        // return if gender already set
        if(UserManager.Instance.getUserById(userId).getGender() != null)
        {
            Debug.Log("Gender already set");
            return false;
        }

        // store gender
        UIManager.Instance.HideGestureGender(true);

        if (gesture == KinectGestures.Gestures.RaiseLeftHand)
        {
            Debug.Log("User " + userId + " is female");
            UserManager.Instance.addGenderIcon(userId, "female");
            UserManager.Instance.setGender(userId, "female");
            UIManager.Instance.ShowFemaleGender();
            UIManager.Instance.ShowStickManDelay(2.0f);

            // show outfit menu for females
            //UserManager.Instance.addOutfitMenu(userId, "female");   //testing

        }
        else if (gesture == KinectGestures.Gestures.RaiseRightHand)
        {
            Debug.Log("User " + userId + " is male");
            UserManager.Instance.addGenderIcon(userId, "male");
            UserManager.Instance.setGender(userId, "male");
            UIManager.Instance.ShowMaleGender();
            UIManager.Instance.ShowStickManDelay(2.0f);

            // show outfit menu for males
            //UserManager.Instance.addOutfitMenu(userId, "male");   //testing
        }
        else if (gesture == KinectGestures.Gestures.SwipeLeft)
        {
            if(UserManager.Instance.getUserById(userId).isOutfitMenuDisplayed() == true)
            {
                // shift menu left
                // maybe hide outfit menu when user hits a pose
            }
        }
        else if (gesture == KinectGestures.Gestures.SwipeRight)
        {
            if (UserManager.Instance.getUserById(userId).isOutfitMenuDisplayed() == true)
            {
                // shift menu right
                // maybe hide outfit menu when user hits a pose
            }
        }
        else if (gesture == KinectGestures.Gestures.SwipeUp)
        {
            if (UserManager.Instance.getUserById(userId).isOutfitMenuDisplayed() == false 
                && UserManager.Instance.getUserById(userId).getGender() != null)
            {
                UserManager.Instance.showOutfitMenu(userId);
            }
        }
        else if (gesture == KinectGestures.Gestures.SwipeDown)
        {
            if (UserManager.Instance.getUserById(userId).isOutfitMenuDisplayed() == true
                 && UserManager.Instance.getUserById(userId).getGender() != null)
            {
                UserManager.Instance.hideOutfitMenu(userId);
            }
        }



        return true;
    }

    public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture,
                                  KinectInterop.JointType joint)
    {
        // the gestures are allowed for the primary user only
        if (userIndex != uindex)
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
