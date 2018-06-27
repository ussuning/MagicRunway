using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    public GameObject uiStartMenu;
    public GameObject uiShowcase;
    public GameObject uiDemo;
    public GameObject uiLive;

    private CanvasFader faderStartMenu;

    public void Start()
    {
        //uiStartMenu.SetActive(false);
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

    /*
     * - icon
     * - name
     * - category
     * 
     * 
     * 
     */
    public void OnClickShowShowcase() {
        ShowOutfit(MRData.Instance.outfits.outfits[0]);    
    }

    public void ShowOutfit(Outfit outfit) {
        /*
         * If already one open or is opening, hide it first before showing next one
         * show current outfit list
         * 
         *  
         */
        Showcase showcase = uiShowcase.GetComponent<Showcase>();
        showcase.Show(outfit);
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