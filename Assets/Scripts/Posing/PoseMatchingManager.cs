using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseMatchingManager : MonoBehaviour {

    public static PoseMatchingManager Instance;

    public GameObject matchedFXParticlesPrefab;

    public ParticleSystem [] comboFXParticles;

    //public float FXDuration = 5f;

    private int numConsecutivePoseMatches = 0;
    private int lastMatcherIdx = -1;

    void Awake()
    {
        Instance = this;
    }

    public void OnPoseMatched(int userIdx)
    {
        GameObject particleGO = (GameObject)Instantiate(matchedFXParticlesPrefab, GetUserScreenPos(userIdx), Quaternion.identity);

        if (userIdx == lastMatcherIdx)
        {
            numConsecutivePoseMatches++;

            switch (numConsecutivePoseMatches)
            {
                case 5:
                    PlayComboParticles(0);
                    break;
                case 8:
                    PlayComboParticles(1);
                    break;
                case 10:
                    PlayComboParticles(2);
                    break;
            }
        }
        else
        {
            numConsecutivePoseMatches = 0;
            StopComboParticles();
        }

        lastMatcherIdx = userIdx;
    }

    public void OnNoPoseMatched()
    {
        lastMatcherIdx = -1;
        numConsecutivePoseMatches = 0;
        StopComboParticles();
    }
    
    public void ClearFX(int userIdx = -1)
    {
        if(userIdx == -1 || //Change to Auto mode
          (userIdx >= 0 && userIdx == lastMatcherIdx)) //last matcher leave the scene
        {
            lastMatcherIdx = -1;
            numConsecutivePoseMatches = 0;
            StopComboParticles();
        }
    }

    private void PlayComboParticles(int particlesIdx)
    {
        for (int i=0; i<comboFXParticles.Length; i++)
        {
            if(i == particlesIdx)
            {
                comboFXParticles[i].gameObject.SetActive(true);
                comboFXParticles[i].Play();
            }
            else
            {
                comboFXParticles[i].Stop();
                comboFXParticles[i].gameObject.SetActive(false);   
            }
        }
    }

    private void StopComboParticles()
    {
        PlayComboParticles(-1);
    }

    Vector3 GetUserScreenPos(int userIdx)
    {
        KinectManager manager = KinectManager.Instance;

        if (manager && manager.IsInitialized())
        {
            long userID = manager.GetUserIdByIndex(userIdx);

            Camera foregroundCamera = Camera.main;
            Rect backgroundRect = foregroundCamera.pixelRect;
            PortraitBackground portraitBack = PortraitBackground.Instance;

            if (portraitBack && portraitBack.enabled)
            {
                backgroundRect = portraitBack.GetBackgroundRect();
            }

            int iJointIndex = (int)KinectInterop.JointType.SpineMid;
            if (manager.IsJointTracked(userID, iJointIndex))
            {
                return manager.GetJointPosColorOverlay(userID, iJointIndex, foregroundCamera, backgroundRect);
            }
        }

        return Vector3.zero;
    }
}
