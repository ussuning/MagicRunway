using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public float shiftOutSpeed = 10f;
    public float shiftInSpeed = 20f;

    public float idolTime = 5f;

    public Camera cam;
    protected Canvas canvas;

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

        canvas = GetComponentInParent<Canvas>();
    }

    public RectTransform pointFrom;
    public RectTransform pointTo;

    public Vector3 ptFrom;
    public Vector3 ptTo;

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
            if (!isHidden)
            {
                if (kinect && kinect.IsInitialized())
                {
                    if (kinect.IsUserTracked(OwnerID))
                    {
                        Vector3 fromScreenPt;
                        Vector3 toScreenPt;
                        GetUserPointingScreenPoints(ClosetSide, OwnerID, out fromScreenPt, out toScreenPt);

                        Vector2 fromLocal = ScreenPtToCanvasPt(fromScreenPt);
                        if (pointFrom != null)
                            pointFrom.position = fromLocal;
                        Vector2 toLocal = ScreenPtToCanvasPt(toScreenPt);
                        if (pointTo != null)
                            pointTo.position = toLocal;

                        ptFrom = pointFrom.transform.position;
                        ptTo = pointTo.transform.position;
                        
                        //Vector2 directionLocal;
                        //RectTransformUtility.ScreenPointToLocalPointInRectangle(imgRectT, ray2D.origin,    cam, out originLocal);
                        //RectTransformUtility.ScreenPointToLocalPointInRectangle(imgRectT, ray2D.direction, cam, out directionLocal);
                        //Ray2D rayLocal = new Ray2D(originLocal, directionLocal);

                        RaycastHit2D hit = Physics2D.Raycast(ptFrom, ptTo-ptFrom, float.MaxValue, LayerMask.GetMask(new string []{ "Pointable2D" }));

                        Debug.Log("hit = " + (hit.collider == null ? "null" : hit.collider.name));

                        if (hit.collider != null)
                        {
                            ClosetItem closetItem = hit.collider.GetComponent<ClosetItem>();
                            OnClosetItemHover(closetItem);
                        }
                        else
                        {
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
    }

    public void Show()
    {
        if(isHidden)
            isShowing = true;
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

    private void OnClosetItemHover(ClosetItem hoveredItem)
    {
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


    void GetUserPointingScreenPoints(Side closetSide, long ownerID, out Vector3 from, out Vector3 to)
    {
        Vector2 wristPos2D;
        Vector2 elbowPos2D;
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
        to = wristPos2D;
        from = elbowPos2D;
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