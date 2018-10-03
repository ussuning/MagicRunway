using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenderUIController : MonoBehaviour {

    public SpriteRenderer maleSprite;
    public SpriteRenderer femaleSprite;

    public float IconYOffset = 160f;
    public float maxRotation = 15f;

    private float m_alpha = 1f;
    private float f_alpha = 1f;
    private bool m_isFading = false;
    private bool f_isFading = false;

    [SerializeField]
    private float fadingDuration = 1.0f;    //Time for fading
    private float fadeSpeed = 1.0f;

    private User.Gender selectedGender = User.Gender.None;

    KinectManager manager;
    Camera uiCamera;

    void OnEnable()
    {
        if (!uiCamera)
        {
            GameObject cameraGO = GameObject.Find("/Live runway/FittingRoom/Camera");
            uiCamera = cameraGO.GetComponent<Camera>();
        }

        if (!manager)
        {
            manager = KinectManager.Instance;
        }
    }

    void Start ()
    {
        m_alpha = f_alpha = 1f;
        m_isFading = f_isFading = false;

        if (fadingDuration > 0)
            fadeSpeed = 1 / fadingDuration;
    }
	
	void Update ()
    {
        if (m_isFading)
        {
            m_alpha -= Time.deltaTime * fadeSpeed;
            if (m_alpha < 0f)
                m_alpha = 0f;

            maleSprite.color = new Color(maleSprite.color.r, maleSprite.color.g, maleSprite.color.b, m_alpha);

            if (m_alpha == 0f)
            {
                m_isFading = false;

                if (selectedGender == User.Gender.Female)
                    Invoke("FadeFemaleSprite", 2f);
            }
        }

        if (f_isFading)
        {
            f_alpha -= Time.deltaTime * fadeSpeed;
            if (f_alpha < 0f)
                f_alpha = 0f;

            femaleSprite.color = new Color(femaleSprite.color.r, femaleSprite.color.g, femaleSprite.color.b, f_alpha);

            if (f_alpha == 0f)
            {
                f_isFading = false;

                if (selectedGender == User.Gender.Male)
                    Invoke("FadeMaleSprite", 2f);
            }
        }
    }

    public void OnGenderSelected(User.Gender g)
    {
        selectedGender = g;
        if (g == User.Gender.Male)
        {
            FadeFemaleSprite();
        }
        else if(g == User.Gender.Female)
        {
            FadeMaleSprite();
        }
    }

    public void Hide()
    {
        if (gameObject.activeInHierarchy)
            gameObject.SetActive(false);
    }

    public void Show()
    {
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);
    }

    public void SetUITransform(long userID)
    {
        transform.position = GetUserScreenPos(userID) + new Vector3(0f, IconYOffset, 0f);
        float userRot = manager.GetUserOrientation(userID, false).eulerAngles.y;
        if (userRot >= 0 && userRot <= maxRotation)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, userRot, 0f), 1f);
        else if (userRot >= 360-maxRotation && userRot <= 360f)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, userRot-360f, 0f), 1f);
    }

    private void FadeMaleSprite()
    {
        m_isFading = true;
    }

    private void FadeFemaleSprite()
    {
        f_isFading = true;
    }

    Vector3 GetUserScreenPos(long userID)
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
            if (manager.IsJointTracked(userID, iJointIndex))
            {
                return manager.GetJointPosColorOverlay(userID, iJointIndex, uiCamera, backgroundRect);
            }
        }

        return Vector3.zero;
    }
}

