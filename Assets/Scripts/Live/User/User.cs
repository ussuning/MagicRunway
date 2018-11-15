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

    //public GenderUIController GenderSelectionUI;
    public ActivationIconUIController ActivationIcon;
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

            //if (ugender != Gender.None)
            //{
            //    if (GenderSelectionUI)
            //        GenderSelectionUI.OnGenderSelected(ugender);
            //}
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
            ActivationIcon.gameObject.SetActive(false);

            isReadyToBeActivated = false;
            isActivated = true; 
        }
    }

    public void deactivate(bool isReady = false)
    {
        poseAgentSelector.enabled = false;
        uScore.gameObject.SetActive(false);

        ugender = Gender.None;
        //GenderSelectionUI.ResetUI();
        ActivationIcon.gameObject.SetActive(true);

        isReadyToBeActivated = isReady;
        isActivated = false;
    }

    void Update()
    {  
        //if(ugender == Gender.None)
        //{
        //    if (GenderSelectionUI)
        //    {
        //        if (isReadyToBeActivated && manager.IsUserPositionValid(UserID))
        //        {
        //            if(GenderSelectionUI.SetUITransform(UserID))
        //                GenderSelectionUI.Show();
        //        }
        //        else
        //            GenderSelectionUI.Hide();
        //    }
        //}

        if (isReadyToBeActivated && manager.IsUserPositionValid(UserID))
        {
            if (ActivationIcon)
            {
                if (!ActivationIcon.gameObject.activeSelf)
                    ActivationIcon.gameObject.SetActive(true);
                ActivationIcon.SetUITransform(UserID);
            }
        }
        else
        {
            if (ActivationIcon)
            {
                if (ActivationIcon.gameObject.activeSelf)
                    ActivationIcon.gameObject.SetActive(false);
            }
        }
    }

    Vector3 GetUserScreenPos()
    {
        Vector3 userScreenPos = Vector3.zero;
        Vector3 screenOffset = new Vector3(0f, 200f, -5f);
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
            if (manager.IsJointTracked(UserID, iJointIndex))
            {
                userScreenPos = manager.GetJointPosColorOverlay(UserID, iJointIndex, uiCamera, backgroundRect);
            }
        }

        return userScreenPos + screenOffset;
    }

    void OnDestroy()
    {
        if(uScore)
        {
            Destroy(uScore.gameObject);
        }
    }
}
