using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public GameObject uiStartMenu;
    //public GameObject uiInventory;
    public GameObject uiJoinIn;
    public GameObject uiShowcase;
    public GameObject uiCollectionTitle;
    public GameObject uiUpNext;
    //public GameObject uiGestureGender;
    //public GameObject uiHandCursor;
    //public GameObject uiHandCursor2;
    //public GameObject uiStickMan;

    public GameObject uiSettingMenu;

    // Inventory Menu
    //public float inventoryScrollSize = 424;
    //public GameObject scrollRectGO;
    //public GameObject contentPanelGO;
    //public GameObject controlPanelGO;
    //public GameObject scrollRectGO2;
    //public GameObject contentPanelGO2;
    //public GameObject controlPanelGO2;
    //public GameObject inventoryMaleGO;
    //public GameObject inventoryMaleGO2;
    //public GameObject inventoryFemaleGO;
    //public GameObject inventoryFemaleGO2;

    //protected GameObject uiMaleGender;
    //protected GameObject uiFemaleGender;
    protected Button uiJoinInButton;

    private CanvasFader faderStartMenu;
    private CanvasFader faderInventory;
    private CanvasFader faderControl;
    private CanvasFader faderControl2;
    private CanvasFader faderStickMan;
    private CanvasFader faderCollectionTitle;
    private IEnumerator gestureGenderCoroutine;
    private IEnumerator stickManCoroutine;

    void OnEnable()
    {
        uiJoinInButton = uiJoinIn.GetComponent<Button>(); 
        uiJoinInButton.onClick.AddListener(() => UI_JoinInButtonCallBack(uiJoinInButton));

    }

    public void Start()
    {
        //uiShowcase.SetActive(false);
        //uiGestureGender.SetActive(false);
        uiStartMenu.SetActive(false);
        uiSettingMenu.SetActive(false);
        //uiMaleGender = uiGestureGender.transform.Find("Male").gameObject;
        //uiFemaleGender = uiGestureGender.transform.Find("Female").gameObject;
        faderStartMenu = uiStartMenu.GetComponent<CanvasFader>();
        //faderInventory = uiInventory.GetComponent<CanvasFader>();
        //faderStickMan = uiStickMan.GetComponent<CanvasFader>();
        //faderControl = controlPanelGO.GetComponent<CanvasFader>();
        //faderControl2 = controlPanelGO2.GetComponent<CanvasFader>();
        faderCollectionTitle = uiCollectionTitle.GetComponent<CanvasFader>();

        UIEvents.OnCanvaseFadeCompleteCallback += UIEvents_CanvasFadeComplete;
    }

    void Update()
    {
        if (AppManager.Instance.GetRunwayMode() == Mode.LIVE)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (uiSettingMenu.activeSelf)
                    uiSettingMenu.SetActive(false);
                else
                    uiSettingMenu.SetActive(true);
            }
        }
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

    public void HideForNextCollection()
    {
        HideUpNext();
        //HideStartMenu(false);
        //HideInventory(false);
    }
        
    //----------------------------------------
    // Start Menu
    //----------------------------------------

    public void ShowStartMenu(bool animate) {
        if (animate == true) {
            uiStartMenu.SetActive(true);
            faderStartMenu.StartFading(CanvasFade.IN);
        } else {
            CanvasGroup canvasGroup = uiStartMenu.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1;
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
    // Collection Title
    //----------------------------------------

    public void ShowCollectionTitle(string title, bool animate)
    {
        foreach (Text text in uiCollectionTitle.GetComponentsInChildren<Text>())
        {
            text.text = title;
        }
        if (animate == true)
        {
            uiCollectionTitle.SetActive(true);
            faderCollectionTitle.StartFading(CanvasFade.IN);
        }
        else
        {
            uiCollectionTitle.SetActive(true);
        }
    }

    public void HideCollectionTitle(bool animate)
    {
        if (animate == true)
        {
            faderCollectionTitle.StartFading(CanvasFade.OUT);
        }
        else
        {
            uiCollectionTitle.SetActive(false);
        }
    }

    //----------------------------------------
    // Inventory 
    //----------------------------------------

    //public void ShowInventory(bool animate)
    //{
    //    if (animate == true)
    //    {
    //        uiInventory.SetActive(true);
    //        faderInventory.StartFading(CanvasFade.IN);
    //    }
    //    else
    //    {
    //        uiInventory.SetActive(true);
    //    }
    //}

    //public void HideInventory(bool animate)
    //{
    //    if (animate == true)
    //    {
    //        faderInventory.StartFading(CanvasFade.OUT);
    //    }
    //    else
    //    {
    //        uiInventory.SetActive(false);
    //    }
    //}

    //public void ShowControlPanel(int index)
    //{
    //    if(index == 0)
    //    {
    //        controlPanelGO.SetActive(true);
    //        faderControl.StartFading(CanvasFade.IN);
    //    }
    //    else if(index == 1)
    //    {
    //        controlPanelGO2.SetActive(true);
    //        faderControl2.StartFading(CanvasFade.IN);
    //    }
    //}

    //public void HideControlPanel(int index)
    //{
    //    if (index == 0)
    //    {
    //        controlPanelGO.SetActive(false);
    //        faderControl.StartFading(CanvasFade.OUT);
    //    }
    //    else if (index == 1)
    //    {
    //        controlPanelGO2.SetActive(false);
    //        faderControl2.StartFading(CanvasFade.OUT);
    //    }
    //}


    //public IEnumerator scrollInventory(int userIndex, long userId, string dir)
    //{
    //    Debug.Log("scrollInventory " + userIndex + " " + dir);
    //    RectTransform rt;
    //    ScrollRect scrollRect;
    //    RectTransform contentPanel;
    //    GameObject controlGO;
    //    GameObject control;

    //    if (userIndex == 0 )
    //    {
    //        rt = scrollRectGO.GetComponent<RectTransform>();
    //        scrollRect = scrollRectGO.GetComponent<ScrollRect>();
    //        contentPanel = contentPanelGO.GetComponent<RectTransform>();
    //        controlGO = controlPanelGO;
    //    }
    //    else
    //    {
    //        rt = scrollRectGO2.GetComponent<RectTransform>();
    //        scrollRect = scrollRectGO2.GetComponent<ScrollRect>();
    //        contentPanel = contentPanelGO2.GetComponent<RectTransform>();
    //        controlGO = controlPanelGO2;
    //    }

    //    int currentSlot = UserManager.Instance.getUserById(userId).getInventorySlot();
       
    //    Debug.Log("Current Slot = " + UserManager.Instance.getUserById(userId).getInventorySlot());

    //    Vector3 pos = rt.position;
    //    Canvas.ForceUpdateCanvases();
    //    string controlName;
    //    int nextSlot;
    //    if (dir == "up")
    //    {
    //        nextSlot = UserManager.Instance.getUserById(userId).getInventorySlot() - 1;
    //        UserManager.Instance.getUserById(userId).setInventorySlot(nextSlot);
    //        controlName = "Menu_Up_" + userIndex;
    //        control = controlGO.transform.Find(controlName).gameObject;
    //        InventoryControlSelected(control);
    //        contentPanel.anchoredPosition = contentPanel.anchoredPosition - new Vector2(0, inventoryScrollSize);
    //    }
    //    else if(dir == "down")
    //    {
    //        nextSlot = UserManager.Instance.getUserById(userId).getInventorySlot() + 1;
    //        UserManager.Instance.getUserById(userId).setInventorySlot(nextSlot);
    //        controlName = "Menu_Down_" + userIndex;
    //        control = controlGO.transform.Find(controlName).gameObject;
    //        InventoryControlSelected(control);
    //        contentPanel.anchoredPosition = contentPanel.anchoredPosition + new Vector2(0, inventoryScrollSize);
    //    }

    //    //Debug.Log("Next Slot = " + UserManager.Instance.getUserById(userId).getInventorySlot());

    //    yield return null;
    //}

    public void ClickStartMenu()
    {
        GameObject JoinInButton = uiStartMenu.transform.Find("JoinInButton").gameObject;
        Button StartMenuButton = JoinInButton.GetComponent<Button>();
        StartMenuButton.onClick.Invoke();
    }

    //public void ShowStickManDelay(float time = 30.0f)
    //{
    //    stickManCoroutine = WaitToShowStickMan(time);
    //    StartCoroutine(stickManCoroutine);
    //}

    //public void ShowStickMan(bool animate)
    //{
    //    if (animate == true)
    //    {
    //        uiStickMan.SetActive(true);
    //        if(faderStickMan)
    //            faderStickMan.StartFading(CanvasFade.IN);
    //    }
    //    else
    //    {
    //        uiStickMan.SetActive(true);
    //    }
    //}

    //public void HideStickMan(bool animate)
    //{
    //    if (animate == true)
    //    {
    //        if(faderStickMan)
    //            faderStickMan.StartFading(CanvasFade.OUT);
    //    }
    //    else
    //    {
    //        uiStickMan.SetActive(false);
    //    }
    //}

        /*
    public void ShowHandCursor()
    {
        uiHandCursor.SetActive(true);
        uiHandCursor2.SetActive(true);
    }

    public void HideHandCursor()
    {
        uiHandCursor.SetActive(false);
        uiHandCursor2.SetActive(false);
    }
    */
    public void HideAll()
    {
        HideOutfit(false);
        //  HideCollection(false);    // turn off to use for player UI
        //HideInventory(false);
        HideUpNext();
        //HideGestureGender(false);
        //HideHandCursor();
        HideStartMenu(false);
        //HideStickMan(false);
        HideCollectionTitle(false);
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
    /*
    public void ShowCollection(Collection collection)
    {
        // turn off to use for player UI
       // CollectionDisplay cd = uiCollection.GetComponent<CollectionDisplay>();
       // cd.ShowCollection(collection);
    }

    public void HideCollection(bool animate = true)
    {
          CollectionDisplay cd = uiCollection.GetComponent<CollectionDisplay>();
          cd.Hide(animate);
    }
    */
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

    //public void ShowGestureGender(float time = 30.0f)
    //{
    //    if(gestureGenderCoroutine != null)
    //    {
    //        StopCoroutine(gestureGenderCoroutine); 
    //    }

    //    CanvasFader cf = uiGestureGender.GetComponent<CanvasFader>();

    //    uiGestureGender.SetActive(true);
    //    uiMaleGender.SetActive(true);
    //    uiFemaleGender.SetActive(true);

    //    cf.StartFading(CanvasFade.IN);

    //    gestureGenderCoroutine = WaitToCloseGender(time);
    //    StartCoroutine(gestureGenderCoroutine);
    //}

    //public void ShowMaleGender(int index)
    //{
    //    if(index == 0)
    //    {
    //        inventoryMaleGO.SetActive(true);
    //    }
    //    else if(index == 1)
    //    {
    //        inventoryMaleGO2.SetActive(true);
    //    }
    //    ShowControlPanel(index);
    //}

    //public void ShowFemaleGender(int index)
    //{
    //    if (index == 0)
    //    {
    //        inventoryFemaleGO.SetActive(true);
    //    }
    //    else if (index == 1)
    //    {
    //        inventoryFemaleGO2.SetActive(true);
    //    }
    //    ShowControlPanel(index);
    //}

    //----------------------------------------
    // Inventory
    //----------------------------------------
    public void InventoryControlSelected(GameObject control)
    {
        GameObject selected = control.transform.Find("Selected").gameObject;
        selected.SetActive(true);
        StartCoroutine(InventoryControlUnselected(selected));
    }

    public void SetInventorySlotBorderOff(int userIndex, int slot)
    {
        string inventoryMenuName = "InventoryMenu_" + userIndex;
        string slotName = "slot_" + slot;

        GameObject inventoryGO = GameObject.Find(inventoryMenuName);
        GameObject slotGO = inventoryGO.transform.Find(slotName).gameObject;
        GameObject selected = slotGO.transform.Find("Selected").gameObject;
        selected.SetActive(false);
    }

    IEnumerator InventoryControlUnselected(GameObject selected)
    {
        yield return new WaitForSeconds(0.75f);

        selected.SetActive(false);

        yield return null;
    }

    //IEnumerator WaitToCloseGender(float delay)
    //{
    //    yield return new WaitForSeconds(delay);
    //    HideGestureGender();
    //}

    //IEnumerator WaitToShowStickMan(float delay)
    //{
    //    yield return new WaitForSeconds(delay);
    //    ShowStickMan(true);
    //}

    //public void HideGestureGender(bool animate = true)
    //{
    //    if (gestureGenderCoroutine != null)
    //    {
    //        StopCoroutine(gestureGenderCoroutine);
    //    }

    //    if (animate == true)
    //    {
    //        CanvasFader cf = uiGestureGender.GetComponent<CanvasFader>();
    //        cf.StartFading(CanvasFade.OUT);
    //    } else
    //    {
    //        uiGestureGender.SetActive(false);
    //    }
    //    gestureGenderCoroutine = null;
    //}

    //----------------------------------------

    void UIEvents_CanvasFadeComplete(GameObject go, CanvasFade fade)
    {
        if (go == uiStartMenu)
        {
            if (fade == CanvasFade.OUT) { uiStartMenu.SetActive(false); }
        }

        if (go == uiCollectionTitle)
        {
            if (fade == CanvasFade.OUT) { uiCollectionTitle.SetActive(false); }
        }

        //if (go == uiGestureGender)
        //{
        //    if (fade == CanvasFade.OUT) { uiGestureGender.SetActive(false); }
        //}
    }

    protected void UI_JoinInButtonCallBack(Button buttonPressed)
    {
        if (buttonPressed == uiJoinInButton)
        {
            Debug.Log("Join Button Callback" + uiJoinInButton.name);
        }
    }
}