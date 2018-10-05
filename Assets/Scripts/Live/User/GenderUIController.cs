using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenderUIController : MonoBehaviour {

    public GameObject maleGO;
    public GameObject femaleGO;

    public bool needScaling = true;
    public bool needRotating = true;

    public float followSpeed = 1f;
    public float rotateSpeed = 1f;
    public Vector3 IconOffset = new Vector3(0f, 200f, -5f);
    public float scalingPosSmoothing = 110f;
    public float maxScale = 2f;
    public float minScale = 1f;
    public float maxRotation = 15f;

    public float fadingDuration = 1.0f;    //Time for fading
    private float fadeSpeed = 1.0f;

    private float m_alpha = 1f;
    private float f_alpha = 1f;
    private bool m_isFading = false;
    private bool f_isFading = false;

    private User.Gender selectedGender = User.Gender.None;

    private MeshRenderer maleMesh;
    private MeshRenderer femaleMesh;

    KinectManager manager;
    Camera uiCamera;

    void Awake()
    {
        if(maleGO)
            maleMesh = maleGO.GetComponent<MeshRenderer>();
        if(femaleGO)
            femaleMesh = femaleGO.GetComponent<MeshRenderer>();
    }

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

        if (fadingDuration > 0f)
            fadeSpeed = 1f / fadingDuration;
    }
	
	void Update ()
    {
        if (m_isFading)
        {
            m_alpha -= Time.deltaTime * fadeSpeed;
            if (m_alpha < 0f)
                m_alpha = 0f;

            if(maleMesh)
                maleMesh.material.color = new Color(maleMesh.material.color.r, maleMesh.material.color.g, maleMesh.material.color.b, m_alpha);

            if (m_alpha == 0f)
            {
                m_isFading = false;

                if (selectedGender == User.Gender.Female)
                    Invoke("FadeFemaleIcon", 2f);
            }
        }

        if (f_isFading)
        {
            f_alpha -= Time.deltaTime * fadeSpeed;
            if (f_alpha < 0f)
                f_alpha = 0f;

            if (femaleMesh)
                femaleMesh.material.color = new Color(femaleMesh.material.color.r, femaleMesh.material.color.g, femaleMesh.material.color.b, f_alpha);

            if (f_alpha == 0f)
            {
                f_isFading = false;

                if (selectedGender == User.Gender.Male)
                    Invoke("FadeMaleIcon", 2f);
            }
        }
    }

    public void OnGenderSelected(User.Gender g)
    {
        selectedGender = g;
        if (g == User.Gender.Male)
        {
            FadeFemaleIcon();
        }
        else if(g == User.Gender.Female)
        {
            FadeMaleIcon();
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
        Vector3 userScreenPos = GetUserScreenPos(userID);
        Vector3 newIconPos = userScreenPos + IconOffset;

        if (needScaling)
        {
            float kinectMaxDistance = manager.maxUserDistance;
            float kinectMinDistance = manager.minUserDistance;
            float scale = (kinectMaxDistance - userScreenPos.z) / (kinectMaxDistance - kinectMinDistance);
            scale = Mathf.Clamp(scale * (maxScale - minScale) + minScale, minScale, maxScale);
            transform.localScale = new Vector3(scale, scale, scale);

            newIconPos += Vector3.down * scalingPosSmoothing * (maxScale - scale)/2f;
        }

        if (needRotating)
        {
            float userRot = manager.GetUserOrientation(userID, false).eulerAngles.y;
            if ((userRot >= 0f && userRot <= maxRotation) || (userRot < 0f && userRot >= -maxRotation))
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, userRot, 0f), rotateSpeed);
            else if (userRot >= 360f - maxRotation && userRot <= 360f)
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, userRot - 360f, 0f), rotateSpeed);
        }

        transform.position = Vector3.Lerp(transform.position, newIconPos, followSpeed);
    }

    private void FadeMaleIcon()
    {
        m_isFading = true;
    }

    private void FadeFemaleIcon()
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

