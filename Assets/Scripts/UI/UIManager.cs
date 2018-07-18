using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    public GameObject uiStartMenu;
    public GameObject uiShowcase;
    public GameObject uiCollection;
    public GameObject uiUpNext;
    public GameObject uiGestureGender;

    protected GameObject uiMaleGender;
    protected GameObject uiFemaleGender;

    private CanvasFader faderStartMenu;
    private IEnumerator gestureGenderCoroutine;

    public void Start()
    {
        uiStartMenu.SetActive(false);
        //uiShowcase.SetActive(false);
        uiGestureGender.SetActive(false);

        uiMaleGender = uiGestureGender.transform.Find("Male").gameObject;
        uiFemaleGender = uiGestureGender.transform.Find("Female").gameObject;

        faderStartMenu = uiStartMenu.GetComponent<CanvasFader>();

        UIEvents.OnCanvaseFadeCompleteCallback += UIEvents_CanvasFadeComplete;
    }

    //----------------------------------------

    public void OnClickShow()
    {
        ShowStartMenu(true);
    }

    public void OnClickHide()
    {
        faderStartMenu.StartFading(CanvasFade.OUT);
    }

    //----------------------------------------
    // Start Menu
    //----------------------------------------

    public void ShowStartMenu(bool animate) {
        if (animate == true) {
            uiStartMenu.SetActive(true);
            faderStartMenu.StartFading(CanvasFade.IN);
        } else {
            uiStartMenu.SetActive(true);
        }
    }

    public void HideStartMenu(bool animate) {
        if (animate == true)
        {
            faderStartMenu.StartFading(CanvasFade.OUT);
        }
        else
        {
            uiStartMenu.SetActive(false);
        }
    }
    
    public void HideAll()
    {
        HideOutfit(false);
        HideCollection(false);
        HideUpNext();
    }
    //----------------------------------------
    // Showcase
    //----------------------------------------

    public void OnClickShowShowcase() {
        ShowOutfit(MRData.Instance.outfits.outfits[0]);
    }

    public void ShowOutfit(Outfit outfit) {
        Showcase showcase = uiShowcase.GetComponent<Showcase>();
        showcase.Show(outfit);
    }

    public void HideOutfit(bool animate = true) {
        Showcase showcase = uiShowcase.GetComponent<Showcase>();
        showcase.Hide(animate);
    }

    //----------------------------------------
    // Collection
    //----------------------------------------

    public void ShowCollection(Collection collection)
    {
        CollectionDisplay cd = uiCollection.GetComponent<CollectionDisplay>();
        cd.ShowCollection(collection);
    }

    public void HideCollection(bool animate = true)
    {
        CollectionDisplay cd = uiCollection.GetComponent<CollectionDisplay>();
        cd.Hide(animate);
    }

    public void RunUpNextTimer(string collectionName, float totalTimeSeconds = 10.0f, float warningTimeSeconds = 5.0f) {
        uiUpNext.SetActive(true);
        UpNext un = uiUpNext.GetComponent<UpNext>();
        un.StartUpNext(collectionName, totalTimeSeconds, warningTimeSeconds);
    }

    public void ShowUpNext(Collection collection)
    {
        uiUpNext.SetActive(true);
        UpNext un = uiUpNext.GetComponent<UpNext>();
        un.StartUpNext(collection.name);
    }

    public void HideUpNext()
    {
        uiUpNext.SetActive(false);
    }

    //----------------------------------------
    // Gestures
    //----------------------------------------

    public void ShowGestureGender(float time = 30.0f)
    {
        if(gestureGenderCoroutine != null)
        {
            StopCoroutine(gestureGenderCoroutine); 
        }

        CanvasFader cf = uiGestureGender.GetComponent<CanvasFader>();

        uiGestureGender.SetActive(true);
        uiMaleGender.SetActive(true);
        uiFemaleGender.SetActive(true);

        cf.StartFading(CanvasFade.IN);

        gestureGenderCoroutine = WaitToCloseGender(time);
        StartCoroutine(gestureGenderCoroutine);
    }

    public void ShowMaleGender(float time = 30.0f)
    {
        uiMaleGender.SetActive(true);
        uiFemaleGender.SetActive(false);
    }

    public void ShowFemaleGender(float time = 30.0f)
    {
        uiMaleGender.SetActive(false);
        uiFemaleGender.SetActive(true);
    }

    IEnumerator WaitToCloseGender(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideGestureGender();
    }

    public void HideGestureGender(bool animate = true)
    {
        CanvasFader cf = uiGestureGender.GetComponent<CanvasFader>();
        cf.StartFading(CanvasFade.OUT);
        gestureGenderCoroutine = null;
    }

    //----------------------------------------

    void UIEvents_CanvasFadeComplete(GameObject go, CanvasFade fade)
    {
        if (go == uiStartMenu)
        {
            if (fade == CanvasFade.OUT) { uiStartMenu.SetActive(false); }
        }

        if (go == uiGestureGender)
        {
            if (fade == CanvasFade.OUT) { uiGestureGender.SetActive(false); }
        }
    }
}