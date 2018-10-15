﻿using System.Collections;
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
    public Camera jointCamera;

    public GameObject jointImgPrefab;
    public GameObject bubblePrefab;
    public GameObject pointerArrowPrefab;

    protected RectTransform pointFrom;
    protected RectTransform pointTo;
    protected RectTransform pointSpine;
    protected Image pointerImg;
    protected RectTransform pointerRectTransform;

    public RectTransform offsetTransform;

    public Vector3 ptFrom;
    public Vector3 ptTo;

    protected Image bubble;
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

    private int outfitStartIdx = 0;
    public int OutfitStartIdx
    {
        get
        {
            return outfitStartIdx;
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

        if (bubble == null)
        {
            GameObject bubbleInstance = GameObject.Instantiate(bubblePrefab);
            bubbleInstance.transform.SetParent(this.transform.parent.parent, false);
            bubbleInstance.name = "Bubble" + ClosetSide;
            bubble = bubbleInstance.GetComponent<Image>();
        }

        if (pointFrom == null)
        {
            GameObject jointImgInstance = GameObject.Instantiate(jointImgPrefab);
            jointImgInstance.transform.SetParent(this.transform.parent.parent, false);
            jointImgInstance.name = "pointFrom" + ClosetSide;
            pointFrom = jointImgInstance.GetComponent<RectTransform>();
        }
        if (pointTo == null)
        {
            GameObject jointImgInstance = GameObject.Instantiate(jointImgPrefab);
            jointImgInstance.transform.SetParent(this.transform.parent.parent, false);
            jointImgInstance.name = "pointTo" + ClosetSide;
            pointTo = jointImgInstance.GetComponent<RectTransform>();
        }
        if (pointSpine == null)
        {
            GameObject jointImgInstance = GameObject.Instantiate(jointImgPrefab);
            jointImgInstance.transform.SetParent(this.transform.parent.parent, false);
            jointImgInstance.name = "pointSpine" + ClosetSide;
            pointSpine = jointImgInstance.GetComponent<RectTransform>();
        }
        if (pointerImg == null)
        {
            GameObject instance = GameObject.Instantiate(pointerArrowPrefab);
            instance.transform.SetParent(this.transform.parent.parent, false);
            instance.name = "pointer" + ClosetSide;
            pointerRectTransform = instance.GetComponent<RectTransform>();
            pointerImg = instance.GetComponentInChildren<Image>();
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
            screenPt.x * (canvas.pixelRect.width/canvas.scaleFactor) / jointCamera.scaledPixelWidth,
            screenPt.y * (canvas.pixelRect.height/canvas.scaleFactor) / jointCamera.scaledPixelHeight);
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
                            pointFrom.anchoredPosition = fromLocal;
                        Vector2 toLocal = ScreenPtToCanvasPt(toScreenPt);
                        if (pointTo != null)
                            pointTo.anchoredPosition = toLocal;
                        Vector2 spineShoulderLocal = ScreenPtToCanvasPt(spineShoulderPt);
                        if (pointSpine != null)
                            pointSpine.anchoredPosition = spineShoulderLocal;

                        UpdateOffsetTransform();

                        // For Debug line rendering, using world coords.
                        ptFrom = pointFrom.transform.position;
                        ptTo = pointTo.transform.position;


                        // Gather hits and parse
                        RaycastHit2D [] hits = Physics2D.RaycastAll(ptFrom, ptTo-ptFrom, float.MaxValue, LayerMask.GetMask(new string []{ "Pointable2D" }));
                        //Debug.Log("hit = " + (hit.collider == null ? "null" : hit.collider.name));

                        RaycastHit2D pointerRailHit = new RaycastHit2D();
                        ClosetItem closetItemHit = null;
                        float lowestHitDist = float.MaxValue;
                        foreach (RaycastHit2D hit in hits)
                        {
                            if (hit.collider.gameObject == this.gameObject)
                            {
                                //Debug.Log("pointerRailHit = " + hit.collider.gameObject.name);
                                pointerRailHit = hit;
                            }
                            else if (hit.collider.GetComponentInParent<Closet>() == this)
                            {
                                ClosetItem closetItem = hit.collider.GetComponentInParent<ClosetItem>();
                                if(closetItem != null && hit.distance < lowestHitDist)
                                {
                                    lowestHitDist = hit.distance;
                                    closetItemHit = closetItem;
                                }
                            }
                        }

                        SetItemToBubble(closetItemHit);
                        if (closetItemHit != null) 
                            OnClosetItemHover(closetItemHit);
                        else
                            OnUnselectAll();

                        UpdatePointer(ref pointerRailHit);
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

    void UpdateOffsetTransform()
    {
        Vector2 oldAnchorPos = offsetTransform.anchoredPosition;

        float halfIdleTime = idleTime / 2.0f;
        if (isHidden || isHiding)
        {
            // Set back to default position.
            offsetTransform.anchoredPosition = Vector2.zero;
        }
        else if (idleElapsedTime > halfIdleTime)
        {
            // Start moving back to default position at halfIdleTime.
            float elapsedTime = idleElapsedTime - halfIdleTime;
            float t = elapsedTime / halfIdleTime;
            offsetTransform.anchoredPosition = Vector2.Lerp(offsetTransform.anchoredPosition, Vector2.zero, t);
        }
        else
        {
            // Otherwise, follow user's x position.
            Vector3 neoPos = offsetTransform.position;
            neoPos.x = pointSpine.position.x;
            offsetTransform.position = neoPos;// new Vector3(localPos.x, offsetTransform.anchoredPosition.y);
            float canvasPixelWidth = canvas.pixelRect.width / canvas.scaleFactor;
            if (ClosetSide == Side.Left)
            {
                offsetTransform.anchoredPosition = new Vector2(
                    Mathf.Clamp(offsetTransform.anchoredPosition.x - canvasPixelWidth / 3.0f, 0, canvasPixelWidth * 0.5f),
                    offsetTransform.anchoredPosition.y);
            }
            else
            {
                offsetTransform.anchoredPosition = new Vector2(
                    Mathf.Clamp(offsetTransform.anchoredPosition.x + canvasPixelWidth / 3.0f, -canvasPixelWidth * 0.5f, 0),
                    offsetTransform.anchoredPosition.y);
            }

        }

        // Smooth it.
        offsetTransform.anchoredPosition = Vector2.Lerp(oldAnchorPos, offsetTransform.anchoredPosition, 0.5f);

    }
    float timeSincePointerHit = 0;
    float pointerFadeTime = 0.5f;

    private void UpdatePointer(ref RaycastHit2D hit)
    {
        // Set position.
        if (hit.collider != null)
        {
            pointerRectTransform.position = hit.point;
            timeSincePointerHit = 0;

            // Set direction.
            Vector3 direction = pointTo.position - pointFrom.position;
            direction.z = 0;
            direction = direction.normalized;
            Vector3 lookAtPos = new Vector3(pointFrom.position.x, pointFrom.position.y) + direction * 5000f;
            lookAtPos.z = 0;
            pointerRectTransform.LookAt(lookAtPos, Vector3.forward);
        }
        else
        {
            timeSincePointerHit += Time.deltaTime;
            if (timeSincePointerHit > pointerFadeTime + 0.1f)
                pointerRectTransform.position = new Vector3(0, -3000);
        }


        //else
        //    pointerRectTransform.position = new Vector3(pointTo.position.x, pointTo.position.y);



        // Control the alpha
        Color neoColor = pointerImg.color;
        if (isHidden || isHiding)
        {
            // Hide the pointer
            neoColor.a = 0;
        }
        else if (timeSincePointerHit > 0)
        {
            // Start fading when idle
            float t = Mathf.Clamp01(idleElapsedTime / pointerFadeTime); // fade in half a second
            neoColor.a = 1.0f - t;
        }
        else
        {
            // Show the pointer.
            neoColor.a = 1;
        }

        Color neoLerpedColor = Color.Lerp(pointerImg.color, neoColor, 0.5f);
        // Smooth transition color
        pointerImg.color = neoLerpedColor;
    }

    private void SetItemToBubble(ClosetItem closetItem)
    {
        ClosetOutfitItem neoOutfit = closetItem as ClosetOutfitItem;
        if (neoOutfit != null && neoOutfit.outfit != lastSelectedOutfit)
        {
            bubble.sprite = neoOutfit.OutfitImage.sprite;
            bubbleStartPos = neoOutfit.OutfitImage.rectTransform.position;
            bubbleEndPos = Vector3.Lerp(pointSpine.position, pointFrom.position, 0.5f);
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
            //Debug.Log("Closet.Show()");
            isShowing = true;
            isHiding = false;
            showingStartTime = Time.time;
        }

        activateIcon.SetProgressValue(0f);
    }

    public void Hide()
    {
        if (!isHidden)
        {
            //Debug.Log("Closet.Hide()");
            isHiding = true;
            isShowing = false;
            hidingStartTime = Time.time;
        }
    }

    public void SetCloset(int userIdx, User.Gender userGender, List<Outfit> outfits, int outfitIdx = 0)
    {
        //this.ownerId = userID;
        this.ownerIdx = userIdx;
        this.ownerGender = userGender;
        this.outfits = outfits;
        this.outfitStartIdx = outfitIdx;

        numberPages = Mathf.CeilToInt((float)outfits.Count / ClosetManager.NUMBER_CLOSET_ITEMS);

        SetClosetImages(GetDisplayedOutfits(outfits, outfitStartIdx));

        topArrow.animator.SetBool("isLeft", ClosetSide == Side.Left);
        bottomArrow.animator.SetBool("isLeft", ClosetSide == Side.Left);
        foreach (ClosetItem item in OutfitItems)
            item.animator.SetBool("isLeft", ClosetSide == Side.Left);

        activateIcon.gameObject.SetActive(true);
        isActive = true;
    }

    public void ResetCloset()
    {
        ClearClosetImage();
        ownerIdx = -1;
        ownerGender = User.Gender.None;
        outfits = null;
        outfitStartIdx = 0;
        lastSelectedOutfit = null;
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
        outfitStartIdx = 0;
        lastSelectedOutfit = null;
        isActive = false;
        activateIcon.gameObject.SetActive(false);
    }

    public void PageUp()
    {
        for (int i = 0; i < ClosetManager.NUMBER_CLOSET_ITEMS; i++)
        {
            outfitStartIdx--;
            if (outfitStartIdx < 0)
                outfitStartIdx = outfits.Count - 1;
        }

        SetClosetImages(GetDisplayedOutfits(outfits, outfitStartIdx));
    }

    public void PageDown()
    {
        for(int i=0; i<ClosetManager.NUMBER_CLOSET_ITEMS; i++)
        {
            outfitStartIdx++;
            if (outfitStartIdx >= outfits.Count)
                outfitStartIdx = 0;
        }

        SetClosetImages(GetDisplayedOutfits(outfits, outfitStartIdx));
    }

    private void SetClosetImages(List<Outfit> outfits)
    {
        StartCoroutine(DoSetClosetImages(outfits));
    }

    IEnumerator DoSetClosetImages(List<Outfit> outfits)
    {
        // Hide animation
        foreach (ClosetItem item in OutfitItems)
            item.SetNextAnimTrigger("onHide");

        yield return new WaitForSeconds(0.25f); // wait for hide animation

        // Show new outfits
        for (int i = 0; i < OutfitItems.Length; i++)
        {
            yield return new WaitForSeconds(0.01f); // wait for hide animation
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

    List<Outfit> GetDisplayedOutfits(List<Outfit> displayedOutfits, int startOutfitIndex)
    {
        List<Outfit> dOutfits = new List<Outfit>();
        for (int i = 0; i < ClosetManager.NUMBER_CLOSET_ITEMS; i++)
        {
            dOutfits.Add(displayedOutfits[(startOutfitIndex + i )% displayedOutfits.Count]);
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
            wristPos2D = jointCamera.WorldToScreenPoint(wristPos);
            elbowPos2D = jointCamera.WorldToScreenPoint(elbowPos);
        }
        else // (closetSide == Closet.Side.Right)
        {
            Vector3 wristPos = kinect.GetJointPosition(ownerID, (int)KinectInterop.JointType.WristRight);
            Vector3 elbowPos = kinect.GetJointPosition(ownerID, (int)KinectInterop.JointType.ElbowRight);
            wristPos2D = jointCamera.WorldToScreenPoint(wristPos);
            elbowPos2D = jointCamera.WorldToScreenPoint(elbowPos);
        }
        Vector3 spineShoulderPos = kinect.GetJointPosition(ownerID, (int)KinectInterop.JointType.SpineMid);
        spineShoulder2D = jointCamera.WorldToScreenPoint(spineShoulderPos);

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