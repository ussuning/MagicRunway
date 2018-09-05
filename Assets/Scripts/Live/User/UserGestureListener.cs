using UnityEngine;
using System.Collections;
using System;

public class UserGestureListener : MonoBehaviour, KinectGestures.GestureListenerInterface
{
    public long uid;
    public int uindex;
    const int maxSlots = 12;
   
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
        // the gestures are allowed for the primary user only
        if (userIndex != uindex)
            return false;
        if (AppManager.Instance.getMode() == Mode.AUTO && gesture == KinectGestures.Gestures.RaiseRightHand)
        {
            UIManager.Instance.ClickStartMenu();
            return true;
        }

        if (AppManager.Instance.getMode() == Mode.LIVE)
        {
            if (gesture == KinectGestures.Gestures.RaiseLeftHand)
            {
                //    Debug.Log("Gesture Completed: " + gesture + " " + userIndex + " " + userId);
                // return if gender already set
                if (UserManager.Instance.getUserById(userId).getGender() != null)
                {
                    //  Debug.Log("Gender already set");
                    return false;
                }

                // store gender
                UIManager.Instance.HideGestureGender(true);

                //  Debug.Log("User " + userId + " is female");
                //  UserManager.Instance.addGenderIcon(userId, "female");
                UserManager.Instance.setGender(userId, "female");
                UIManager.Instance.ShowFemaleGender(userIndex);
                UIManager.Instance.ShowStickManDelay(10.0f);
                StartCoroutine(UserManager.Instance.renderOutfit(userId, UserManager.Instance.getUserById(userId).getInventorySlot()));
                KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.RaiseLeftHand);
            }
            else if (gesture == KinectGestures.Gestures.RaiseRightHand)
            {
                //   Debug.Log("Gesture Completed: " + gesture + " " + userIndex + " " + userId);
                // return if gender already set
                if (UserManager.Instance.getUserById(userId).getGender() != null)
                {
                    //  Debug.Log("Gender already set");
                    return false;
                }

                // store gender
                UIManager.Instance.HideGestureGender(true);

                // Debug.Log("User " + userId + " is male");
                // UserManager.Instance.addGenderIcon(userId, "male");
                UserManager.Instance.setGender(userId, "male");
                UIManager.Instance.ShowMaleGender(userIndex);
                UIManager.Instance.ShowStickManDelay(10.0f);
                StartCoroutine(UserManager.Instance.renderOutfit(userId, UserManager.Instance.getUserById(userId).getInventorySlot()));
                KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.RaiseRightHand);
            }
            else if (gesture == KinectGestures.Gestures.SwipeRight)
            {
                Debug.Log("Gesture Completed: " + gesture + " " + userIndex + " " + userId);
                KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.SwipeRight);
                int nextSlot = UserManager.Instance.getUserById(userId).getInventorySlot() + 1;
                if (nextSlot >= maxSlots)
                {
                    KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.SwipeRight);
                    return false;
                }
                else
                {
                    Destroy(UserManager.Instance.getUserById(userId).getOutfit());
                    StartCoroutine(UIManager.Instance.scrollInventory(userIndex, userId, "down"));
                    StartCoroutine(UserManager.Instance.renderOutfit(userId, nextSlot));
                    KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.SwipeRight);
                }
            }
            else if (gesture == KinectGestures.Gestures.SwipeLeft)
            {
                Debug.Log("Gesture Completed: " + gesture + " " + userIndex + " " + userId);
                KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.SwipeLeft);
                int nextSlot = UserManager.Instance.getUserById(userId).getInventorySlot() - 1;

                if (nextSlot < 1)
                {
                    KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.SwipeLeft);
                    return false;
                }
                else
                {
                    Destroy(UserManager.Instance.getUserById(userId).getOutfit());
                    StartCoroutine(UIManager.Instance.scrollInventory(userIndex, userId, "up"));
                    StartCoroutine(UserManager.Instance.renderOutfit(userId, nextSlot));
                    KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.SwipeLeft);
                }
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
