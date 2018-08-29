using UnityEngine;
using System.Collections;
using System;

public class UserGestureListener : MonoBehaviour, KinectGestures.GestureListenerInterface
{
    public long uid;
    public int uindex;
    const int maxSlots = 13;
   
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

        if(AppManager.Instance.getMode() == Mode.AUTO)
        {
            if(gesture == KinectGestures.Gestures.RaiseRightHand)
            {
                UIManager.Instance.ClickStartMenu();
            }

            return false;
        }
    
        // the gestures are allowed for the primary user only
        if (userIndex != uindex)
            return false;

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
            UserManager.Instance.renderOutfit(userId, UserManager.Instance.getUserById(userId).getInventorySlot());
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
            UserManager.Instance.renderOutfit(userId, UserManager.Instance.getUserById(userId).getInventorySlot());
            KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.RaiseRightHand);
        }
        else if (gesture == KinectGestures.Gestures.SwipeRight || gesture == KinectGestures.Gestures.SwipeDown)
        {
           // Debug.Log("Gesture Completed: " + gesture + " " + userIndex + " " + userId);
            int nextSlot = UserManager.Instance.getUserById(userId).getInventorySlot() + 1;
            if (nextSlot > maxSlots)
            {
                return false;
            }
            Destroy(UserManager.Instance.getUserById(userId).getOutfit());
            StartCoroutine(UIManager.Instance.scrollInventory(userIndex, userId, "down"));
            UserManager.Instance.renderOutfit(userId, nextSlot);
        }
        else if (gesture == KinectGestures.Gestures.SwipeLeft || gesture == KinectGestures.Gestures.SwipeUp)
        {
           // Debug.Log("Gesture Completed: " + gesture + " " + userIndex + " " + userId);
            int nextSlot = UserManager.Instance.getUserById(userId).getInventorySlot() - 1;
         
            if(nextSlot < 1)
            {
                return false;
            }
            Destroy(UserManager.Instance.getUserById(userId).getOutfit());
            StartCoroutine(UIManager.Instance.scrollInventory(userIndex, userId, "up"));
            UserManager.Instance.renderOutfit(userId, nextSlot);
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
