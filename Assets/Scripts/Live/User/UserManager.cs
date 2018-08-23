using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class UserManager : Singleton<UserManager>
{
    public GameObject kinectController;
    public GameObject userSkeletonPrefab;
    public GameObject userScorePrefab;
    public GameObject malePrefab;
    public GameObject femalePrefab;
    private Dictionary<long, User> userLookup = new Dictionary<long, User>();
    private Dictionary<long, PoseAgentSelector> poseAgents = new Dictionary<long, PoseAgentSelector>();
    private Dictionary<long, GameObject> userScoreBoxes = new Dictionary<long, GameObject>();
    private KinectManager kinectManager;

    public void Start()
    {
        kinectManager = KinectManager.Instance;
        UserEvents.OnNewUserDetectedCallback += UserManager_NewUserDetected;
        UserEvents.OnUserLostCallback += UserManager_UserLostDetected;
    }

    public List<long> getCurrentUserIds()
    {
        List<long> users = new List<long>();
        foreach (KeyValuePair<long, User> user in userLookup)
        {
            users.Add(user.Value.getUserId());
        }
        return users;
    }

    public User getUserById(long userId)
    {
        if (userLookup.ContainsKey(userId))
        {
            return userLookup[userId];
        }
        return new User(0, 0);
    }

    public GameObject getUserScoreBoxById(long userId)
    {
        if (userScoreBoxes.ContainsKey(userId))
            return userScoreBoxes[userId];
        return null;
    }

    // stub - program later
    public bool userExists(long uid)
    {
        if (! userLookup.ContainsKey(uid))
        {
            return false;
        }

        return true;
    }

    // move to user controller
    // New User detected, instantiate user skeleton and attach gesture listener
    void UserManager_NewUserDetected(long userId, int userIndex)
    {
        //Debug.Log("UserManager: New User Event Callback invoked.");

        // add user if not already exists, sometimes same user is detected as new user multiple times
        if(userLookup.ContainsKey(userId)) 
        {
            return;
        }
      
        // listening for these gestures for this user  
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.RaiseLeftHand);   //female
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.RaiseRightHand);  // male
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.SwipeLeft);       // move icons left
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.SwipeRight);      // move icons right
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.SwipeUp);       // display menu
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.SwipeDown);       // hide menu

        StartCoroutine(addUser(userId, userIndex));

        // show gender icons 
        Mode mode = AppManager.Instance.getMode();
        if (mode == Mode.LIVE)
        {
            UIManager.Instance.HideStickMan(true);
            UIManager.Instance.ShowGestureGender(10.0f);

            //wait 25 seconds, show stickman again to continue posing
            UIManager.Instance.ShowStickManDelay(11.0f);
        }
    }

    // move to user controller
    // User Lost detected, remove game object
    void UserManager_UserLostDetected(long userId, int userIndex)
    {
        // remove listener
        int count = removeUser(userId, userIndex);

        // hide start menu button if no users detected
        // int count = KinectManager.Instance.GetUsersCount();    // not dependable, sometimes kinect returns 1 when no users present
        if (count == 0)
        {
            Mode mode = AppManager.Instance.getMode();
            if (mode == Mode.LIVE)
            {
              //  Debug.Log("User count = 0 cries. Going back to Auto Runway");
                // AppManager.Instance.LiveToAuto();
                StartCoroutine(AppManager.Instance.ShouldRestartScene());
            }
            if (mode == Mode.AUTO)
            {
                UIManager.Instance.HideStartMenu(true);
            }
        }
    }

    public void setGender(long userId, string gender)
    {
        if (userLookup.ContainsKey(userId))
        {
            userLookup[userId].setGender(gender);
        }
    }

    public Dictionary<long, User> getCurrentUsers()
    {
        return userLookup;
    }

    // need to clean up later - move to UI 
    public void addGenderIcon(long userId, string gender)
    {
        string userContainerName = "User_" + userId;
        GameObject userContainer = GameObject.Find(userContainerName);
        GameObject genderGO = userContainer.transform.Find("Gender").gameObject;

        if (gender == "female")
        {
            GameObject femaleGO = (GameObject)Instantiate(femalePrefab);
            femaleGO.name = "Female";
            femaleGO.transform.SetParent(genderGO.transform);
        }
        else
        { 
            GameObject maleGO = (GameObject)Instantiate(malePrefab);
            maleGO.name = "Male";
            maleGO.transform.SetParent(genderGO.transform);
        }
    }

    protected int removeUser(long userId, int userIndex)
    {
        // remove listener
        removeGestureListener(userId, userIndex);

        // remove user data from scene
        removeUserModel(userId);

        // remove user score box (UI)
        removeUserScoreUI(userId);

        // destory user data
        userLookup.Remove(userId);

        return userLookup.Count;
    }

    protected void setup(long userId, int userIndex)
    {
        GameObject Users = GameObject.Find("Users");
        string userContainerName = "User_" + userId ;

        // create gameobject for this user
        GameObject userContainer = new GameObject();
        userContainer.name = userContainerName;

        // instantiate user instance and add to user lookup
        User user = userContainer.AddComponent<User>();
        user.initialize(userId, userIndex);
        userLookup[userId] = user;

        userContainer.transform.SetParent(Users.transform);

        // add gender gameobject for this user
        GameObject genderContainer = new GameObject();
        genderContainer.name = "Gender";
        genderContainer.transform.SetParent(userContainer.transform);
    }

    // need to clean up later - move to UI 
    protected void renderUserModel(long userId)
    {
        string userContainerName = "User_" + userId;
        GameObject userContainer = GameObject.Find(userContainerName);

        // instantiate prefab for new user - move to user
        GameObject userSkeletonGO = (GameObject)Instantiate(userSkeletonPrefab);
        userSkeletonGO.name = "User Skeleton_" + userId;
        userSkeletonGO.transform.SetParent(userContainer.transform);
        userLookup[userId].setUserSkeletonGO(userSkeletonGO);

    }

    // need to clean up later - move to UI
    protected void removeUserModel(long userId)
    {
        string userContainerName = "User_" + userId;
        GameObject userContainer = GameObject.Find(userContainerName);
        Destroy(userContainer);
    }

    // need to clean up later - move to UI
  /*  public void showOutfitMenu(long userId)
    {
        string outfitMenuName = "OutFitMenu_" + userId;
        GameObject outfitMenuGO = GameObject.Find(outfitMenuName);
        outfitMenuGO.SetActive(true);
        userLookup[userId].setOutfitMenuStatus(true);
    }

    // need to clean up later - move to UI,  maybe hide outfit menu when user hits a pose 
    public void hideOutfitMenu(long userId)
    {
        string outfitMenuName = "OutFitMenu_" + userId;
        GameObject outfitMenuGO = GameObject.Find(outfitMenuName);
        outfitMenuGO.SetActive(false);
        userLookup[userId].setOutfitMenuStatus(false);
    }
   */

    public void renderOutfit(long userId, int slot)
    {
      //  Debug.Log("User Manager slot = " + slot + " " + userId + " index = " + userLookup[userId].getUserIndex());
        string inventoryMenuName = "InventoryMenu_" +  (userLookup[userId].getUserIndex());
      //  Debug.Log("Inventory Menu Name = " + inventoryMenuName);
        
        GameObject inventoryMenuGO = GameObject.Find(inventoryMenuName);
        GameObject slotGO = inventoryMenuGO.transform.Find("slot_" + slot).gameObject;

     //   Debug.Log("Slot = " + slot);
        PrefabReference prefabRef = slotGO.GetComponent<PrefabReference>();

     //   Debug.Log("Prefab = " + prefabRef.name);
        prefabRef.Load(userLookup[userId].getUserIndex());
        userLookup[userId].setOutfit(prefabRef.instance);

        //GameObject outfitGO = UserManager.Instance.getUserById(userId).getOutfit();
        //Image img = slotGO.GetComponent<Image>();
        //img.color = new Color32(255, 255, 255, 100);
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
            //Debug.Log("addgesturelistener  " + userId + " " + userIndex);

            if (component.uindex == userIndex)
            {
                component.Initialize(userId, userIndex);
                return true;
            }
        }

        return false;
    }

    protected bool removeGestureListener(long userId, int userIndex)
    {
        foreach (var component in kinectController.GetComponents<UserGestureListener>())
        {
            if (component.uindex == userIndex)
            {
                component.uid = 0;
                return true;
            }
        }
        return false;
    }

    protected void addPoseDetection(long uid)
    {
        PoseAgentSelector agentSelector = userLookup[uid].getUserSkeletonGO().GetComponent<PoseAgentSelector>();
        poseAgents.Add(uid, agentSelector);
        if (AppManager.Instance.getMode() == Mode.LIVE)
            agentSelector.Init(uid);
    }

    public void initPoseDetection()
    {
        foreach(long uid in poseAgents.Keys)
        {
            poseAgents[uid].Init(uid);
        }
    }

    protected void updateGenderIconPos(long userId, Vector3 pos)
    {
        // move icon to pos
        string userContainerName = "User_" + userId;
        GameObject userContainer = GameObject.Find(userContainerName);

        if (userLookup.ContainsKey(userId))
        {
            if (userLookup[userId].getGender() == "female")
            {

                GameObject gender = userContainer.transform.Find("Gender/Female").gameObject;
                gender.transform.position = pos;
            }
            else if (userLookup[userId].getGender() == "male")
            {
                GameObject gender = userContainer.transform.Find("Gender/Male").gameObject;
                gender.transform.position = pos;
            }
        }
    }

    protected void updateOufitMenuPos(long userId, int userIndex, Vector3 pos)
    {   
     /*   if(userIndex == 0)
        {
            outfitMenuGO.transform.position = pos;
        }
        else if(userIndex == 1)
        {
            outfitMenuGO.transform.position = pos;
        }*/
    }

    IEnumerator joinLivePrompt()
    {
        //Debug.Log("UserManager COROUTINE- Show Start Menu!!");
        yield return new WaitForSeconds(5);
        UIManager.Instance.ShowStartMenu(true);
    }

    public IEnumerator renderOutfitsforAllUsers()
    {
        yield return new WaitForSeconds(4);

        Dictionary<long, User> currentUsers = getCurrentUsers();
        foreach (KeyValuePair<long, User> user in currentUsers)
        {
            renderOutfit(user.Value.getUserId(), user.Value.getInventorySlot());
        }
    }

    IEnumerator addUser(long userId, int userIndex)
    {
       // Debug.Log("UserManager COROUTINE- Setup");
        // add user to scene
        setup(userId, userIndex);

        // instantiate prefab for new user - turn off, causing runtime issues
        renderUserModel(userId);

        // instantiate user pose score prefab
        addUserScoreUI(userId);

        // add listener and assign for user 
        addGestureListener(userId, userIndex);

        // pose detection setup
        addPoseDetection(userId);

        // show start menu button to transition into Live mode
        if (AppManager.Instance.getMode() == Mode.AUTO)
        {
            StartCoroutine(joinLivePrompt());
        }
        else
        {
            renderOutfit(userId, 1);   // remove hard code later
        }

        yield return null;
    }

    public IEnumerator getNumberofUsers(Action<int> callback)
    {
        callback(userLookup.Count);
        yield return null;
    }

    void Update()
    {
        // display gender icon next to each user on every tick
      /*  if (AppManager.Instance.getMode() == Mode.LIVE)
        {
            foreach (KeyValuePair<long, User> user in userLookup)
            {
                // render gender icon
                updateGenderIconPos(user.Value.getUserId(), user.Value.getGenderIconPosition());
            }
        }*/
    }
}
