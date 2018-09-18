using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiveRunwayManager : MonoBehaviour, IRunwayMode, KinectGestures.GestureListenerInterface
{
    public GameObject UserPrefab;

    //public GameObject stickman;
    //public GameObject poseAcademy;
    public GameObject liveRunwayContainer;
    public GameObject userContainer;
    //public GameObject outfits;
    //public GameObject canvas;
    //public GameObject fittingRoom;
    //public GameObject userModel;

    //private Collection curCollection;
    //private Collection nextCollection;
    //private List<GameObject> models = new List<GameObject>();

    //private float collectionShowTime = 20.0f;
    //private float collectionWarningTime = 10.0f;
    //private int curCollectionIndex = 0;
    //private int totalCollections = 0;
    //private int totalOutfits = 0;

    private List<long> userBuffer = new List<long>();
    private Dictionary<long, User> users = new Dictionary<long, User>();

    private bool isModeActive = false;

    private float gestureGenderShowLength = 3.0f;

    //private bool isCollectionEnding = false;
    void Awake()
    {
        liveRunwayContainer.SetActive(false);
    }

    public Mode GetMode()
    {
        return Mode.LIVE;
    }

    public void Begin()
    {
        Debug.Log(string.Format("[LiveRunwayManager] Begin:"));
    }

    public void SetUp()
    {
        Debug.Log(string.Format("[LiveRunwayManager] SetUp:"));

        liveRunwayContainer.SetActive(true);
        //fittingRoom.SetActive(true);

        CreateUsersFromBuffer();
        isModeActive = true;

        UIManager.Instance.ShowGestureGender(10.0f);
        UIManager.Instance.ShowStickManDelay(11.0f);
    }

    public void End()
    {
        Debug.Log(string.Format("[LiveRunwayManager] End:"));

        UIManager.Instance.HideAll();
        
        //fittingRoom.SetActive(false);
        liveRunwayContainer.SetActive(false);

        isModeActive = false;
    }

    public void ShowGestureGender()
    {
        UIManager.Instance.ShowGestureGender(gestureGenderShowLength);
    }

    /*
    private void PrepareCollectionLiveModelPrefabs()
    {
        if (models.Count > 0)
        {
            ClearModels();
        }

        totalOutfits = 0;
        isCollectionEnding = false;

        curCollection = MRData.Instance.collections.collections[curCollectionIndex];

        totalOutfits = curCollection.outfits.Count;

        foreach (Outfit outfit in curCollection.outfits)
        {
            string path = GetPathForOutfitPrefab(outfit.prefab, outfit.sex);
            GameObject go = RunwayModelsPrefabManager.InstantiateGameObject(path, outfits.transform);
            //go.SetActive(false);
            models.Add(go);
        }

        int nextCollectionIndex = curCollectionIndex + 1;

        if (nextCollectionIndex == totalCollections)
        {
            nextCollectionIndex = 0;
        }

        nextCollection = MRData.Instance.collections.collections[nextCollectionIndex];
    }
    */
    /*
    private void PrepareNextCollection()
    {
        UIManager.Instance.HideCollection();

        curCollectionIndex++;

        if (curCollectionIndex == totalCollections)
        {
            curCollectionIndex = 0;
        }

        PrepareCollectionLiveModelPrefabs();
        PlayLiveRunway();
    }
    */

    public static string GetPathForOutfitPrefab(string prefabName, string gender)
    {
        string sex = (gender == "f") ? "Female" : "Male";
        string path = "RunwayModels/" + sex + "/" + prefabName;

        return path;
    }


    //GestureListenerInterface

    public void UserDetected(long userId, int userIndex)
    {
        if(isModeActive)
        {
            if (users.ContainsKey(userId))
            {
                Debug.Log(string.Format("[LiveRunwayManager] UserDetected: User {0} is already added in the Dictionary users", userId));
                return;
            }
            User user = CreateUser(userId);
            KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.RaiseLeftHand);
            KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.RaiseRightHand);
            Debug.Log(string.Format("[LiveRunwayManager] UserDetected: User {0} created", userId));
        }
        else
        {
            if(userBuffer.Contains(userId))
            {
                Debug.Log(string.Format("[LiveRunwayManager] UserDetected: User {0} is already added in the userBuffer", userId));
                return;
            }

            userBuffer.Add(userId);
        }    
    }

    public void UserLost(long userId, int userIndex)
    {
        if(isModeActive)
        {
            if (!users.ContainsKey(userId))
            {
                Debug.Log(string.Format("[LiveRunwayManager] UserLost: User {0} is not found in the Dictionary users", userId));
                return;
            }

            DeleteUser(userId);
            KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.RaiseLeftHand);
            KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.RaiseRightHand);
            Debug.Log(string.Format("[LiveRunwayManager] UserLost: User {0} is removed from the Dictionary users", userId));

            if (users.Count <= 0)
                AppManager.Instance.TransitionToAuto();
        }
        else
        {
            if (!userBuffer.Contains(userId))
            {
                Debug.Log(string.Format("[LiveRunwayManager] UserLost: User {0} is not found in the userBuffer", userId));
                return;
            }

            userBuffer.Remove(userId);
        }
    }

    public void GestureInProgress(long userId, int userIndex, KinectGestures.Gestures gesture, float progress, KinectInterop.JointType joint, Vector3 screenPos)
    {  
    }

    public bool GestureCompleted(long userId, int userIndex, KinectGestures.Gestures gesture, KinectInterop.JointType joint, Vector3 screenPos)
    {
        if (!isModeActive)
            return false;

        if (users.ContainsKey(userId))
        {
            User user = users[userId];
            if (user.UserGender == User.Gender.None)
            {
                switch (gesture)
                {
                    case KinectGestures.Gestures.RaiseLeftHand:
                        user.UserGender = User.Gender.Female;
                        Debug.Log(string.Format("[LiveRunwayManager] GestureCompleted: User {0} is Female", userId));
                        KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.RaiseLeftHand);
                        KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.RaiseRightHand);
                        break;
                    case KinectGestures.Gestures.RaiseRightHand:
                        user.UserGender = User.Gender.Male;
                        Debug.Log(string.Format("[LiveRunwayManager] GestureCompleted: User {0} is Male", userId));
                        KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.RaiseLeftHand);
                        KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.RaiseRightHand);
                        break;
                }
            }
            return true;
        }

        return false;
    }

    public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture, KinectInterop.JointType joint)
    {
        return true;
    }

    void CreateUsersFromBuffer()
    {
        foreach(long userId in userBuffer)
        {
            CreateUser(userId);
            KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.RaiseLeftHand);
            KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.RaiseRightHand);
            Debug.Log(string.Format("[LiveRunwayManager] CreateUsersFromBuffer: User {0} created", userId));
        }
        userBuffer.Clear();
    }

    User CreateUser(long userId)
    {
        GameObject userGO = Instantiate(UserPrefab, userContainer.transform);
        userGO.name = string.Format("User_{0}", userId);

        User user = userGO.GetComponent<User>();
        if (user == null)
            user = userGO.AddComponent<User>();
        user.initialize(userId);

        users.Add(userId, user);

        return user;
    }

    void DeleteUser(long userId)
    {
        User user = users[userId];
        users.Remove(userId);
        Destroy(user.gameObject);
    }
}
