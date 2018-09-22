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

    public SpriteFader GenderSelectionUI;

    private long uid;
    public long UserID
    {
        get
        {
            return uid;
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
                GenderSelectionUI.StartFadingOut();

                object[] param = { uid, ugender };
                EventMsgDispatcher.Instance.TriggerEvent(EventDef.User_Gender_Selected, param);
            }
        }
        get
        {
            return ugender;
        }
    }

    private UserScore uScore;
    public UserScore UserScore
    {
        get
        {
            return uScore;
        }
    }

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

    public void initialize(long id, UserScore userScore)
    {
        uid = id;
        uScore = userScore;

        poseAgentSelector.Init(this);
    }

    void Update()
    {  
        if(ugender == Gender.None)
        {
            if (GenderSelectionUI)
            {
                GenderSelectionUI.transform.position = GetUserScreenPos() + new Vector3(0f, 160f, 0f);
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
            if (manager.IsJointTracked(uid, iJointIndex))
            {
                return manager.GetJointPosColorOverlay(uid, iJointIndex, uiCamera, backgroundRect);
            }
        }

        return Vector3.zero;
    }
}
