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

    public int NumberOfPlayers = 2;
    public float UserReconnectionTime = 3f;

    private List<int> userBuffer = new List<int>();
    private List<int> disconnectedUserBuffer = new List<int>();
    private Dictionary<int, User> users = new Dictionary<int, User>();
    private int [] usersSortedByDistance
    {
        get
        {
            int[] uIdx = new int[users.Count];
            int counter = 0;
            foreach(int i in users.Keys)
            {
                uIdx[counter] = i;
                counter++;
            }

            SortUsersByDistance(ref uIdx, 0);

            return uIdx;
        }
    }
    private int NumActivatedUsers
    {
        get
        {
            int aUsers = 0;
            foreach(User user in users.Values)
            {
                if (user.IsActivated)
                    aUsers++;
            }
            return aUsers;
        }
    }

    private bool isModeActive = false;

    KinectManager manager;

    void Awake()
    {
        liveRunwayContainer.SetActive(false);
    }

    void Update()
    {
        if (!isModeActive)
            return;

        int[] userIdx = usersSortedByDistance;

        for (int i = 0; i < userIdx.Length; i++)
        {
            int uIdx = userIdx[i];
            User user_i = users[uIdx];
            if (i < NumberOfPlayers)
            {
                if (!user_i.IsActivated)
                {
                    user_i.IsReadyToBeActivated = true;
                }
            } 
            else
            {
                if(user_i.IsActivated)
                {
                    user_i.deactivate();
                    ClosetManager.Instance.OnUserLost(uIdx);
                    OutfitGameObjectsManager.Instance.OnUserLost(uIdx);
                }
                else
                {
                    user_i.IsReadyToBeActivated = false;
                }
            }
        }
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
        Debug.Log(string.Format("[LiveRunwayManager] SetUp: level = {0}", level));

        if (!manager)
            manager = KinectManager.Instance;
        manager.maxTrackedUsers = 6;

        UIManager.Instance.HideCollectionTitle(false);
        liveRunwayContainer.SetActive(true);

        CreateUsersFromBuffer();

        ClosetManager.Instance.OnEnterLiveMode();
        //PoseMgr.Instance.StartPosing();

        isModeActive = true;
    }

    public void End()
    {
        Debug.Log(string.Format("[LiveRunwayManager] End:"));

        UIManager.Instance.HideAll();
        PoseMatchingManager.Instance.ClearFX();
        //PoseMgr.Instance.StopPosing();

        liveRunwayContainer.SetActive(false);

        isModeActive = false;
    }

    //GestureListenerInterface

    public void UserDetected(long userId, int userIndex)
    {
        if(isModeActive)
        {
            if (disconnectedUserBuffer.Contains(userIndex)) //Reconnection
            {
                Debug.Log(string.Format("[LiveRunwayManager] UserDetected: User {0} RECONNECTED, userID: {1}", userIndex, userId));
                disconnectedUserBuffer.Remove(userIndex);
                OutfitGameObjectsManager.Instance.ShowUserOutfit(userIndex);
            }

            KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.RaiseLeftHand);
            KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.RaiseRightHand);

            if (users.ContainsKey(userIndex))
                return;

            User user = CreateUser(userIndex);
            Debug.Log(string.Format("[LiveRunwayManager] UserDetected: User {0}: {1} created", userIndex, userId));
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
            if (!disconnectedUserBuffer.Contains(userIndex))
            {
                disconnectedUserBuffer.Add(userIndex);
                OutfitGameObjectsManager.Instance.HideUserOutfit(userIndex);
            }

            KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.RaiseLeftHand);
            KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.RaiseRightHand);

            StartCoroutine(DisconnectingUser(userIndex, userId));
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
        if (!isModeActive)
            return;

        Closet closet = ClosetManager.Instance.GetUserCloset(userIndex);
        if (closet)
        {
            closet.activateIcon.SetProgressValue(progress);
        }
    }

    public bool GestureCompleted(long userId, int userIndex, KinectGestures.Gestures gesture, KinectInterop.JointType joint, Vector3 screenPos)
    {
        if (!isModeActive)
            return false;

        if (users.ContainsKey(userIndex))
        {
            User user = users[userIndex];
            //if (!user.IsActivated)
            //{
            //    if (NumActivatedUsers >= NumberOfPlayers)
            //        return false;

            //    user.activate();

            //    UpdateLiveStatus(NumActivatedUsers);
            //}

            if (userIndex == usersSortedByDistance[0] || userIndex == usersSortedByDistance[1])
            {
                if (!user.IsActivated)
                {
                    user.activate();
                    UpdateLiveStatus(NumActivatedUsers);
                }

                switch (gesture)
                {
                    case KinectGestures.Gestures.RaiseLeftHand:
                        user.UserGender = User.Gender.Female;
                        Debug.Log(string.Format("[LiveRunwayManager] GestureCompleted: User {0} is Female", userId));
                        //KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.RaiseLeftHand);
                        //KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.RaiseRightHand);
                        break;
                    case KinectGestures.Gestures.RaiseRightHand:
                        user.UserGender = User.Gender.Male;
                        Debug.Log(string.Format("[LiveRunwayManager] GestureCompleted: User {0} is Male", userId));
                        //KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.RaiseLeftHand);
                        //KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.RaiseRightHand);
                        break;
                }
                ClosetManager.Instance.OnUserGenderSelected(userIndex, user.UserGender);
            }
            
            return true;
        }

        return false;
    }

    public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture, KinectInterop.JointType joint)
    {
        if (!isModeActive)
            return false;

        Closet closet = ClosetManager.Instance.GetUserCloset(userIndex);
        if (!closet)
            return false;

        closet.activateIcon.SetProgressValue(0f);
        return true;          
    }

    void CreateUsersFromBuffer()
    {
        foreach(int userIdx in userBuffer)
        {
            long userId = userBuffer[userIdx];
            User user = CreateUser(userIdx);
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

        user.initialize(userIdx, userScore, false);

        users.Add(userIdx, user);

        return user;
    }

    void DeleteUser(int userIdx)
    {
        User user = users[userIdx];
        users.Remove(userIdx);
        Destroy(user.gameObject);
    } 

    IEnumerator DisconnectingUser(int userIndex, long userId)
    {
        Debug.Log(string.Format("[LiveRunwayManager] DisconnectingUser: waiting for user {0} to reconnect", userIndex));

        yield return new WaitForSeconds(UserReconnectionTime);

        if(disconnectedUserBuffer.Contains(userIndex))
        {
            disconnectedUserBuffer.Remove(userIndex);

            if (users.ContainsKey(userIndex))
            {
                DeleteUser(userIndex);
                Debug.Log(string.Format("[LiveRunwayManager] DisconnectingUser: User {0}: {1} is Disconnected, removed from the Dictionary users", userIndex, userId));

                ClosetManager.Instance.OnUserLost(userIndex);
                OutfitGameObjectsManager.Instance.OnUserLost(userIndex);
                PoseMatchingManager.Instance.ClearFX(userIndex);

                UpdateLiveStatus(NumActivatedUsers);

                if (users.Count <= 0)
                    AppManager.Instance.TransitionToAuto();
            }
        }
    }

    private void SortUsersByDistance(ref int[] allUsers, int idx)
    {
        for (int i = idx; i > 0; i--)
        {
            float curUserPosZ = manager.GetUserPosition(manager.GetUserIdByIndex(allUsers[i])).z;
            float preUserPosZ = manager.GetUserPosition(manager.GetUserIdByIndex(allUsers[i - 1])).z;
            if (curUserPosZ < preUserPosZ) //cur closer than pre
            {
                int curUser = allUsers[i];
                allUsers[i] = allUsers[i - 1];
                allUsers[i - 1] = curUser;
                SortUsersByDistance(ref allUsers, idx);
            }
        }

        if (idx + 1 < allUsers.Length)
            SortUsersByDistance(ref allUsers, idx + 1);
    }

    private void UpdateLiveStatus(int numActivatedUsers)
    {
        //foreach (User u in users.Values)
        //{
        //    if (!u.IsActivated)
        //        u.IsReadyToBeActivated = (numActivatedUsers < NumberOfPlayers);
        //}

        PoseMgr poseMgr = PoseMgr.Instance;
        if (!poseMgr.IsPosing && numActivatedUsers >= 1)
            poseMgr.StartPosing();
        else if (poseMgr.IsPosing && numActivatedUsers == 0)
            poseMgr.StopPosing();
    }   
}

