using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public enum AutoRunwayCamera { MAIN, SLOW_MO };
//public enum AutoRunwayCameraFollowState { FOLLOW, UNFOLLOW, NONE}
public enum AutoRunwayCameraSpeed { NORMAL, SLOW, VERY_SLOW }
public enum AutoRunwayCameraState { DEFAULT, CLOSE_UP, CLOSE_UP_PAN, ZOOM }
public enum AutoRunwayCameraTransition { CUT, SMOOTH, FADE }

public class RunwayCameraController : MonoBehaviour {
 
    public ColliderEvents RunwayEnterEvents;
    public ColliderEvents RunwayMidEvents;
    public ColliderEvents RunwayEndEvents;
    public ColliderEvents RunwayExitEvents;

    public GameObject FlashMid;
    public GameObject FlashFront;
    public Transform camLookAt;
    public Transform camLookAtFinal;
    public AudioSource Flash;

    public PostProcessVolume profile;

    protected Vector3 lastCamLookAtFinalPos = Vector3.zero;

    [Tooltip("Higher value increase camera responsiveness, but may have more jitter. Lower values increase smoothness, but lower lookAt responsiveness.")]
    [Range(0.1f, 10.0f)]
    public float camLerpFactor = 2.0f;

    public Camera MainCamera;
    public Camera SlowMoCamera;
    public Camera VideoCamera;

    public AutoRunwayCamera curCam = AutoRunwayCamera.MAIN;
    //public AutoRunwayCameraFollowState curCamFollowState = AutoRunwayCameraFollowState.NONE;
    public AutoRunwayCameraState curCamState = AutoRunwayCameraState.DEFAULT;
    public AutoRunwayCameraTransition curCamTransition = AutoRunwayCameraTransition.CUT;
    public Image blackout;

    public float mainCamMinFOV = 20f;
    public float targetHeightPercent = 0.65f;
    public float fovSpeed = 60f; // fov/sec 

    protected Camera activeCam;

    internal ColliderInfoMap modelsInMidZone = new ColliderInfoMap();
    internal ColliderInfoMap modelsOnRunway = new ColliderInfoMap();

    protected float timeSinceSlowMo;

    private float mainCamOriginFOV = 60f;
    private float slowMoCamOriginFOV = 13f;
    private float videoCamOriginFOV = 13f;
    private Quaternion mainCamOriginRot;
    private Quaternion slowMoCamOriginRot;

    //private float panStartY = 0.8f;
    private float panStartY = 0.4f;
    private float panEndY = -0.2f;

    private Vector3 zoomRot1 = new Vector3(1.2f, 96.35f, 0);
    private float zoomFOV1 = 12f;

    private ParticleSystem flashMidParticle;
    private ParticleSystem flashFrontParticle;
    private int midFlashParticles = 0;
    private int frontFlashParticles = 0;

    private DepthOfField depthOfField;
    //----------------------------------------
    // MonoBehaviour Overrides
    //----------------------------------------

    void Awake() {
        mainCamOriginRot = MainCamera.transform.rotation;
        slowMoCamOriginRot = SlowMoCamera.transform.rotation;

        mainCamOriginFOV = MainCamera.fieldOfView;
        slowMoCamOriginFOV = SlowMoCamera.fieldOfView;

        flashMidParticle = FlashMid.GetComponent<ParticleSystem>();
        flashFrontParticle = FlashFront.GetComponent<ParticleSystem>();

        profile.profile.TryGetSettings<DepthOfField>(out depthOfField);
         
    }

    void OnEnable()
    {
        AddAllListeners();
        SetCamera(AutoRunwayCamera.MAIN, AutoRunwayCameraState.DEFAULT, AutoRunwayCameraTransition.CUT);
        FlashMid.SetActive(false);
        FlashFront.SetActive(false);
    }

    void OnDisable()
    {
        TimeManager.instance.timeScale = 1.0f;
        Flash.Stop();
        FlashMid.SetActive(false);
        FlashFront.SetActive(false);

        modelsInMidZone.active.Clear();
        modelsInMidZone.history.Clear();
        modelsOnRunway.active.Clear();
        modelsOnRunway.history.Clear();

        RemoveAllListeners();
    }

    void OnDestroy()
    {
        TimeManager.instance.timeScale = 1.0f;

        RemoveAllListeners();
    }

    void Update()
    {
        ClearNullRefsInActiveMaps();
        UpdateCamLookAt();

        if (curCamState == AutoRunwayCameraState.DEFAULT) { UpdateToOrigin(); }
        else if (curCamState == AutoRunwayCameraState.CLOSE_UP) { UpdateCloseUp(); }
        else if (curCamState == AutoRunwayCameraState.CLOSE_UP_PAN) { UpdateCloseUpPan(); }
        else if (curCamState == AutoRunwayCameraState.ZOOM) { UpdateZoomUp(); }

        UpdateFlashSound();
        UpdateVideoCam();
    }

    private void ClearNullRefsInActiveMaps()
    {
        ColliderInfoMap[] colliderInfoMaps = new ColliderInfoMap[] { modelsInMidZone, modelsOnRunway };
        foreach (ColliderInfoMap colliderMapInfo in colliderInfoMaps)
        {
            List<Collider> nonNullColliders = new List<Collider>();
            foreach (Collider collider in colliderMapInfo.active)
                if (collider != null)
                    nonNullColliders.Add(collider);
            colliderMapInfo.active = nonNullColliders;
        }
    }

    private void UpdateVideoCam()
    {
        VideoCamera.transform.LookAt(camLookAt);
    }

    private void UpdateFlashSound()
    {
        int count = flashMidParticle.particleCount;

        if (count > midFlashParticles)
        {
            Flash.Play();
        }
        midFlashParticles = count;

        int count2 = flashFrontParticle.particleCount;

        if (count2 > frontFlashParticles)
        {
            Flash.Play();
        }
        frontFlashParticles = count2;
        //midFlashParticles = 0;
        //frontFlashParticles = 0;

    }
    private void UpdateCloseUpPan()
    {
        if (modelsOnRunway.active.Count == 0) { return; }

        activeCam.fieldOfView = 6.7f;

        activeCam.transform.LookAt(camLookAt);
    }

    private void UpdateCamLookAt()
    {
        if (modelsOnRunway.active.Count == 0) { return; }

        Bounds b = modelsOnRunway.active[0].bounds;

        if (curCam == AutoRunwayCamera.SLOW_MO)
        {
            timeSinceSlowMo += Time.unscaledDeltaTime;
            float t = (Mathf.Cos(timeSinceSlowMo / 2.0f) + 1.0f) / 2.0f;
            //0.15
            //Vector3 max = new Vector3(b.center.x, Mathf.Lerp(b.center.y, b.max.y, panStartY), b.center.z);
            //Vector3 min = new Vector3(b.center.x, Mathf.Lerp(b.center.y, b.min.y, 0), b.center.z);

            Vector3 max = new Vector3(b.center.x, b.center.y + panStartY, b.center.z);
            Vector3 min = new Vector3(b.center.x, b.center.y + panEndY, b.center.z);

            camLookAtFinal.position = Vector3.Lerp(min, max, t);
        }
        else
        {
            camLookAtFinal.position = b.center;
        }


        Vector3 currPos = camLookAtFinal.position;

        // Position Prediction. This is used to compensate for positional lag of camLookAt's lerping (to smooth camera).
        if (lastCamLookAtFinalPos != Vector3.zero)
        {
            Vector3 delta = camLookAtFinal.position - lastCamLookAtFinalPos;
            //delta.y = 0;
            camLookAtFinal.position += delta.normalized * 0.5f;
        }

        lastCamLookAtFinalPos = currPos;

        camLookAt.transform.position = Vector3.Lerp(camLookAt.transform.position, camLookAtFinal.transform.position, Time.deltaTime * camLerpFactor);
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

    private void UpdateToOrigin()
    {
        if (curCamTransition == AutoRunwayCameraTransition.SMOOTH)
        {
            activeCam.transform.eulerAngles = Vector3.Lerp(activeCam.transform.eulerAngles, mainCamOriginRot.eulerAngles, 3.0f * Time.deltaTime);
            activeCam.fieldOfView = Mathf.Lerp(activeCam.fieldOfView, mainCamOriginFOV, 2.0f * Time.deltaTime);
        }
        else
        {
            activeCam.transform.rotation = mainCamOriginRot;
            activeCam.fieldOfView = mainCamOriginFOV;
        }
    }

    private void UpdateCloseUp()
    {
        Vector3 pointDown = new Vector3(6, 90, 0);
        Int32 fov = 38;

        if (curCamTransition == AutoRunwayCameraTransition.SMOOTH)
        {
            activeCam.transform.eulerAngles = Vector3.Lerp(activeCam.transform.eulerAngles, pointDown, 3.0f * Time.deltaTime);
            activeCam.fieldOfView = Mathf.Lerp(activeCam.fieldOfView, fov, 2.0f * Time.deltaTime);
        }
        else
        {
            activeCam.transform.eulerAngles = pointDown;
            activeCam.fieldOfView = fov;
        }
    }

    private void UpdateZoomUp()
    {
        if (curCamTransition == AutoRunwayCameraTransition.SMOOTH)
        {
            activeCam.transform.eulerAngles = Vector3.Lerp(activeCam.transform.eulerAngles, zoomRot1, 3.0f * Time.deltaTime);
            activeCam.fieldOfView = Mathf.Lerp(activeCam.fieldOfView, zoomFOV1, 2.0f * Time.deltaTime);
        }
        else
        {
            activeCam.transform.eulerAngles = zoomRot1;
            activeCam.fieldOfView = zoomFOV1;
        }
    }

    private void CamerasReset()
    {
        MainCamera.fieldOfView = mainCamOriginFOV;
        MainCamera.transform.rotation = mainCamOriginRot;
        SlowMoCamera.transform.rotation = slowMoCamOriginRot;
    }

    private void SelectCamera(AutoRunwayCamera arc)
    {
        curCam = arc;

        if(arc == AutoRunwayCamera.SLOW_MO)
        {
            MainCamera.enabled = false;
            SlowMoCamera.enabled = true;
            activeCam = SlowMoCamera;
            depthOfField.enabled.value = true;
        }
        else
        {
            MainCamera.enabled = true;
            SlowMoCamera.enabled = false;
            activeCam = MainCamera;
            depthOfField.enabled.value = false;
        }
    }

    private void SeekCameraToModel()
    {
        if (modelsOnRunway.active.Count == 0) { return; }

        Bounds b = modelsOnRunway.active[0].bounds;

        camLookAt.transform.position = new Vector3(b.center.x,b.center.y + panStartY,b.center.z);
        activeCam.transform.LookAt(camLookAt);
    }

    private void SetCamera(AutoRunwayCamera cam, AutoRunwayCameraState camState, AutoRunwayCameraTransition camTrans, AutoRunwayCameraSpeed camSpeed = AutoRunwayCameraSpeed.NORMAL)
    {
        CamerasReset();
        SelectCamera(cam);

        if(camState == AutoRunwayCameraState.CLOSE_UP_PAN && camTrans != AutoRunwayCameraTransition.SMOOTH)
        {
            SeekCameraToModel();
        }

        if (camSpeed == AutoRunwayCameraSpeed.VERY_SLOW) { TimeManager.instance.timeScale = 0.5f; }
        else if (camSpeed == AutoRunwayCameraSpeed.SLOW) { TimeManager.instance.timeScale = 0.95f; }
        else { TimeManager.instance.timeScale = 1.0f; }

        timeSinceSlowMo = 0.0f;

        curCamTransition = camTrans;
        curCamState = camState;
    }

    //----------------------------------------
    // Event Callbacks
    //----------------------------------------

    private void OnRunwayEnter(Collider model)
    {
        
        //Debug.Log("HERE COMES A NEW CHALLENGER!" + model.ToString());
        if (modelsOnRunway.history.ContainsKey(model) == false)
        {
            modelsOnRunway.history.Add(model, Time.unscaledTime);
        }

        modelsOnRunway.active.Add(model);

        SetCamera(AutoRunwayCamera.MAIN, AutoRunwayCameraState.ZOOM, AutoRunwayCameraTransition.SMOOTH);
    }

    private void OnRunwayMidEnter(Collider model)
    {
        //Debug.Log("SLOW MO TIME!  " + model.ToString());
        if (modelsInMidZone.history.ContainsKey(model) == false)
        {
            //Enter 1st time
            modelsInMidZone.history.Add(model, Time.unscaledTime);
            
            SetCamera(AutoRunwayCamera.SLOW_MO, AutoRunwayCameraState.CLOSE_UP_PAN, AutoRunwayCameraTransition.CUT, AutoRunwayCameraSpeed.VERY_SLOW);
            FlashMid.SetActive(true);
        } else
        {
            //Enter 2nd time
            modelsInMidZone.active.Add(model);

            SetCamera(AutoRunwayCamera.SLOW_MO, AutoRunwayCameraState.CLOSE_UP_PAN, AutoRunwayCameraTransition.CUT, AutoRunwayCameraSpeed.VERY_SLOW);
        }
    }

    private void OnRunwayMidExit(Collider model)
    {
        SetCamera(AutoRunwayCamera.MAIN, AutoRunwayCameraState.DEFAULT, AutoRunwayCameraTransition.CUT);

        //Model Exiting Mid Zone 2nd time
        FlashMid.SetActive(false);
        if (modelsInMidZone.active.Count > 0)
        {
            modelsInMidZone.active.RemoveAt(0);
            ClearModelHistory(model);
        }
    }

    private void OnRunwayEndEnter(Collider model)
    {
        SetCamera(AutoRunwayCamera.MAIN, AutoRunwayCameraState.CLOSE_UP, AutoRunwayCameraTransition.SMOOTH);
        FlashFront.SetActive(true);
    }

    private void OnRunwayEndExit(Collider model)
    {
        SetCamera(AutoRunwayCamera.MAIN, AutoRunwayCameraState.DEFAULT, AutoRunwayCameraTransition.CUT);
        FlashFront.SetActive(false);
    }

    private void OnRunwayExit(Collider model)
    {
        ClearModelHistory(model);
    }

    private void ClearModelHistory(Collider model)
    {
        if (modelsOnRunway.active.Contains(model))
            modelsOnRunway.active.Remove(model);
        if (modelsOnRunway.history.ContainsKey(model))
            modelsOnRunway.history.Remove(model);
        if (modelsInMidZone.history.ContainsKey(model))
            modelsInMidZone.history.Remove(model);
    }
    /*
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
    */
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
        RunwayEndEvents.OnTriggerExitEvt += OnRunwayEndExit;
    }
    /*
    private void AddAutoRunwayListeners()
    {
        RemoveAutoRunwayListeners();

        AutoRunwayEvents.OnFinaleStartCallback += OnFinaleStart;
        AutoRunwayEvents.OnFinaleEndCallback += OnFinaleEnd;
    }
    */
    private void RemoveRunwayListeners()
    {
        RunwayMidEvents.OnTriggerEnterEvt -= OnRunwayMidEnter;
        RunwayMidEvents.OnTriggerExitEvt -= OnRunwayMidExit;

        RunwayEnterEvents.OnTriggerEnterEvt -= OnRunwayEnter;
        RunwayExitEvents.OnTriggerEnterEvt -= OnRunwayExit;

        RunwayEndEvents.OnTriggerEnterEvt -= OnRunwayEndEnter;
        RunwayEndEvents.OnTriggerExitEvt -= OnRunwayEndExit;
    }
    /*
    private void RemoveAutoRunwayListeners()
    {
        AutoRunwayEvents.OnFinaleStartCallback -= OnFinaleStart;
        AutoRunwayEvents.OnFinaleEndCallback -= OnFinaleEnd;
    }
    */
    private void AddAllListeners()
    {
        AddRunwayListeners();
        //AddAutoRunwayListeners();
    }

    private void RemoveAllListeners()
    {
        RemoveRunwayListeners();
        //RemoveAutoRunwayListeners();
    }
}

public class ColliderInfoMap {
    public Dictionary<Collider, float> history = new Dictionary<Collider, float>();
    public List<Collider> active = new List<Collider>();
}
