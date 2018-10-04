using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenderUIController : MonoBehaviour {

    //public SpriteRenderer maleSprite;
    //public SpriteRenderer femaleSprite;
    public GameObject maleGO;
    public GameObject femaleGO;

    public bool needScaling = true;
    public bool needRotating = true;

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

            if(maleGO)
                maleGO.GetComponent<MeshRenderer>().material.color = new Color(maleGO.GetComponent<MeshRenderer>().material.color.r, maleGO.GetComponent<MeshRenderer>().material.color.g, maleGO.GetComponent<MeshRenderer>().material.color.b, m_alpha);

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

            if (femaleGO)
                femaleGO.GetComponent<MeshRenderer>().material.color = new Color(femaleGO.GetComponent<MeshRenderer>().material.color.r, femaleGO.GetComponent<MeshRenderer>().material.color.g, femaleGO.GetComponent<MeshRenderer>().material.color.b, f_alpha);

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

            Debug.Log(string.Format("scale {0} maleGO bounds = {1}", scale, maleGO.GetComponent<MeshRenderer>().bounds.size)); 
            newIconPos += Vector3.down * scalingPosSmoothing * (maxScale - scale)/2f;
        }

        if (needRotating)
        {
            float userRot = manager.GetUserOrientation(userID, false).eulerAngles.y;
            if ((userRot >= 0 && userRot <= maxRotation) || (userRot < 0 && userRot >= -maxRotation))
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, userRot, 0f), 1f);
            else if (userRot >= 360 - maxRotation && userRot <= 360f)
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, userRot - 360f, 0f), 1f);
        }

        transform.position = Vector3.Lerp(transform.position, newIconPos, 1f);
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

