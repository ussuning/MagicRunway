using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserManager : Singleton<UserManager>
{
    public GameObject userSkeletonPrefab;
    public AppManager appManager;
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

        // find "Users" GameObject
        int playerNumber = userIndex + 1;
        GameObject Users = GameObject.Find("Users");
        userContainer = new GameObject();
        userContainerName = "User" + playerNumber;
        userContainer.name = userContainerName;
        userContainer.transform.SetParent(Users.transform);

        // instantiate prefab for new user
        userSkeletonPrefab = (GameObject)Instantiate(Resources.Load("UserSkeleton"));
        userSkeletonPrefab.name = "User Skeleton " + playerNumber;
        userSkeletonPrefab.transform.SetParent(userContainer.transform);

        // get listener and assign for user
        UserGestureListener userGestureListener = userSkeletonPrefab.GetComponent<UserGestureListener>();
        userGestureListener.initialize(userId, userIndex);

        // get pose agent and attach the brain
        PoseRecognizingAgent poseAgent = userSkeletonPrefab.GetComponent<PoseRecognizingAgent>();
        poseAgent.Init(userId);
      //  GameObject brainGO = GameObject.Find("Brain");
      //  Brain brain = brainGO.GetComponent<Brain>();
      //  poseAgent.GiveBrain(brain);

        // show start menu button to transition into Live mode
        UIManager.Instance.ShowStartMenu(true);
        UIManager.Instance.ShowHandCursor();
    }

    // User Lost detected, remove game object
    void UserManager_UserLostDetected(long userId, int userIndex)
    {
        Debug.Log("UserManager: User Lost Detected.");

        // destroy user data
        int playerNumber = userIndex + 1;
        userContainerName = "User" + playerNumber;
        userContainer = GameObject.Find(userContainerName);
        Destroy(userContainer);

        int count = KinectManager.Instance.GetUsersCount();

        Debug.Log("count = " + count);

        // hide start menu button if no users detected
        if (KinectManager.Instance.GetUsersCount() <= 1)
        {
            Debug.Log("Where everyone go!? User count = 0 cries.");
            appManager.LiveToAuto();
        }
    }

    // set gender for ML pose s in Live
    void setGender(int gender)
    {


    }


}
