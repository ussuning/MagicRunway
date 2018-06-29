using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    public GameObject uiStartMenu;
    public GameObject uiShowcase;
    public GameObject uiDemo;
    public GameObject uiLive;
    public GameObject uiCollection;
    public GameObject uiUpNext;

    private CanvasFader faderStartMenu;

    public void Start()
    {
        uiStartMenu.SetActive(false);
        //uiShowcase.SetActive(false);
        uiDemo.SetActive(false);
        uiLive.SetActive(false);

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
        if(animate == true) {
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

    public void HideOutfit() {
        Showcase showcase = uiShowcase.GetComponent<Showcase>();
        showcase.Hide(true);
    }
   
    //----------------------------------------
    // Collection
    //----------------------------------------

    public void ShowCollection(Collection collection)
    {
        CollectionDisplay cd = uiCollection.GetComponent<CollectionDisplay>();
        cd.ShowCollection(collection);
    }

    public void RunUpNextTimer(string collectionName, float totalTimeSeconds = 10.0f, float warningTimeSeconds = 5.0f) {
        UpNext un = uiUpNext.GetComponent<UpNext>();
        un.StartUpNext(collectionName, totalTimeSeconds, warningTimeSeconds);
    }
    //----------------------------------------

    void UIEvents_CanvasFadeComplete(GameObject go, CanvasFade fade)
    {
        if (go == uiStartMenu)
        {
            if (fade == CanvasFade.OUT) { uiStartMenu.SetActive(false); }
        }
    }
}