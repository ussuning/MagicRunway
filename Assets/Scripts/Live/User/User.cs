using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User : MonoBehaviour {

    public enum Gender
    {
        None = 0,
        Female,
        Male
    };

    public GenderUIController GenderSelectionUI;
    public TextMesh DebugText;

    private int uidx;
    public int UserIndex
    {
        get
        {
            return uidx;
        }
    }

    public long UserID
    {
        get
        {
            return manager.GetUserIdByIndex(uidx);
        }
    }

    private Gender ugender;
    public Gender UserGender
    {
        set
        {
            ugender = value;

            if (ugender != Gender.None)
            {
                if (GenderSelectionUI)
                    GenderSelectionUI.OnGenderSelected(ugender);
            }
        }
        get
        {
            return ugender;
        }
    }

    private int uage = 20;
    public int UserAge
    {
        set
        {
            uage = value;
        }
        get
        {
            return uage;
        }
    }

    private bool isActivated = false;
    public bool IsActivated
    {
        get
        {
            return isActivated;
        }
    }

    private bool isReadyToBeActivated = true;
    public bool IsReadyToBeActivated
    {
        set
        {
            isReadyToBeActivated = value;
        }
    }


    private UserScore uScore;
    private PoseAgentSelector poseAgentSelector;

    KinectManager manager;

    void Awake ()
    {
        poseAgentSelector = GetComponent<PoseAgentSelector>();
    }

    void OnEnable()
    {
        if(!manager)
        {
            manager = KinectManager.Instance;
        }
    }

    public void initialize(int idx, UserScore userScore, bool isReady = false)
    {
        uidx = idx;
        uScore = userScore;

        poseAgentSelector.Init(idx);
        poseAgentSelector.enabled = false;

        uScore.init(idx);
        uScore.gameObject.SetActive(false);

        isReadyToBeActivated = isReady;
        isActivated = false;
    }

    public void activate()
    {
        if (isReadyToBeActivated)
        {
            poseAgentSelector.enabled = true;
            uScore.gameObject.SetActive(true);

            //ugender = Gender.None;

            isReadyToBeActivated = false;
            isActivated = true; 
        }
    }

    public void deactivate(bool isReady = false)
    {
        poseAgentSelector.enabled = false;
        uScore.gameObject.SetActive(false);

        ugender = Gender.None;
        GenderSelectionUI.ResetUI();

        isReadyToBeActivated = isReady;
        isActivated = false;
    }

    void Update()
    {  
        if(ugender == Gender.None)
        {
            if (GenderSelectionUI)
            {
                if (isReadyToBeActivated && manager.IsUserPositionValid(UserID))
                {
                    if(GenderSelectionUI.SetUITransform(UserID))
                        GenderSelectionUI.Show();
                }
                else
                    GenderSelectionUI.Hide();
            }
        }

#if UNITY_EDITOR
        //if (DebugText)
        //{
        //    DebugText.text = string.Format("User {0}: {1}", UserIndex, UserID);
        //    DebugText.transform.position = GenderSelectionUI.transform.position;
        //    DebugText.gameObject.SetActive(true);
        //}
#else
        if (DebugText)
        {
            DebugText.gameObject.SetActive(false);
        }
#endif
    }

#if UNITY_EDITOR
    Vector3 GetUserScreenPos(long userID)
    {
        Vector3 userScreenPos = Vector3.zero;
        if (manager && manager.IsInitialized())
        {
            GameObject cameraGO = GameObject.Find("/Live runway/FittingRoom/Camera");
            Camera uiCamera = cameraGO.GetComponent<Camera>();

            // get the background rectangle (use the portrait background, if available)
            Rect backgroundRect = uiCamera.pixelRect;
            PortraitBackground portraitBack = PortraitBackground.Instance;

            if (portraitBack && portraitBack.enabled)
            {
                backgroundRect = portraitBack.GetBackgroundRect();
            }

            int iJointIndex = (int)KinectInterop.JointType.Head;
            if (manager.IsJointTracked(userID, iJointIndex))
            {
                userScreenPos = manager.GetJointPosColorOverlay(userID, iJointIndex, uiCamera, backgroundRect);
            }
        }

        return userScreenPos;
    }
#endif

    void OnDestroy()
    {
        if(uScore)
        {
            Destroy(uScore.gameObject);
        }
    }
}
