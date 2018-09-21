using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiveRunwayManager : MonoBehaviour, IRunwayMode, KinectGestures.GestureListenerInterface
{
    public GameObject UserPrefab;
    public GameObject PosingScorePrefab;

    public GameObject liveRunwayContainer;
    public GameObject userContainer;
    public GameObject posingScoreContainer;

    public ClosetManager closetMgr;

    private List<long> userBuffer = new List<long>();
    private Dictionary<long, User> users = new Dictionary<long, User>();

    private bool isModeActive = false;

    private float gestureGenderShowLength = 3.0f;

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

    public void SetUp(int level = 0)
    {
        Debug.Log(string.Format("[LiveRunwayManager] SetUp:"));

        liveRunwayContainer.SetActive(true);

        CreateUsersFromBuffer();
        isModeActive = true;

        ClosetManager.Instance.OnEnterLiveMode();

        PoseMgr.Instance.StartPosing();
    }

    public void End()
    {
        Debug.Log(string.Format("[LiveRunwayManager] End:"));

        UIManager.Instance.HideAll();
        
        liveRunwayContainer.SetActive(false);

        isModeActive = false;
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

            ClosetManager.Instance.OnUserLost(userId);
            OutfitGameObjectsManager.Instance.OnUserLost(userId);

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

        GameObject posingScoreGO = Instantiate(PosingScorePrefab, posingScoreContainer.transform);
        UserScore userScore = posingScoreGO.GetComponent<UserScore>();

        user.initialize(userId, userScore);

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
