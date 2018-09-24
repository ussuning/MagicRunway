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

    private List<int> userBuffer = new List<int>();
    private Dictionary<int, User> users = new Dictionary<int, User>();

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

        PoseMgr.Instance.StopPosing();

        isModeActive = false;
    }

    //GestureListenerInterface

    public void UserDetected(long userId, int userIndex)
    {
        if(isModeActive)
        {
            if (users.ContainsKey(userIndex))
                return;

            User user = CreateUser(userIndex);
            KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.RaiseLeftHand);
            KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.RaiseRightHand);
            Debug.Log(string.Format("[LiveRunwayManager] UserDetected: User {0} created", userId));
        }
        else
        {
            if(userBuffer.Contains(userIndex))
                return;

            userBuffer.Add(userIndex);
        }    
    }

    public void UserLost(long userId, int userIndex)
    {
        if(isModeActive)
        {
            if (!users.ContainsKey(userIndex))
                return;

            DeleteUser(userIndex);
            KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.RaiseLeftHand);
            KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.RaiseRightHand);
            Debug.Log(string.Format("[LiveRunwayManager] UserLost: User {0} is removed from the Dictionary users", userId));

            ClosetManager.Instance.OnUserLost(userIndex);
            OutfitGameObjectsManager.Instance.OnUserLost(userIndex);

            if (users.Count <= 0)
                AppManager.Instance.TransitionToAuto();
        }
        else
        {
            if (!userBuffer.Contains(userIndex))
                return;

            userBuffer.Remove(userIndex);
        }
    }

    public void GestureInProgress(long userId, int userIndex, KinectGestures.Gestures gesture, float progress, KinectInterop.JointType joint, Vector3 screenPos)
    {  
    }

    public bool GestureCompleted(long userId, int userIndex, KinectGestures.Gestures gesture, KinectInterop.JointType joint, Vector3 screenPos)
    {
        if (!isModeActive)
            return false;

        if (users.ContainsKey(userIndex))
        {
            User user = users[userIndex];
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
        foreach(int userIdx in userBuffer)
        {
            long userId = userBuffer[userIdx];
            CreateUser(userIdx);
            KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.RaiseLeftHand);
            KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.RaiseRightHand);
            Debug.Log(string.Format("[LiveRunwayManager] CreateUsersFromBuffer: User {0} created", userId));
        }
        userBuffer.Clear();
    }

    User CreateUser(int userIdx)
    {
        GameObject userGO = Instantiate(UserPrefab, userContainer.transform);
        userGO.name = string.Format("User_{0}", KinectManager.Instance.GetUserIdByIndex(userIdx));

        User user = userGO.GetComponent<User>();
        if (user == null)
            user = userGO.AddComponent<User>();

        GameObject posingScoreGO = Instantiate(PosingScorePrefab, posingScoreContainer.transform);
        UserScore userScore = posingScoreGO.GetComponent<UserScore>();

        user.initialize(userIdx, userScore);

        users.Add(userIdx, user);

        return user;
    }

    void DeleteUser(int userIdx)
    {
        User user = users[userIdx];
        users.Remove(userIdx);
        Destroy(user.gameObject);
    }
}
