using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MR;

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
    public float tutorialMinTime = 2f;

    public OutfitSelectionTutorialController selectionTutorial;
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

    private User.Gender closetGender;
    public User.Gender ClosetGender
    {
        get
        {
            return closetGender;
        }
    }

    private List<Outfit> outfits_m;
    public List<Outfit> Outfits_m
    {
        get
        {
            return outfits_m;
        }
    }

    private List<Outfit> outfits_f;
    public List<Outfit> Outfits_f
    {
        get
        {
            return outfits_f;
        }
    }

    private int outfitStartIdx_m = 0;
    public int OutfitStartIdx_m
    {
        get
        {
            return outfitStartIdx_m;
        }
    }

    private int outfitStartIdx_f = 0;
    public int OutfitStartIdx_f
    {
        get
        {
            return outfitStartIdx_f;
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

    private ClosetGenderItem genderSwapButton;
    private ClosetArrowItem topArrow, bottomArrow;
    private ClosetOutfitItem[] OutfitItems = new ClosetOutfitItem[ClosetManager.NUMBER_CLOSET_ITEMS];

    private float ownerPointDir;

    private RectTransform rectTrans;
    private Vector3 shownPos;
    private Vector3 hiddenPos;
    private float idleElapsedTime = 0f;

    Vector3 lastFromScreenPt;
    Vector3 lastToScreenPt;
    Vector3 lastSpineShoulderPt;

    KinectManager kinect;

    void Awake ()
    {
        rectTrans = GetComponent<RectTransform>();

        for (int i=0; i<transform.childCount; i++)
        {
            if(i == 0)
            {
                genderSwapButton = transform.GetChild(i).GetComponent<ClosetGenderItem>();
                genderSwapButton.Closet = this;
            }
            else if(i == 1)
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
                ClosetOutfitItem outfitItem = transform.GetChild(i).GetComponent<ClosetOutfitItem>();
                outfitItem.Closet = this;
                outfitItem.OnItemSelectedEvent += OnItemOutfitItemSelected;
                OutfitItems[i - 2] = outfitItem;
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

        selectionTutorial.EndTutorial();
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
                        // Attempt for each arm side until a hit is registered.
                        foreach (Side armSide in new Side[] { ClosetSide, ClosetSide == Side.Left ? Side.Right : Side.Left })
                        {
                            // First, get Screen points
                            GetUserPointingScreenPoints(armSide, OwnerID, ref lastFromScreenPt, ref lastToScreenPt, ref lastSpineShoulderPt);
                            // Second, convert from Screen Pt to Canvas Pt
                            Vector2 fromLocal = ScreenPtToCanvasPt(lastFromScreenPt);
                            if (pointFrom != null)
                                pointFrom.anchoredPosition = fromLocal;
                            Vector2 toLocal = ScreenPtToCanvasPt(lastToScreenPt);
                            if (pointTo != null)
                                pointTo.anchoredPosition = toLocal;
                            Vector2 spineShoulderLocal = ScreenPtToCanvasPt(lastSpineShoulderPt);
                            if (pointSpine != null)
                                pointSpine.anchoredPosition = spineShoulderLocal;

                            UpdateOffsetTransform();

                            // For Debug line rendering, using world coords.
                            ptFrom = pointFrom.transform.position;
                            ptTo = pointTo.transform.position;


                            // Gather hits and parse
                            RaycastHit2D[] hits = Physics2D.RaycastAll(ptFrom, ptTo - ptFrom, float.MaxValue, LayerMask.GetMask(new string[] { "Pointable2D" }));
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
                                    if (closetItem != null && hit.distance < lowestHitDist)
                                    {
                                        lowestHitDist = hit.distance;
                                        closetItemHit = closetItem;
                                    }
                                }
                            }

                            // If the first arm missed, try the other arm first;
                            if (armSide == ClosetSide && pointerRailHit.collider == null)
                                continue;

                            SetItemToBubble(closetItemHit);
                            if (closetItemHit != null)
                                OnClosetItemHover(closetItemHit);
                            else
                                OnUnselectAll();

                            UpdatePointer(ref pointerRailHit);

                            // If this arm did not miss, then no need to try the other arm.
                            if (pointerRailHit.collider != null)
                                break;
                        }
                    }
                    else
                    {
                        OnUnselectAll();
                    }
                }
            }

            if (selectionTutorial.IsTutorialFinished && idleElapsedTime >= idleTime)
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

        selectionTutorial.ShowTutorial();
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

        selectionTutorial.HideTutorial();
    }

    public void SetCloset(int userIdx, User.Gender userGender, int userAge, Outfits outfits /*, int outfitIdx_m = 0, int outfitIdx_f = 0, bool isTutorialFinished = false*/)
    {
        this.ownerIdx = userIdx;
        this.closetGender = userGender;

        this.outfits_m = outfits.maleOutfits.OrderBy(row => row.age).ToList(); ;
        this.outfits_f = outfits.femaleOutfits.OrderBy(row => row.age).ToList(); ;

        this.outfitStartIdx_m = GetOutfitIndexByAge(outfits_m, userAge);
        this.outfitStartIdx_f = GetOutfitIndexByAge(outfits_f, userAge); ;

        genderSwapButton.SetGender(closetGender);

        if (closetGender == User.Gender.Male)
            SetClosetImages(GetDisplayedOutfits(outfits_m, outfitStartIdx_m));
        else if (closetGender == User.Gender.Female)
            SetClosetImages(GetDisplayedOutfits(outfits_f, outfitStartIdx_f));

        selectionTutorial.StartTutorial();

        topArrow.animator.SetBool("isLeft", ClosetSide == Side.Left);
        bottomArrow.animator.SetBool("isLeft", ClosetSide == Side.Left);
        foreach (ClosetItem item in OutfitItems)
            item.animator.SetBool("isLeft", ClosetSide == Side.Left);

        activateIcon.gameObject.SetActive(true);
        isActive = true;
    }

    public void SwapCloset(int userIdx, User.Gender closetGender, List<Outfit> mOutfits, List<Outfit> fOutfits, int outfitIdx_m, int outfitIdx_f, bool isTutorialFinished)
    {
        ResetCloset();

        this.ownerIdx = userIdx;
        this.closetGender = closetGender; 

        this.outfits_m = mOutfits;
        this.outfits_f = fOutfits;

        this.outfitStartIdx_m = outfitIdx_m;
        this.outfitStartIdx_f = outfitIdx_f;

        genderSwapButton.SetGender(closetGender);

        if (closetGender == User.Gender.Male)
            SetClosetImages(GetDisplayedOutfits(outfits_m, outfitStartIdx_m));
        else if (closetGender == User.Gender.Female)
            SetClosetImages(GetDisplayedOutfits(outfits_f, outfitStartIdx_f));

        if (!isTutorialFinished)
            selectionTutorial.StartTutorial();
        else
            selectionTutorial.EndTutorial(true);
    }

    public void ActivateCloset ()
    {
        Show();
    }

    public void SwapClosetGender()
    {
        if (closetGender == User.Gender.Male)
            closetGender = User.Gender.Female;
        else if (closetGender == User.Gender.Female)
            closetGender = User.Gender.Male;

        genderSwapButton.SetGender(closetGender);

        if (closetGender == User.Gender.Male)
            SetClosetImages(GetDisplayedOutfits(outfits_m, outfitStartIdx_m));
        else if (closetGender == User.Gender.Female)
            SetClosetImages(GetDisplayedOutfits(outfits_f, outfitStartIdx_f));
    }

    public void ResetCloset()
    {
        ClearClosetImage();
        ownerIdx = -1;
        closetGender = User.Gender.None;
        outfits_m = null;
        outfits_f = null;
        outfitStartIdx_m = 0;
        outfitStartIdx_f = 0;
        lastSelectedOutfit = null;
    }

    public void Clear(bool resetPos = false)
    {
        selectionTutorial.ResetTutorial();
        if(resetPos)
            rectTrans.anchoredPosition = hiddenPos;
        isHidden = true;
        isHiding = false;
        isShowing = false;
        idleElapsedTime = 0f;

        ClearClosetImage();
        ownerIdx = -1;
        closetGender = User.Gender.None;
        if(outfits_m != null)
            outfits_m.Clear();
        if (outfits_f != null)
            outfits_f.Clear();
        outfitStartIdx_m = 0;
        outfitStartIdx_f = 0;
        lastSelectedOutfit = null;
        isActive = false;
        activateIcon.gameObject.SetActive(false);
    }

    public void PageUp()
    {
        for (int i = 0; i < ClosetManager.NUMBER_CLOSET_ITEMS; i++)
        {
            if (closetGender == User.Gender.Male)
            {
                outfitStartIdx_m--;
                if (outfitStartIdx_m < 0)
                    outfitStartIdx_m = outfits_m.Count - 1;

                SetClosetImages(GetDisplayedOutfits(outfits_m, outfitStartIdx_m));
            }
            else if (closetGender == User.Gender.Female)
            {
                outfitStartIdx_f--;
                if (outfitStartIdx_f < 0)
                    outfitStartIdx_f = outfits_f.Count - 1;

                SetClosetImages(GetDisplayedOutfits(outfits_f, outfitStartIdx_f));
            }
        }
    }

    public void PageDown()
    {
        for(int i=0; i<ClosetManager.NUMBER_CLOSET_ITEMS; i++)
        {
            if (closetGender == User.Gender.Male)
            {
                outfitStartIdx_m++;
                if (outfitStartIdx_m >= outfits_m.Count)
                    outfitStartIdx_m = 0;

                SetClosetImages(GetDisplayedOutfits(outfits_m, outfitStartIdx_m));
            }
            else if(closetGender == User.Gender.Female)
            {
                outfitStartIdx_f++;
                if (outfitStartIdx_f >= outfits_f.Count)
                    outfitStartIdx_f = 0;

                SetClosetImages(GetDisplayedOutfits(outfits_f, outfitStartIdx_f));
            }
        }
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
        Debug.Log(string.Format("[Closet] OnClosetItemHover: hoveredItem = " + hoveredItem.name));

        if(hoveredItem == genderSwapButton)
        {
            genderSwapButton.OnItemHover();
            topArrow.OnItemUnselected();
            bottomArrow.OnItemUnselected();
            foreach (ClosetOutfitItem outfitItem in OutfitItems)
            {
                outfitItem.OnItemUnselected();
            }
        }
        else if(hoveredItem == topArrow)
        {
            genderSwapButton.OnItemUnselected();
            topArrow.OnItemHover();
            bottomArrow.OnItemUnselected();
            foreach (ClosetOutfitItem outfitItem in OutfitItems)
            {
                outfitItem.OnItemUnselected();
            }
        }
        else if(hoveredItem == bottomArrow)
        {
            genderSwapButton.OnItemUnselected();
            topArrow.OnItemUnselected();
            bottomArrow.OnItemHover();
            foreach (ClosetOutfitItem outfitItem in OutfitItems)
            {
                outfitItem.OnItemUnselected();
            }
        }
        else
        {
            genderSwapButton.OnItemUnselected();
            topArrow.OnItemUnselected();
            bottomArrow.OnItemUnselected();
            ClosetOutfitItem hoveredOutfit = hoveredItem as ClosetOutfitItem;
            foreach (ClosetOutfitItem outfitItem in OutfitItems)
            {
                if (hoveredOutfit == outfitItem)
                    outfitItem.OnItemHover();
                else
                    outfitItem.OnItemUnselected();
            }
        }

        idleElapsedTime = 0f;
    }

    private void OnUnselectAll()
    {
        genderSwapButton.OnItemUnselected();
        topArrow.OnItemUnselected();
        bottomArrow.OnItemUnselected();
        foreach (ClosetOutfitItem outfitItem in OutfitItems)
        {
            outfitItem.OnItemUnselected();
        }

        if(!isHidden && !isHiding)
            idleElapsedTime += Time.deltaTime;
    }

    private int GetOutfitIndexByAge(List<Outfit> of, int age)
    {
        if (of.Count > 0) {
            int youngestOutfitAge = of[0].age;
            int oldestOutfitAge = of[of.Count - 1].age;
            for (int i = 0; i < of.Count; i++)
            {
                if (age < youngestOutfitAge)
                {
                    if (of[i].age == youngestOutfitAge)
                        return i;
                }
                else if (age > oldestOutfitAge)
                {
                    if (of[i].age == oldestOutfitAge)
                        return i;
                }
                else if (age / 10 * 10 == of[i].age)
                {
                    return i;
                }
            }
        }
        return -1;
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

    void GetUserPointingScreenPoints(Side closetSide, long ownerID, ref Vector3 from, ref Vector3 to, ref Vector3 spineShoulder)
    {
        Vector2 wristPos2D;
        Vector2 elbowPos2D;
        Vector2 spineShoulder2D;
        Vector3 wristPos;
        Vector3 elbowPos;
        if (closetSide == Closet.Side.Left)
        {
            wristPos = kinect.GetJointPosition(ownerID, (int)KinectInterop.JointType.WristLeft);
            elbowPos = kinect.GetJointPosition(ownerID, (int)KinectInterop.JointType.ElbowLeft);
            wristPos2D = jointCamera.WorldToScreenPoint(wristPos);
            elbowPos2D = jointCamera.WorldToScreenPoint(elbowPos);
        }
        else // (closetSide == Closet.Side.Right)
        {
            wristPos = kinect.GetJointPosition(ownerID, (int)KinectInterop.JointType.WristRight);
            elbowPos = kinect.GetJointPosition(ownerID, (int)KinectInterop.JointType.ElbowRight);
            wristPos2D = jointCamera.WorldToScreenPoint(wristPos);
            elbowPos2D = jointCamera.WorldToScreenPoint(elbowPos);
        }
        Vector3 spineShoulderPos = kinect.GetJointPosition(ownerID, (int)KinectInterop.JointType.SpineMid);
        spineShoulder2D = jointCamera.WorldToScreenPoint(spineShoulderPos);

        if (wristPos != Vector3.zero)
            to = wristPos2D;
        if (elbowPos != Vector3.zero)
            from = elbowPos2D;
        if (spineShoulderPos != Vector3.zero)
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