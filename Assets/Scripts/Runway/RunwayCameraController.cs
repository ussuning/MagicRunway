using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AutoRunwayCamera { MAIN, SLOW_MO };
public enum AutoRunwayCameraFollowState { FOLLOW, UNFOLLOW, NONE}
public enum AutoRunwayCameraSlowMo { SLOW, VERY_SLOW, SUPER_SLOW }

public class RunwayCameraController : MonoBehaviour {
 
    public ColliderEvents RunwayEnterEvents;
    public ColliderEvents RunwayMidEvents;
    public ColliderEvents RunwayEndEvents;
    public ColliderEvents RunwayExitEvents;

    public Transform camLookAt;
    public Camera MainCamera;
    public Camera SlowMoCamera;
    //public GameObject MidFlashes;
    //public GameObject FrontFlashes;
    public AutoRunwayCamera curCamera = AutoRunwayCamera.MAIN;
    public AutoRunwayCameraFollowState curCamFollowState = AutoRunwayCameraFollowState.NONE;

    public float mainCamMinFOV = 20f;
    public float targetHeightPercent = 0.65f;
    public float fovSpeed = 60f; // fov/sec 

    protected Camera activeCam;

    protected float timeUntilEndFrontFlashes = 0f;

    internal ColliderInfoMap modelsInMidZone = new ColliderInfoMap();
    internal ColliderInfoMap modelsOnRunway = new ColliderInfoMap();

    protected float timeSinceSlowMo;

    private float mainCamOriginFOV = 60f;
    private float slowMoCamOriginFOV = 13f;
    private Vector3 mainCamOriginRot;
    private Vector3 slowMoCamOriginRot;

    //----------------------------------------
    // MonoBehaviour Overrides
    //----------------------------------------

    void Awake() {
        mainCamOriginRot = new Vector3(MainCamera.transform.eulerAngles.x, MainCamera.transform.eulerAngles.y,
            MainCamera.transform.eulerAngles.z);
        slowMoCamOriginRot = new Vector3(SlowMoCamera.transform.eulerAngles.x, SlowMoCamera.transform.eulerAngles.y,
            SlowMoCamera.transform.eulerAngles.z);

        mainCamOriginFOV = MainCamera.fieldOfView;
        slowMoCamOriginFOV = SlowMoCamera.fieldOfView;
    }

    void OnEnable()
    {
        Init();
    }

    void OnDisable()
    {
        modelsInMidZone.active.Clear();
        modelsInMidZone.history.Clear();
        modelsOnRunway.active.Clear();
        modelsOnRunway.history.Clear();

        RemoveAllListeners();
    }

    void OnDestroy()
    {
        RemoveAllListeners();
    }

    void Update()
    {
        if (curCamFollowState == AutoRunwayCameraFollowState.NONE) { return; }

        if (curCamFollowState == AutoRunwayCameraFollowState.FOLLOW)
        {
            UpdateFollowCam();
            return;
        }

        if (curCamFollowState == AutoRunwayCameraFollowState.UNFOLLOW)
        {
            return;
        }
    }


    private void Init() {
        AddAllListeners();
        CamerasReset();
        SelectCamera(AutoRunwayCamera.MAIN);
        
        /*
        if (MidFlashes == null)
            MidFlashes = GameObject.Find("MidFlashes");
        if (FrontFlashes == null)
            FrontFlashes = GameObject.Find("FrontFlashes");
            */

        //EnableMidFlashes(false);
        //EnableFrontFlashes(false);
	}

    private void SlowMo(AutoRunwayCameraSlowMo speed)
    {
        curCamFollowState = AutoRunwayCameraFollowState.FOLLOW;

        if (speed == AutoRunwayCameraSlowMo.VERY_SLOW)
        {
            Time.timeScale = 0.5f;
        }
        else if (speed == AutoRunwayCameraSlowMo.SUPER_SLOW)
        {
            Time.timeScale = 0.25f;
        }
        else
        {
            Time.timeScale = 0.95f;
        }
        
        timeSinceSlowMo = 0.0f;
        SelectCamera(AutoRunwayCamera.SLOW_MO);
    }

    private void UpdateFollowCam()
    {
        if (modelsOnRunway.active.Count == 0) { return; }

        Bounds b = modelsOnRunway.active[0].bounds;

        if (curCamera == AutoRunwayCamera.SLOW_MO)
        {
            timeSinceSlowMo += Time.unscaledDeltaTime;
            float t = (Mathf.Cos(timeSinceSlowMo / 2.0f) + 1.0f) / 2.0f;

            Vector3 max = new Vector3(b.center.x, Mathf.Lerp(b.center.y, b.max.y, 0.15f), b.center.z);
            Vector3 min = new Vector3(b.center.x, Mathf.Lerp(b.center.y, b.min.y, 0.1f), b.center.z);

            camLookAt.position = Vector3.Lerp(min, max, t);
        }
        else
        {
            camLookAt.position = b.center;
        }

        UpdateMainCameraZoom(b);
        activeCam.transform.LookAt(camLookAt);
    }
   
    private void UpdateMainCameraZoom(Bounds b) {
        Vector3 max = new Vector3(b.center.x, b.max.y, b.center.z);
        Vector3 min = new Vector3(b.center.x, b.min.y, b.center.z);
        Vector2 max2D = MainCamera.WorldToScreenPoint(max);
        Vector2 min2D = MainCamera.WorldToScreenPoint(min);
        float height = max2D.y - min2D.y;
        float heightPercent = height / Screen.height;
        float fovDelta = (heightPercent - targetHeightPercent) * (fovSpeed * Time.deltaTime);
        MainCamera.fieldOfView += fovDelta;
        MainCamera.fieldOfView = Mathf.Clamp(MainCamera.fieldOfView, mainCamMinFOV, float.MaxValue);
    }

    private void UpdateMainCameraToOrigin()
    {
        MainCamera.transform.eulerAngles = Vector3.Lerp(MainCamera.transform.eulerAngles, mainCamOriginRot, 3.0f * Time.deltaTime);
        MainCamera.fieldOfView = Mathf.Lerp(MainCamera.fieldOfView, mainCamOriginFOV, 2.0f * Time.deltaTime);
    }


    /*
    private void EnableMidFlashes(bool enable) {
        MidFlashes.gameObject.SetActive(enable);
    }

    private void EnableFrontFlashes(bool enable)
    {
        FrontFlashes.gameObject.SetActive(enable);
    }
    */


    private void CamerasReset()
    {
        SelectCamera(AutoRunwayCamera.MAIN);

        MainCamera.fieldOfView = mainCamOriginFOV;
        MainCamera.transform.eulerAngles = new Vector3(mainCamOriginRot.x, mainCamOriginRot.y, mainCamOriginRot.z);

        SlowMoCamera.transform.eulerAngles = new Vector3(slowMoCamOriginRot.x, slowMoCamOriginRot.y, slowMoCamOriginRot.z);

        curCamFollowState = AutoRunwayCameraFollowState.NONE;
    }

    private void SelectCamera(AutoRunwayCamera arc)
    {
        curCamera = arc;

        if(arc == AutoRunwayCamera.MAIN)
        {
            Time.timeScale = 1.0f;
            MainCamera.enabled = true;
            SlowMoCamera.enabled = false;
            activeCam = MainCamera;
        }
        else if(arc == AutoRunwayCamera.SLOW_MO)
        {
            MainCamera.enabled = false;
            SlowMoCamera.enabled = true;
            activeCam = SlowMoCamera;
        }
        else
        {
            MainCamera.enabled = true;
            SlowMoCamera.enabled = false;
            activeCam = MainCamera;
        }
    }

    //----------------------------------------
    // Event Callbacks
    //----------------------------------------

    private void OnRunwayEnter(Collider model)
    {
        Debug.Log("HERE COMES A NEW CHALLENGER!");
        if (modelsOnRunway.history.ContainsKey(model) == false)
        {
            modelsOnRunway.history.Add(model, Time.unscaledTime);
        }

        modelsOnRunway.active.Add(model);
        curCamFollowState = AutoRunwayCameraFollowState.FOLLOW;
    }

    private void OnRunwayMidEnter(Collider model)
    {
        if (modelsInMidZone.history.ContainsKey(model) == false)
        {
            modelsInMidZone.history.Add(model, Time.unscaledTime);
            SlowMo(AutoRunwayCameraSlowMo.VERY_SLOW);
        } else
        {
            modelsInMidZone.active.Add(model);
            SlowMo(AutoRunwayCameraSlowMo.SLOW);
        }
    }

    private void OnRunwayMidExit(Collider model)
    {
        Debug.Log("EXIT");
        CamerasReset();

        //Model Exiting Mid Zone 2nd time
        if (modelsInMidZone.active.Count > 0)
        {
            modelsInMidZone.active.RemoveAt(0);

            PurgeModel(model);
        }
    }

    private void OnRunwayEndEnter(Collider model)
    {
        timeUntilEndFrontFlashes = 2.5f;
    }

    private void OnRunwayExit(Collider model)
    {
        PurgeModel(model);
    }

    private void PurgeModel(Collider model)
    {
        if (modelsOnRunway.active.Contains(model))
            modelsOnRunway.active.Remove(model);
        if (modelsOnRunway.history.ContainsKey(model))
            modelsOnRunway.history.Remove(model);
        if (modelsInMidZone.history.ContainsKey(model))
            modelsInMidZone.history.Remove(model);
    }

    private void OnFinaleStart(Collection collection)
    {
        //Debug.Log("****************************** FINALE START!!");
        RemoveRunwayListeners();
    }

    private void OnFinaleEnd(Collection collection)
    {
        //Debug.Log("****************************** FINALE END!!");
        AddRunwayListeners();
    }

    //----------------------------------------
    // Event Listeners
    //----------------------------------------

    private void AddRunwayListeners()
    {
        RemoveRunwayListeners();

        if (RunwayEnterEvents == null)
            RunwayEnterEvents = GameObject.Find("RunwayEnter")?.GetComponent<ColliderEvents>();
        if (RunwayMidEvents == null)
            RunwayMidEvents = GameObject.Find("RunwayMid")?.GetComponent<ColliderEvents>();
        if (RunwayEndEvents == null)
            RunwayEndEvents = GameObject.Find("RunwayEnd")?.GetComponent<ColliderEvents>();
        if (RunwayExitEvents == null)
            RunwayExitEvents = GameObject.Find("RunwayExit")?.GetComponent<ColliderEvents>();

        RunwayMidEvents.OnTriggerEnterEvt += OnRunwayMidEnter;
        RunwayMidEvents.OnTriggerExitEvt += OnRunwayMidExit;

        RunwayEnterEvents.OnTriggerEnterEvt += OnRunwayEnter;
        RunwayExitEvents.OnTriggerEnterEvt += OnRunwayExit;

        RunwayEndEvents.OnTriggerEnterEvt += OnRunwayEndEnter;
    }

    private void AddAutoRunwayListeners()
    {
        RemoveAutoRunwayListeners();

        AutoRunwayEvents.OnFinaleStartCallback += OnFinaleStart;
        AutoRunwayEvents.OnFinaleEndCallback += OnFinaleEnd;
    }

    private void RemoveRunwayListeners()
    {
        RunwayMidEvents.OnTriggerEnterEvt -= OnRunwayMidEnter;
        RunwayMidEvents.OnTriggerExitEvt -= OnRunwayMidExit;

        RunwayEnterEvents.OnTriggerEnterEvt -= OnRunwayEnter;
        RunwayExitEvents.OnTriggerEnterEvt -= OnRunwayExit;

        RunwayEndEvents.OnTriggerEnterEvt -= OnRunwayEndEnter;
    }

    private void RemoveAutoRunwayListeners()
    {
        AutoRunwayEvents.OnFinaleStartCallback -= OnFinaleStart;
        AutoRunwayEvents.OnFinaleEndCallback -= OnFinaleEnd;
    }

    private void AddAllListeners()
    {
        AddRunwayListeners();
        AddAutoRunwayListeners();
    }

    private void RemoveAllListeners()
    {
        RemoveRunwayListeners();
        RemoveAutoRunwayListeners();
    }
}

public class ColliderInfoMap {
    public Dictionary<Collider, float> history = new Dictionary<Collider, float>();
    public List<Collider> active = new List<Collider>();
}
