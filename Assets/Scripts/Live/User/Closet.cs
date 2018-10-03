using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Closet : MonoBehaviour {

    public enum Side
    {
        Left = 0,
        Right,
    };

    public Side ClosetSide;

    public float shiftOutSpeed = 10f;
    public float shiftInSpeed = 20f;

    public float idolTime = 5f;

    public ImageProgress activateIcon;
    public Camera cam;

    //private long ownerId;
    private int ownerIdx;
    public int OwnerIndex
    {
        get
        {
            return ownerIdx;
        }
    }
    public long OwnerID
    {
        get
        {
            return kinect.GetUserIdByIndex(ownerIdx);
        }
    }

    private User.Gender ownerGender;
    public User.Gender OwnerGender
    {
        get
        {
            return ownerGender;
        }
    }

    private List<Outfit> outfits;
    public List<Outfit> Outfits
    {
        get
        {
            return outfits;
        }
    }

    private int outfitPageIdx = 0;
    public int OutfitPageIndex
    {
        get
        {
            return outfitPageIdx;
        }
    }

    private bool isActive = false;
    public bool IsActive
    {
        get
        {
            return isActive;
        }
    }

    private bool isHidden = true;
    public bool IsHidden
    {
        get
        {
            return isHidden;
        }
    }
    private bool isHidding = false;
    private bool isShowing = false;

    private ClosetArrowItem topArrow, bottomArrow;
    private ClosetOutfitItem[] OutfitItems = new ClosetOutfitItem[ClosetManager.NUMBER_CLOSET_ITEMS];

    private int numberPages = 0;

    private float ownerPointDir;

    private RectTransform rectTrans;
    private Vector3 shownPos;
    private Vector3 hiddenPos;
    private float idolTimeEllapsed = 0f;

    KinectManager kinect;

    void Awake ()
    {
        rectTrans = GetComponent<RectTransform>();

        for (int i=0; i<transform.childCount; i++)
        {
            if(i == 0)
            {
                topArrow = transform.GetChild(i).GetComponent<ClosetArrowItem>();
                topArrow.Closet = this;
            }
            else if(i == transform.childCount - 1)
            {
                bottomArrow = transform.GetChild(i).GetComponent<ClosetArrowItem>();
                bottomArrow.Closet = this;
            }
            else
            {
                OutfitItems[i-1] = transform.GetChild(i).GetComponent<ClosetOutfitItem>();
                OutfitItems[i - 1].Closet = this;
            }
        }
    }

    void Start ()
    {
        if(!kinect)
            kinect = KinectManager.Instance;

        hiddenPos = rectTrans.anchoredPosition;
        if (ClosetSide == Side.Left)
            shownPos = new Vector3(hiddenPos.x + rectTrans.sizeDelta.x, hiddenPos.y, hiddenPos.z);
        else if(ClosetSide == Side.Right)
            shownPos = new Vector3(hiddenPos.x - rectTrans.sizeDelta.x, hiddenPos.y, hiddenPos.z);

        isHidden = true;
        isHidding = false;
        isShowing = false;
        idolTimeEllapsed = 0f;
    }

    void Update()
    {
        if (isActive)
        {
            if (!isHidden && !isHidding)
            {
                if (kinect && kinect.IsInitialized())
                {
                    if (kinect.IsUserTracked(OwnerID))
                    {
                        ownerPointDir = Mathf.Lerp(ownerPointDir, GetPointDirection(ClosetSide, OwnerID), 0.25f);
                        if (ownerPointDir >= bottomArrow.BottomBound && ownerPointDir < bottomArrow.TopBound)
                        {
                            OnBottomArrowHover();
                        }
                        else if (ownerPointDir >= OutfitItems[3].BottomBound && ownerPointDir < OutfitItems[3].TopBound)
                        {
                            OnOutfitItemHover(3);
                        }
                        else if (ownerPointDir >= OutfitItems[2].BottomBound && ownerPointDir < OutfitItems[2].TopBound)
                        {
                            OnOutfitItemHover(2);
                        }
                        else if (ownerPointDir >= OutfitItems[1].BottomBound && ownerPointDir < OutfitItems[1].TopBound)
                        {
                            OnOutfitItemHover(1);
                        }
                        else if (ownerPointDir >= OutfitItems[0].BottomBound && ownerPointDir < OutfitItems[0].TopBound)
                        {
                            OnOutfitItemHover(0);
                        }
                        else if (ownerPointDir >= topArrow.BottomBound && ownerPointDir <= topArrow.TopBound)
                        {
                            OnTopArrowHover();
                        }
                        else if (ownerPointDir < bottomArrow.BottomBound || ownerPointDir > topArrow.TopBound)
                        {
                            OnUnselectAll();
                        }
                    }
                    else
                    {
                        OnUnselectAll();
                    }
                }
            }

            if(idolTimeEllapsed >= idolTime)
            {
                Hide();
                idolTimeEllapsed = 0f;
            }
        }
    }

    void LateUpdate()
    {
        if (!isHidden)
        {
            if (isHidding)
            {
                if (ClosetSide == Side.Left)
                {
                    rectTrans.anchoredPosition += Vector2.left * shiftOutSpeed;
                    if (rectTrans.anchoredPosition.x < hiddenPos.x)
                    {
                        rectTrans.anchoredPosition = hiddenPos;
                        isHidden = true;
                        isHidding = false;
                    }
                }
                else if (ClosetSide == Side.Right)
                {
                    rectTrans.anchoredPosition += Vector2.right * shiftOutSpeed;
                    if (rectTrans.anchoredPosition.x > hiddenPos.x)
                    {
                        rectTrans.anchoredPosition = hiddenPos;
                        isHidden = true;
                        isHidding = false;
                    }
                }
            }
        }
        else
        {
            if (isShowing)
            {
                if (ClosetSide == Side.Left)
                {
                    rectTrans.anchoredPosition += Vector2.right * shiftInSpeed;
                    if (rectTrans.anchoredPosition.x > shownPos.x)
                    {
                        rectTrans.anchoredPosition = shownPos;
                        isHidden = false;
                        isShowing = false;
                    }
                }
                else if (ClosetSide == Side.Right)
                {
                    rectTrans.anchoredPosition += Vector2.left * shiftInSpeed;
                    if (rectTrans.anchoredPosition.x < shownPos.x)
                    {
                        rectTrans.anchoredPosition = shownPos;
                        isHidden = false;
                        isShowing = false;
                    }
                }
            }
        }

        activateIcon.SetImageOut((rectTrans.anchoredPosition.x - shownPos.x)/(hiddenPos.x - shownPos.x));
    }

    public void Show()
    {
        if(isHidden)
            isShowing = true;

        activateIcon.SetProgressValue(0f);
    }

    public void Hide()
    {
        if (!isHidden)
            isHidding = true;
    }

    public void SetCloset(int userIdx, User.Gender userGender, List<Outfit> outfits, int pageIdx = 0)
    {
        //this.ownerId = userID;
        this.ownerIdx = userIdx;
        this.ownerGender = userGender;
        this.outfits = outfits;
        this.outfitPageIdx = pageIdx;

        numberPages = Mathf.CeilToInt((float)outfits.Count / ClosetManager.NUMBER_CLOSET_ITEMS);

        SetClosetImage(GetDisplayedOutfits(outfits, outfitPageIdx));

        activateIcon.gameObject.SetActive(true);
        isActive = true;
    }

    public void ResetCloset()
    {
        ClearClosetImage();
        ownerIdx = -1;
        ownerGender = User.Gender.None;
        outfits = null;
        outfitPageIdx = 0;
    }

    public void Clear(bool resetPos = false)
    {
        if(resetPos)
            rectTrans.anchoredPosition = hiddenPos;
        isHidden = true;
        isHidding = false;
        isShowing = false;
        idolTimeEllapsed = 0f;

        ClearClosetImage();
        ownerIdx = -1;
        ownerGender = User.Gender.None;
        if(outfits != null)
            outfits.Clear();
        outfitPageIdx = 0;
        isActive = false;
        activateIcon.gameObject.SetActive(false);
    }

    public void PageUp()
    {
        outfitPageIdx++;
        if (outfitPageIdx >= numberPages)
            outfitPageIdx = 0;

        SetClosetImage(GetDisplayedOutfits(outfits, outfitPageIdx));
    }

    public void PageDown()
    {
        outfitPageIdx--;
        if (outfitPageIdx < 0)
            outfitPageIdx = numberPages - 1;

        SetClosetImage(GetDisplayedOutfits(outfits, outfitPageIdx));
    }

    private void SetClosetImage(List<Outfit> outfits)
    {
        for(int i=0; i<OutfitItems.Length; i++)
        {
            OutfitItems[i].SetOutfit(outfits[i]);
        }

        topArrow.ShowArrow();
        bottomArrow.ShowArrow();
    }

    private void ClearClosetImage()
    {
        for (int i = 0; i < OutfitItems.Length; i++)
        {
            OutfitItems[i].ClearOutfit();
        }
        topArrow.HideArrow();
        bottomArrow.HideArrow();
    }

    private void OnTopArrowHover()
    {
        topArrow.OnItemHover();
        bottomArrow.OnItemUnselected();
        foreach(ClosetOutfitItem outfitItem in OutfitItems)
        {
            outfitItem.OnItemUnselected();
        }

        idolTimeEllapsed = 0f;
    }

    private void OnBottomArrowHover()
    {
        topArrow.OnItemUnselected();
        bottomArrow.OnItemHover();
        foreach (ClosetOutfitItem outfitItem in OutfitItems)
        {
            outfitItem.OnItemUnselected();
        }

        idolTimeEllapsed = 0f;
    }

    private void OnOutfitItemHover(int idx)
    {
        topArrow.OnItemUnselected();
        bottomArrow.OnItemUnselected();
        for(int i=0; i< ClosetManager.NUMBER_CLOSET_ITEMS; i++)
        {
            if(i == idx)
            {
                OutfitItems[i].OnItemHover();
            }
            else
            {
                OutfitItems[i].OnItemUnselected();
            }
        }

        idolTimeEllapsed = 0f;
    }

    private void OnUnselectAll()
    {
        topArrow.OnItemUnselected();
        bottomArrow.OnItemUnselected();
        foreach (ClosetOutfitItem outfitItem in OutfitItems)
        {
            outfitItem.OnItemUnselected();
        }

        if(!isHidden && !isHidding)
            idolTimeEllapsed += Time.deltaTime;
    }

    List<Outfit> GetDisplayedOutfits(List<Outfit> displayedOutfits, int displayedPage)
    {
        List<Outfit> dOutfits = new List<Outfit>();
        for (int i = 0; i < ClosetManager.NUMBER_CLOSET_ITEMS; i++)
        {
            dOutfits.Add(displayedOutfits[(displayedPage * ClosetManager.NUMBER_CLOSET_ITEMS + i) % displayedOutfits.Count]);
        }

        return dOutfits;
    }


    float GetPointDirection(Side closetSide, long ownerID)
    {
        if (closetSide == Closet.Side.Left)
        {
            //Vector3 lFingersPos = kinect.GetJointPosColorOverlay(ownerID, (int)KinectInterop.JointType.HandTipLeft, cam, cam.pixelRect);
            //Vector3 lHandPos = kinect.GetJointPosColorOverlay(ownerID, (int)KinectInterop.JointType.HandLeft, cam, cam.pixelRect);
            Vector3 lWristPos = kinect.GetJointPosColorOverlay(ownerID, (int)KinectInterop.JointType.WristLeft, cam, cam.pixelRect);
            //Vector3 lElbowPos = kinect.GetJointPosColorOverlay(ownerID, (int)KinectInterop.JointType.ElbowLeft, cam, cam.pixelRect);
            Vector3 lShoulderPos = kinect.GetJointPosColorOverlay(ownerID, (int)KinectInterop.JointType.ShoulderLeft, cam, cam.pixelRect);
            return (lWristPos - lShoulderPos).normalized.y;
        }
        else if (closetSide == Closet.Side.Right)
        {
            //Vector3 rFingersPos = kinect.GetJointPosColorOverlay(ownerID, (int)KinectInterop.JointType.HandTipRight, cam, cam.pixelRect);
            //Vector3 rHandPos = kinect.GetJointPosColorOverlay(ownerID, (int)KinectInterop.JointType.HandRight, cam, cam.pixelRect);
            Vector3 rWristPos = kinect.GetJointPosColorOverlay(ownerID, (int)KinectInterop.JointType.WristRight, cam, cam.pixelRect);
            //Vector3 rElbowPos = kinect.GetJointPosColorOverlay(ownerID, (int)KinectInterop.JointType.ElbowRight, cam, cam.pixelRect);
            Vector3 rShoulderPos = kinect.GetJointPosColorOverlay(ownerID, (int)KinectInterop.JointType.ShoulderRight, cam, cam.pixelRect);
            return (rWristPos - rShoulderPos).normalized.y;
        }

        return -1f;
    }

}
