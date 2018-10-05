using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MR;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Closet : MonoBehaviour {

    public enum Side
    {
        Left = 0,
        Right,
    };

    public Side ClosetSide;

    public float shiftOutTime = 0.5f;
    public float shiftInTime = 0.5f;
    public float showDistanceX = 100f;

    public float idleTime = 3f;

    public ImageProgress activateIcon;
    public Camera cam;

    public RectTransform pointFrom;
    public RectTransform pointTo;
    public RectTransform pointSpine;

    public Vector3 ptFrom;
    public Vector3 ptTo;

    public Image bubble;
    public ParticleSystem bubblePop;
    protected ClosetOutfitItem bubbleOutfit;
    protected bool isBubblePopped = false;
    protected Vector3 bubbleStartPos;
    protected Vector3 bubbleEndPos;
    protected Canvas canvas;

    private Outfit lastSelectedOutfit;

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
    private bool isHiding = false;
    private bool isShowing = false;
    private float hidingStartTime = 0;
    private float showingStartTime = 0;

    private ClosetArrowItem topArrow, bottomArrow;
    private ClosetOutfitItem[] OutfitItems = new ClosetOutfitItem[ClosetManager.NUMBER_CLOSET_ITEMS];

    private int numberPages = 0;

    private float ownerPointDir;

    private RectTransform rectTrans;
    private Vector3 shownPos;
    private Vector3 hiddenPos;
    private float idleElapsedTime = 0f;

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
                OutfitItems[i - 1].OnItemSelectedEvent += OnItemOutfitItemSelected;
            }
        }
    }

    private void OnDestroy()
    {
        foreach (ClosetOutfitItem outfitItem in OutfitItems)
            outfitItem.OnItemSelectedEvent -= OnItemOutfitItemSelected;
    }

    private void OnItemOutfitItemSelected(ClosetItem closetItem)
    {
        ClosetOutfitItem outfitItem = closetItem as ClosetOutfitItem;
        if (outfitItem != null)
        {
            lastSelectedOutfit = outfitItem.outfit;
        }
    }

    void Start ()
    {
        if(!kinect)
            kinect = KinectManager.Instance;

        hiddenPos = rectTrans.anchoredPosition;
        if (ClosetSide == Side.Left)
            shownPos = new Vector3(hiddenPos.x + showDistanceX, hiddenPos.y, hiddenPos.z);
        else if(ClosetSide == Side.Right)
            shownPos = new Vector3(hiddenPos.x - showDistanceX, hiddenPos.y, hiddenPos.z);

        isHidden = true;
        isHiding = false;
        isShowing = false;
        idleElapsedTime = 0f;

        canvas = GetComponentInParent<Canvas>();
    }

    Vector2 ScreenPtToCanvasPt(Vector2 screenPt)
    {
        return new Vector2(
            screenPt.x * canvas.pixelRect.width / cam.scaledPixelWidth,
            screenPt.y * canvas.pixelRect.height / cam.scaledPixelHeight);
    }

    void Update()
    {
        if (isActive)
        {
            if (!isHidden && !isHiding)
            {
                if (kinect && kinect.IsInitialized())
                {
                    if (kinect.IsUserTracked(OwnerID))
                    {
                        // First, get Screen points
                        Vector3 fromScreenPt;
                        Vector3 toScreenPt;
                        Vector3 spineShoulderPt;
                        GetUserPointingScreenPoints(ClosetSide, OwnerID, out fromScreenPt, out toScreenPt, out spineShoulderPt);
                        // Second, convert from Screen Pt to Canvas Pt
                        Vector2 fromLocal = ScreenPtToCanvasPt(fromScreenPt);
                        if (pointFrom != null)
                            pointFrom.position = fromLocal;
                        Vector2 toLocal = ScreenPtToCanvasPt(toScreenPt);
                        if (pointTo != null)
                            pointTo.position = toLocal;
                        Vector2 spineShoulderLocal = ScreenPtToCanvasPt(spineShoulderPt);
                        if (pointSpine != null)
                            pointSpine.position = spineShoulderLocal;

                        // For Debug line rendering, using world coords.
                        ptFrom = pointFrom.transform.position;
                        ptTo = pointTo.transform.position;

                        RaycastHit2D hit = Physics2D.Raycast(ptFrom, ptTo-ptFrom, float.MaxValue, LayerMask.GetMask(new string []{ "Pointable2D" }));
                        //Debug.Log("hit = " + (hit.collider == null ? "null" : hit.collider.name));

                        if (hit.collider != null && 
                            hit.collider.GetComponentInParent<Closet>() == this)// Only allow selecting from this closet
                        {
                            ClosetItem closetItem = hit.collider.GetComponentInParent<ClosetItem>();
                            SetItemToBubble(closetItem);
                            OnClosetItemHover(closetItem);
                        }
                        else
                        {
                            SetItemToBubble(null);
                            OnUnselectAll();
                        }


                        //Debug.Log("Canvas " + canvas.pixelRect);
                        //Debug.Log("bottomArrow.rectPos " + bottomArrow.ItemImage.rectTransform.position);
                        //Debug.Log("bottomArrow.width " + bottomArrow.ItemImage.rectTransform.rect.width);
                        //Debug.Log("bottomArrow.TopBound " + bottomArrow.TopBound);
                        //ownerPointDir = Mathf.Lerp(ownerPointDir, , 0.25f);
                        //if (ownerPointDir >= bottomArrow.BottomBound && ownerPointDir < bottomArrow.TopBound)
                        //{
                        //    OnBottomArrowHover();
                        //}
                        //else if (ownerPointDir >= OutfitItems[3].BottomBound && ownerPointDir < OutfitItems[3].TopBound)
                        //{
                        //    OnOutfitItemHover(3);
                        //}
                        //else if (ownerPointDir >= OutfitItems[2].BottomBound && ownerPointDir < OutfitItems[2].TopBound)
                        //{
                        //    OnOutfitItemHover(2);
                        //}
                        //else if (ownerPointDir >= OutfitItems[1].BottomBound && ownerPointDir < OutfitItems[1].TopBound)
                        //{
                        //    OnOutfitItemHover(1);
                        //}
                        //else if (ownerPointDir >= OutfitItems[0].BottomBound && ownerPointDir < OutfitItems[0].TopBound)
                        //{
                        //    OnOutfitItemHover(0);
                        //}
                        //else if (ownerPointDir >= topArrow.BottomBound && ownerPointDir <= topArrow.TopBound)
                        //{
                        //    OnTopArrowHover();
                        //}
                        //else if (ownerPointDir < bottomArrow.BottomBound || ownerPointDir > topArrow.TopBound)
                        //{
                        //    OnUnselectAll();
                        //}
                    }
                    else
                    {
                        OnUnselectAll();
                    }
                }
            }

            if(idleElapsedTime >= idleTime)
            {
                Hide();
                idleElapsedTime = 0f;
            }

            UpdateBubble();
        }
    }

    private void SetItemToBubble(ClosetItem closetItem)
    {
        ClosetOutfitItem neoOutfit = closetItem as ClosetOutfitItem;
        if (neoOutfit != null && neoOutfit.outfit != lastSelectedOutfit)
        {
            bubble.sprite = neoOutfit.OutfitImage.sprite;
            bubbleStartPos = neoOutfit.OutfitImage.rectTransform.position;
            bubbleEndPos = pointSpine.position;
            bubble.rectTransform.position = bubbleStartPos;
            if (bubbleOutfit != neoOutfit)
            {
                bubble.GetComponent<Animator>().SetTrigger("onBubbleStart");
                isBubblePopped = false;
            }
            bubbleOutfit = neoOutfit;
        }
        else
        {
            bubble.GetComponent<CanvasGroup>().alpha = 0;
            bubble.rectTransform.anchoredPosition = new Vector2(0, -2000);
            bubbleOutfit = null;
        }
    }

    void UpdateBubble()
    {
        if (bubbleOutfit != null)
        {
            bubble.rectTransform.position = Vector3Helper.SmoothStep(bubbleStartPos, bubbleEndPos, bubbleOutfit.hoverProgress);
            //Debug.Log("bubbleOutfit.hoverProgress = " + bubbleOutfit.hoverProgress);
            if (bubbleOutfit.hoverProgress >= 1f)
            {
                if (isBubblePopped == false)
                {
                    bubblePop.transform.position = PoseMatchingManager.Instance.GetUserScreenPos(ownerIdx);
                    bubblePop.Play();

                    isBubblePopped = true;
                }
            }
        }
    }

    private void FixedUpdate()
    {
    }

    void LateUpdate()
    {
        if (!isHidden)
        {
            if (isHiding)
            {
                float t = Mathf.Clamp01((Time.time - hidingStartTime) / shiftOutTime);
                rectTrans.anchoredPosition = Vector3Helper.SmoothStep(shownPos, hiddenPos, t);
                if (t >= 1)
                {
                    isHidden = true;
                    isHiding = false;
                }
            }
        }
        else
        {
            if (isShowing)
            {
                float t = Mathf.Clamp01((Time.time - showingStartTime) / shiftInTime);
                rectTrans.anchoredPosition = Vector3Helper.SmoothStep(hiddenPos, shownPos, t);
                if (t >= 1)
                {
                    isHidden = false;
                    isShowing = false;
                }
            }
        }

        activateIcon.SetImageOut((rectTrans.anchoredPosition.x - shownPos.x)/(hiddenPos.x - shownPos.x));
    }

    public void Show()
    {
        if (isHidden)
        {
            isShowing = true;
            showingStartTime = Time.time;
        }

        activateIcon.SetProgressValue(0f);
    }

    public void Hide()
    {
        if (!isHidden)
        {
            isHiding = true;
            hidingStartTime = Time.time;
        }
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
        isHiding = false;
        isShowing = false;
        idleElapsedTime = 0f;

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

    //private void OnTopArrowHover()
    //{
    //    topArrow.OnItemHover();
    //    bottomArrow.OnItemUnselected();
    //    foreach(ClosetOutfitItem outfitItem in OutfitItems)
    //    {
    //        outfitItem.OnItemUnselected();
    //    }

    //    idolTimeEllapsed = 0f;
    //}

    //private void OnBottomArrowHover()
    //{
    //    topArrow.OnItemUnselected();
    //    bottomArrow.OnItemHover();
    //    foreach (ClosetOutfitItem outfitItem in OutfitItems)
    //    {
    //        outfitItem.OnItemUnselected();
    //    }

    //    idolTimeEllapsed = 0f;
    //}

    //private void OnOutfitItemHover(int idx)
    //{
    //    topArrow.OnItemUnselected();
    //    bottomArrow.OnItemUnselected();
    //    for(int i=0; i< ClosetManager.NUMBER_CLOSET_ITEMS; i++)
    //    {
    //        if(i == idx)
    //        {
    //            OutfitItems[i].OnItemHover();
    //        }
    //        else
    //        {
    //            OutfitItems[i].OnItemUnselected();
    //        }
    //    }

    //    idolTimeEllapsed = 0f;
    //}

    private void OnClosetItemHover(ClosetItem hoveredItem)
    {
        ClosetOutfitItem outfit = hoveredItem as ClosetOutfitItem;
        if (outfit == null || outfit.outfit != lastSelectedOutfit)
            hoveredItem.OnItemHover();

        if (hoveredItem != topArrow)
            topArrow.OnItemUnselected();

        if (hoveredItem != bottomArrow)
            bottomArrow.OnItemUnselected();

        foreach (ClosetOutfitItem outfitItem in OutfitItems)
        {
            if (hoveredItem != outfitItem)
                outfitItem.OnItemUnselected();
        }
        idleElapsedTime = 0f;
    }

    private void OnUnselectAll()
    {
        topArrow.OnItemUnselected();
        bottomArrow.OnItemUnselected();
        foreach (ClosetOutfitItem outfitItem in OutfitItems)
        {
            outfitItem.OnItemUnselected();
        }

        if(!isHidden && !isHiding)
            idleElapsedTime += Time.deltaTime;
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


    void GetUserPointingScreenPoints(Side closetSide, long ownerID, out Vector3 from, out Vector3 to, out Vector3 spineShoulder)
    {
        Vector2 wristPos2D;
        Vector2 elbowPos2D;
        Vector2 spineShoulder2D;
        if (closetSide == Closet.Side.Left)
        {
            Vector3 wristPos = kinect.GetJointPosition(ownerID, (int)KinectInterop.JointType.WristLeft);
            Vector3 elbowPos = kinect.GetJointPosition(ownerID, (int)KinectInterop.JointType.ElbowLeft);
            wristPos2D = cam.WorldToScreenPoint(wristPos);
            elbowPos2D = cam.WorldToScreenPoint(elbowPos);
        }
        else // (closetSide == Closet.Side.Right)
        {
            Vector3 wristPos = kinect.GetJointPosition(ownerID, (int)KinectInterop.JointType.WristRight);
            Vector3 elbowPos = kinect.GetJointPosition(ownerID, (int)KinectInterop.JointType.ElbowRight);
            wristPos2D = cam.WorldToScreenPoint(wristPos);
            elbowPos2D = cam.WorldToScreenPoint(elbowPos);
        }
        Vector3 spineShoulderPos = kinect.GetJointPosition(ownerID, (int)KinectInterop.JointType.SpineMid);
        spineShoulder2D = cam.WorldToScreenPoint(spineShoulderPos);

        to = wristPos2D;
        from = elbowPos2D;
        spineShoulder = spineShoulder2D;
    }

}


#if UNITY_EDITOR
[CustomEditor(typeof(Closet))]
public class ClosetEditor : Editor
{

    void OnSceneGUI()
    {
        Closet t = (Closet)target;
        Handles.color = Color.red;
        Handles.DrawLine(t.ptFrom, t.ptTo);

    }
}
#endif