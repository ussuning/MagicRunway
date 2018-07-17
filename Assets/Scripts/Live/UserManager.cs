using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserManager : Singleton<UserManager>
{
    public GameObject userSkeletonPrefab;
    private GameObject userContainer;
    private string userContainerName;

    public void Start()
    {
        UserEvents.OnNewUserDetectedCallback += UserManager_NewUserDetected;
        UserEvents.OnUserLostCallback += UserManager_UserLostDetected;
    }

    // New User detected, instantiate user skeleton and attach gesture listener
    void UserManager_NewUserDetected(long userId, int userIndex)
    {
        Debug.Log("UserManager: New User Event Callback invoked.");

        // find "Live" GameObject
        int playerNumber = userIndex + 1;
        GameObject liveSection = GameObject.Find("Users");
        userContainer = new GameObject();
        userContainerName = "User" + playerNumber;
        userContainer.name = userContainerName;
        userContainer.transform.SetParent(liveSection.transform);

        // instantiate prefab for new user
        userSkeletonPrefab = (GameObject)Instantiate(Resources.Load("UserSkeleton"));
        userSkeletonPrefab.name = "User Skeleton " + playerNumber;
        userSkeletonPrefab.transform.SetParent(userContainer.transform);

        // get listener and assign for user
        UserGestureListener userGestureListener = userSkeletonPrefab.GetComponent<UserGestureListener>();
        userGestureListener.initialize(userId, userIndex);
    }

    // User Lost detected, remove game object
    void UserManager_UserLostDetected(long userId, int userIndex)
    {
        Debug.Log("UserManager: User Lost Detected.");

        // destroy user data
        Destroy(userContainer);
    }

    // set gender for ML pose s in Live
    void setGender(int gender)
    {


    }


}
