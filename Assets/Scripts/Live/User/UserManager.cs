using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserManager : Singleton<UserManager>
{
    public AppManager appManager;
    public GameObject userSkeletonPrefab;
    public GameObject userScorePrefab;
    public GameObject malePrefab;
    public GameObject femalePrefab;
    public GameObject kinectController;
    private Dictionary<int, User> userLookup = new Dictionary<int, User>();
    private Dictionary<long, GameObject> userScoreBoxes = new Dictionary<long, GameObject>();
    private KinectManager kinectManager;

    public void Start()
    {
        kinectManager = KinectManager.Instance;
        UserEvents.OnNewUserDetectedCallback += UserManager_NewUserDetected;
        UserEvents.OnUserLostCallback += UserManager_UserLostDetected;
    }

    public User getUserByIndex(int userIndex)
    {
        return userLookup[userIndex];
    }

    // stub - program later
    public bool userExists(long uid)
    {
        return false;
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

    public void setGender(int userIndex, string gender)
    {
        userLookup[userIndex].setGender(gender);
    }

    public Dictionary<int, User> getCurrentUsers()
    {
        return userLookup;
    }

    public void addGenderIcon(int userIndex, string gender)
    {
        int playerNumber = userIndex + 1;
        string userContainerName = "User" + playerNumber;
        GameObject userContainer = GameObject.Find(userContainerName);
        GameObject genderGO = userContainer.transform.Find("Gender").gameObject;

        if (gender == "female")
        {
            femalePrefab = (GameObject)Instantiate(Resources.Load("User/gender_female"));
            femalePrefab.name = "Female";
            femalePrefab.transform.SetParent(genderGO.transform);
        }
        else
        {
            malePrefab = (GameObject)Instantiate(Resources.Load("User/gender_male"));
            malePrefab.name = "Male";
            malePrefab.transform.SetParent(genderGO.transform);
        }
    }

    protected void addUser(long userId, int userIndex)
    {
        // add user to scene
        setup(userIndex);

        // instantiate prefab for new user 
        renderUserModel(userIndex);

        // instantiate user score box (UI)
        addUserScoreUI(userId);

        // add listener and assign for user 
        addGestureListener(userId, userIndex);

        // pose detection setup
        addPoseDetection(userId);

        // instantiate user instance and add to user lookup
        int playerNumber = userIndex + 1;
        string userContainerName = "User" + playerNumber;
        GameObject userContainer = GameObject.Find(userContainerName);
        User user = userContainer.AddComponent<User>();
        user.Initialize(userId, userIndex);
        userLookup[userIndex] = user;

        // show start menu button to transition into Live mode
        StartCoroutine(joinLivePrompt());
    }

    protected void removeUser(long userId, int userIndex)
    {
        // remove listener
        removeGestureListener(userIndex);

        // remove user data from scene
        removeUserModel(userIndex);

        // remove user score box (UI)
        removeUserScoreUI(userId);

        // destory user data
        userLookup.Remove(userIndex);
    }

    protected void setup(int userIndex)
    {
        int playerNumber = userIndex + 1;
        GameObject Users = GameObject.Find("Users");
        string userContainerName = "User" + playerNumber;

        // create gameobject for this user
        GameObject userContainer = new GameObject();
        userContainer.name = userContainerName;
        userContainer.transform.SetParent(Users.transform);

        // add gender gameobject for this user
        GameObject genderContainer = new GameObject();
        genderContainer.name = "Gender";
        genderContainer.transform.SetParent(userContainer.transform);
    }

    protected void renderUserModel(int userIndex)
    {
        int playerNumber = userIndex + 1;
        string userContainerName = "User" + playerNumber;
        GameObject userContainer = GameObject.Find(userContainerName);

        // instantiate prefab for new user - move to user
        userSkeletonPrefab = (GameObject)Instantiate(Resources.Load("UserSkeleton"));
        userSkeletonPrefab.name = "User Skeleton " + playerNumber;
        userSkeletonPrefab.transform.SetParent(userContainer.transform);
    }

    protected void removeUserModel(int userIndex)
    {
        int playerNumber = userIndex + 1;
        string userContainerName = "User" + playerNumber;
        GameObject userContainer = GameObject.Find(userContainerName);
        Destroy(userContainer);
    }

    protected void addUserScoreUI(long uid)
    {
        GameObject scoreContainer = GameObject.Find("PoseScoreContainer");

        // instantiate prefab for new user - move to user
        GameObject userScoreGO = (GameObject)Instantiate(userScorePrefab, scoreContainer.transform);
        userScoreGO.GetComponent<UserScore>().Init(uid);
        userScoreBoxes.Add(uid, userScoreGO);
    }

    protected void removeUserScoreUI(long uid)
    {
        if(userScoreBoxes.ContainsKey(uid))
        {
            GameObject userScoreGO = userScoreBoxes[uid];
            userScoreBoxes.Remove(uid);
            Destroy(userScoreGO);
        }
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

    protected void updateGenderIconPos(int userIndex, Vector3 pos)
    {
        // move icon to pos
        int playerNumber = userIndex + 1;
        string userContainerName = "User" + playerNumber;
        GameObject userContainer = GameObject.Find(userContainerName);

        if(userLookup[userIndex].getGender() == "female")
        {

            GameObject gender = userContainer.transform.Find("Gender/Female").gameObject;
            gender.transform.position = pos;
        }
        else if(userLookup[userIndex].getGender() == "male")
        {
            GameObject gender = userContainer.transform.Find("Gender/Male").gameObject;
            gender.transform.position = pos;
        }
    }

    IEnumerator joinLivePrompt()
    {
        yield return new WaitForSeconds(5);
        UIManager.Instance.ShowStartMenu(true);
    }

    private void Update()
    {
        // display gender icon next to each user on every tick
        if (appManager.getMode() == Mode.LIVE)
        {
            foreach (KeyValuePair<int, User> user in userLookup)
            {
                // render gender icon
                updateGenderIconPos(user.Value.getUserIndex(), user.Value.getGenderIconPosition());
            }
        }
    }
}
