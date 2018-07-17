using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserEvents : MonoBehaviour {

    // New User Event
    public delegate void OnNewUserDetected(long userId, int userIndex);
    public static event OnNewUserDetected OnNewUserDetectedCallback;

    public static void NewUserDetected(long userId, int userIndex)
    {
        if (OnNewUserDetectedCallback != null)
        {
            Debug.Log("UserEvents: New User, invoking callbacks");
            OnNewUserDetectedCallback(userId, userIndex);            
        }
    }

    // User Lost Event 
    public delegate void OnUserLost(long userId, int userIndex);
    public static event OnUserLost OnUserLostCallback;

    public static void UserLost(long userId, int userIndex)
    {
        if (OnUserLostCallback != null)
        {
            Debug.Log("UserEvents: UserLost, invoking callbacks");
            OnUserLostCallback(userId, userIndex);
        }
    }

    // User Gender Registration Complete
    public delegate void OnUserGenderComplete(long userId, int userIndex);
    public static event OnUserGenderComplete OnUserGenderCompleteCallback;

    public static void UserGenderComplete(long userId, int userIndex)
    {
        if (OnUserLostCallback != null)
        {
            Debug.Log("UserEvents: UserGenderComplete, invoking callbacks");
            OnUserGenderCompleteCallback(userId, userIndex);
        }
    }
}

