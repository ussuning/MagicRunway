using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseMatchingManager : MonoBehaviour {

    public static PoseMatchingManager Instance;

    public GameObject matchedFXParticlesPrefab;

    public ParticleSystem [] comboFXParticles;

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

            PlayComboParticles(-1);
        }

        lastMatcherIdx = userIdx;
    }
    
    public void ClearFX()
    {
        numConsecutivePoseMatches = 0;
        lastMatcherIdx = -1;
        PlayComboParticles(-1);
    }

    private void PlayComboParticles(int particlesIdx)
    {
        for(int i=0; i<comboFXParticles.Length; i++)
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
