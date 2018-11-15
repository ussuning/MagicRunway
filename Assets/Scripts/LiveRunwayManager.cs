using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiveRunwayManager : MonoBehaviour, IRunwayMode, KinectGestures.GestureListenerInterface
{
    public static int NUM_OF_ACTIVE_USERS = 2;

    public GameObject UserPrefab;
    public GameObject PosingScorePrefab;

    public GameObject liveRunwayContainer;
    public GameObject userContainer;
    public GameObject posingScoreContainer;

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

    KinectManager kinectMgr;
    ClosetManager closetMgr;
    OutfitGameObjectsManager outfitMgr;

    void Awake()
    {
        liveRunwayContainer.SetActive(false);
    }

    void Update()
    {
        if (!isModeActive)
            return;

        int[] userIdx = usersSortedByDistance;
        closetMgr.UpdateClosestUsers(userIdx);

        for (int i = 0; i < userIdx.Length; i++)
        {
            int uIdx = userIdx[i];
            User user_i = users[uIdx];
            if (i < NUM_OF_ACTIVE_USERS)
            {
                if (!user_i.IsActivated)
                {
                    user_i.IsReadyToBeActivated = true;
                }
            } 
            else
            {
                //if(user_i.IsActivated)
                //{
                //    user_i.deactivate();
                //    outfitMgr.HideUserOutfit(uIdx);
                //    closetMgr.OnUserLost(uIdx);
                //}
                //else
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

        UIManager.Instance.HideCollectionTitle(false);

        if (!kinectMgr)
            kinectMgr = KinectManager.Instance;

        kinectMgr.maxTrackedUsers = 6;

        liveRunwayContainer.SetActive(true);

        if (!closetMgr)
            closetMgr = ClosetManager.Instance;
        if (!outfitMgr)
            outfitMgr = OutfitGameObjectsManager.Instance;

        CreateUsersFromBuffer();

        closetMgr.OnEnterLiveMode();

        isModeActive = true;
    }

    public void End()
    {
        Debug.Log(string.Format("[LiveRunwayManager] End:"));

        UIManager.Instance.HideAll();
        PoseMatchingManager.Instance.ClearFX();

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
                outfitMgr.ShowUserOutfit(userIndex);
            }

            kinectMgr.DetectGesture(userId, KinectGestures.Gestures.Wave);

            if (users.ContainsKey(userIndex))
                return;

            User user = CreateUser(userIndex);

            UserFeatureRecognition.Instance.ClassifyUserFeatures(user);

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
                outfitMgr.HideUserOutfit(userIndex);
            }

            kinectMgr.DeleteGesture(userId, KinectGestures.Gestures.Wave);

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

        Closet closet = closetMgr.GetUserCloset(userIndex);
        if (closet)
        {
            switch (gesture)
            {
                case KinectGestures.Gestures.Wave:
                    closet.activateIcon.SetProgressValue(progress, closet.ClosetGender);
                    break;
            }

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
                if (user.UserGender != User.Gender.None) 
                {
                    if (!user.IsActivated)
                    {
                        if (NumActivatedUsers >= NUM_OF_ACTIVE_USERS) //If already 2 active users
                        {
                            int[] userIdx = usersSortedByDistance;
                            for (int i = userIdx.Length - 1; i >= NUM_OF_ACTIVE_USERS; i--) //loop through users from farthest to closest
                            {
                                int uIdx = userIdx[i];
                                User user_i = users[uIdx];
                                if (user_i.IsActivated) //deactivate the farthest one
                                {
                                    user_i.deactivate();
                                    outfitMgr.HideUserOutfit(uIdx);
                                    closetMgr.OnUserLost(uIdx);
                                    break;
                                }
                            }
                        }

                        user.activate();
                        UpdateLiveStatus(NumActivatedUsers);
                    }

                    //switch (gesture)
                    //{
                    //case KinectGestures.Gestures.RaiseLeftHand:
                    //    user.UserGender = User.Gender.Female;
                    //    Debug.Log(string.Format("[LiveRunwayManager] GestureCompleted: User {0} is Female", userId));
                    //    break;
                    //case KinectGestures.Gestures.RaiseRightHand:
                    //    user.UserGender = User.Gender.Male;
                    //    Debug.Log(string.Format("[LiveRunwayManager] GestureCompleted: User {0} is Male", userId));
                    //    break;
                    //}

                    closetMgr.OnUserGenderSelected(userIndex, user.UserGender, user.UserAge);
                }
            }
            
            return true;
        }

        return false;
    }

    public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture, KinectInterop.JointType joint)
    {
        if (!isModeActive)
            return false;

        Closet closet = closetMgr.GetUserCloset(userIndex);
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
            kinectMgr.DetectGesture(userId, KinectGestures.Gestures.Wave);
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

                closetMgr.OnUserLost(userIndex);
                outfitMgr.OnUserLost(userIndex);
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
            float curUserPosZ = kinectMgr.GetUserPosition(kinectMgr.GetUserIdByIndex(allUsers[i])).z;
            float preUserPosZ = kinectMgr.GetUserPosition(kinectMgr.GetUserIdByIndex(allUsers[i - 1])).z;
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
        PoseMgr poseMgr = PoseMgr.Instance;
        if (!poseMgr.IsPosing && numActivatedUsers >= 1)
            poseMgr.StartPosing();
        else if (poseMgr.IsPosing && numActivatedUsers == 0)
            poseMgr.StopPosing();
    }   
}

