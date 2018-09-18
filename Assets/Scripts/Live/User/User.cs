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

    private long uid;

    private Gender ugender;
    public Gender UserGender
    {
        set
        {
            ugender = value;
        }
        get
        {
            return ugender;
        }
    }

    //public GameObject outfitGO;
    //public int inventorySlot;
    //public Vector3 uposition;
    //public Vector3 genderIconPosition;
    //private GameObject userSkeletonGO;
    //private bool isOutfitMenuOn = false;

    PoseAgentSelector poseAgentSelector;

    void Awake ()
    {
        poseAgentSelector = GetComponent<PoseAgentSelector>();
    }

    // Use this for initialization
    public User(long id, int index)
    {
        uid = id;
    }

    public void initialize(long id)
    {
        uid = id;
        //inventorySlot = 1;

        poseAgentSelector.Init(id);
    }

    //public GameObject getOutfit()
    //{
    //    return outfitGO;
    //}

    //public Vector3 getPosition()
    //{
    //    return uposition;
    //}

    //public int getInventorySlot()
    //{
    //    return inventorySlot;
    //}

    //public Vector3 getGenderIconPosition()
    //{
    //    return genderIconPosition;
    //}

    //public GameObject getUserSkeletonGO()
    //{
    //    return userSkeletonGO;
    //}

    //public void setUserSkeletonGO(GameObject go)
    //{
    //    userSkeletonGO = go;
    //}

    //public void setOutfit(GameObject outfit)
    //{
    //    outfitGO = outfit;
    //}

    //public bool isOutfitMenuDisplayed()
    //{
    //    return isOutfitMenuOn;
    //}

    //public void setOutfitMenuStatus(bool status)
    //{
    //    isOutfitMenuOn = status;
    //}

    //public void setInventorySlot(int slot)
    //{
    //    inventorySlot = slot;
    //}

    protected Vector3 getCurrentPosition(int iJointIndex)
    {
        /*  KinectManager manager = KinectManager.Instance;

          if (manager && manager.IsInitialized())
          {
              // get the background rectangle (use the portrait background, if available)
              Camera foregroundCamera = Camera.main;
              Rect backgroundRect = foregroundCamera.pixelRect;
              PortraitBackground portraitBack = PortraitBackground.Instance;

              if (portraitBack && portraitBack.enabled)
              {
                  backgroundRect = portraitBack.GetBackgroundRect();
              }

              //int iJointIndex = (int)KinectInterop.JointType.SpineMid;
              if (manager.IsJointTracked(uid, iJointIndex))
              {
                  return manager.GetJointPosColorOverlay(uid, iJointIndex, foregroundCamera, backgroundRect);
              }
          }
          */
          return Vector3.zero;
    }

    void Update()
    {
        // get this user's pos on every tick
       /* uposition = getCurrentPosition((int)KinectInterop.JointType.SpineMid);
        genderIconPosition = getCurrentPosition((int)KinectInterop.JointType.ShoulderLeft);

        //update gameObject pos 
        gameObject.transform.position = getCurrentPosition((int)KinectInterop.JointType.SpineMid);
        */
    }

}
