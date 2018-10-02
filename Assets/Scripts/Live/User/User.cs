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
                GenderSelectionUI.OnGenderSelected(ugender);
            }
        }
        get
        {
            return ugender;
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

    Camera uiCamera;
    KinectManager manager;

    void Awake ()
    {
        poseAgentSelector = GetComponent<PoseAgentSelector>();
    }

    void OnEnable()
    {
        if(!uiCamera)
        {
            GameObject cameraGO = GameObject.Find("/Live runway/FittingRoom/Camera");
            uiCamera = cameraGO.GetComponent<Camera>();
        }

        if(!manager)
        {
            manager = KinectManager.Instance;
        }
    }

    public void initialize(int idx, UserScore userScore)
    {
        uidx = idx;
        uScore = userScore;

        poseAgentSelector.Init(idx);
        poseAgentSelector.enabled = false;

        uScore.init(idx);
        uScore.gameObject.SetActive(false);

        isReadyToBeActivated = true;
        isActivated = false;
    }

    public void activate()
    {
        if (isReadyToBeActivated)
        {
            poseAgentSelector.enabled = true;
            uScore.gameObject.SetActive(true);

            isReadyToBeActivated = false;
            isActivated = true;
        }
    }

    void Update()
    {  
        if(ugender == Gender.None)
        {
            if (GenderSelectionUI)
            {
                if (isReadyToBeActivated && manager.IsUserTracked(UserID))
                {
                    GenderSelectionUI.transform.position = GetUserScreenPos() + new Vector3(0f, 160f, 0f);
                    GenderSelectionUI.Show();
                }
                else
                    GenderSelectionUI.Hide();
            }
        }
    }

    void OnDestroy()
    {
        if(uScore)
        {
            Destroy(uScore.gameObject);
        }
    }

    Vector3 GetUserScreenPos()
    {
        if (manager && manager.IsInitialized())
        {
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
                return manager.GetJointPosColorOverlay(UserID, iJointIndex, uiCamera, backgroundRect);
            }
        }

        return Vector3.zero;
    }
}
