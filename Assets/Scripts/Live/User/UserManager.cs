using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserManager : Singleton<UserManager>
{
    public AppManager appManager;
    public GameObject userSkeletonPrefab;
    public GameObject kinectController;
    private Dictionary<int, User> userLookup = new Dictionary<int, User>();
    private GameObject userContainer;
    private string userContainerName;
    private KinectManager kinectManager;

    public void Start()
    {
        kinectManager = KinectManager.Instance;
        UserEvents.OnNewUserDetectedCallback += UserManager_NewUserDetected;
        UserEvents.OnUserLostCallback += UserManager_UserLostDetected;
    }

    // New User detected, instantiate user skeleton and attach gesture listener
    void UserManager_NewUserDetected(long userId, int userIndex)
    {
        Debug.Log("UserManager: New User Event Callback invoked.");

        // add user if not already exists, sometimes same user is detected as new user multiple times
        foreach(KeyValuePair<int, User> user in userLookup)
        {
            if(user.Value.getUserId() == userId)
            {
                return;
            }
        }

        // listening for these gestures for this user  
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.RaiseLeftHand);
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.RaiseRightHand);

        addUser(userId, userIndex);

        // show gender icons 
        Mode mode = appManager.getMode();
        if (mode == Mode.LIVE)
        {
            UIManager.Instance.HideStickMan(true);
            UIManager.Instance.ShowGestureGender(10.0f);

            //wait 25 seconds, show stickman again to continue posing
            UIManager.Instance.ShowStickManDelay(11.0f);
        }
    }

    // User Lost detected, remove game object
    void UserManager_UserLostDetected(long userId, int userIndex)
    {
        Debug.Log("UserManager: User Lost Detected.");

        // remove listener
        removeUser(userId, userIndex);

        // hide start menu button if no users detected
        int count = KinectManager.Instance.GetUsersCount();
        if (count == 0)
        {
            Mode mode = appManager.getMode();
            if (mode == Mode.LIVE)
            {
                Debug.Log("User count = 0 cries. Going back to Auto Runway");
                appManager.LiveToAuto();
            }
            if (mode == Mode.AUTO)
            {
                UIManager.Instance.HideStartMenu(true);
            }
        }
    }

    public bool userExists(long uid)
    {
        return false;
    }

    public User getUserByIndex(int userIndex)
    {
        return userLookup[userIndex];
    }

    public void setGender(int userIndex, string gender)
    {
        userLookup[userIndex].setGender(gender);
    }

    protected void addUser(long userId, int userIndex)
    {
        // add user to scene
        setup(userIndex);

        // instantiate prefab for new user 
        renderUserModel(userIndex);

        // add listener and assign for user 
        addGestureListener(userId, userIndex);

        // pose detection setup
        addPoseDetection(userId);

        // instantiate user instance and add to user lookup
        userLookup[userIndex] = new User(userId, userIndex);

        // show start menu button to transition into Live mode
        StartCoroutine(joinLivePrompt());
    }

    protected void removeUser(long userId, int userIndex)
    {
        // remove listener
        removeGestureListener(userIndex);

        // remove user data from scene
        removeUserModel(userIndex);

        // destory user data
        userLookup.Remove(userIndex);
    }

    protected void setup(int userIndex)
    {
        int playerNumber = userIndex + 1;
        GameObject Users = GameObject.Find("Users");
        userContainer = new GameObject();
        userContainerName = "User" + playerNumber;
        userContainer.name = userContainerName;
        userContainer.transform.SetParent(Users.transform);
    }

    protected void renderUserModel(int userIndex)
    {
        int playerNumber = userIndex + 1;

        // instantiate prefab for new user - move to user
        userSkeletonPrefab = (GameObject)Instantiate(Resources.Load("UserSkeleton"));
        userSkeletonPrefab.name = "User Skeleton " + playerNumber;
        userSkeletonPrefab.transform.SetParent(userContainer.transform);
    }

    protected void removeUserModel(int userIndex)
    {
        int playerNumber = userIndex + 1;
        userContainerName = "User" + playerNumber;
        userContainer = GameObject.Find(userContainerName);
        Destroy(userContainer);
    }

    protected bool addGestureListener(long userId, int userIndex )
    {
        foreach (var component in kinectController.GetComponents<UserGestureListener>())
        {
            if (component.playerIndex == userIndex)
            {
                component.initialize(userId, userIndex);
                return true;
            }
        }

        return false;
    }

    protected bool removeGestureListener(int userIndex)
    {
        foreach (var component in kinectController.GetComponents<UserGestureListener>())
        {
            if (component.playerIndex == userIndex)
            {
                component.uid = 0;
                return true;
            }
        }
        return false;
    }

    protected void addPoseDetection(long uid)
    {
        PoseAgentSelector agentSelector = userSkeletonPrefab.GetComponent<PoseAgentSelector>();
        agentSelector.Init(uid);
    }

    IEnumerator joinLivePrompt()
    {
        yield return new WaitForSeconds(5);
        UIManager.Instance.ShowStartMenu(true);
    }
}
