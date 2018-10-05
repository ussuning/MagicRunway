using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public enum AutoRunwayCamera { MAIN, SLOW_MO };
public enum AutoRunwayCameraFollowState { FOLLOW, UNFOLLOW, NONE}
public enum AutoRunwayCameraSpeed { NORMAL, SLOW, VERY_SLOW }
public enum AutoRunwayCameraState { DEFAULT, CLOSE_UP, CLOSE_UP_PAN, ZOOM, CENTER_PAN_LEFT, CENTER_PAN_RIGHT, COURT_LEFT, BIG_FOLLOW, COURT, CLASSIC }
public enum AutoRunwayCameraTransition { CUT, SMOOTH, FADE }
public enum AutoRunwayCameraCourtState { NONE, CENTER_PAN_LEFT, CENTER_PAN_RIGHT, COURT_LEFT, FOLLOW, COURT_RIGHT }
public enum AutoRunwayCameraClassicState { NONE, FOLLOW, FRONTAL_SLOWMO, BACK_SLOWMO, PAN_DOWN, PAN_UP, PAN_UP_BACK }

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
    //public Image blackout;

    public float mainCamMinFOV = 20f;
    public float targetHeightPercent = 0.65f;
    public float fovSpeed = 60f; // fov/sec 

    //[SerializeField]
    private Vector3 lookAtCenterStage = new Vector3(-4.71f, 1, 0);

    protected Camera activeCam;

    internal ColliderInfoMap modelsInMidZone = new ColliderInfoMap();
    internal ColliderInfoMap modelsOnRunway = new ColliderInfoMap();

    protected float timeSinceSlowMo;

    private float mainCamOriginFOV = 60f;
    private float slowMoCamOriginFOV = 13f;
    private float videoCamOriginFOV = 13f;
    private Quaternion mainCamOriginRot;
    private Quaternion slowMoCamOriginRot;
    private Vector3 mainCamOriginPos;

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




    private Vector3 centerPanPt1 = new Vector3(-7.4f, 1, 2.5f);
    private Vector3 centerPanPt2 = new Vector3(-7.4f, 1, -2.5f);

    private Vector3 courtSideLeftPos = new Vector3(-2.14f, 0.47f, 5.0f);
    private Vector3 courtSideLeftRotPt1 = new Vector3(-20f, 150, 0);
    private float courtSideLeftRotPt2 = -2.0f;


    private Vector3 lastMovement = Vector3.zero;
    private Vector3 currentMovement = Vector3.zero;

    private Int32 targetFOV = 60;

    //private bool isScanning = false;
    private AutoRunwayCameraCourtState currentCourtState;
    private AutoRunwayCameraClassicState currentClassicState;

    //----------------------------------------
    // MonoBehaviour Overrides
    //----------------------------------------

    void Awake() {
        mainCamOriginPos = new Vector3(MainCamera.transform.position.x, MainCamera.transform.position.y, MainCamera.transform.position.z);

        mainCamOriginRot = MainCamera.transform.rotation;
        slowMoCamOriginRot = SlowMoCamera.transform.rotation;

        mainCamOriginFOV = MainCamera.fieldOfView;
        slowMoCamOriginFOV = SlowMoCamera.fieldOfView;

        flashMidParticle = FlashMid.GetComponent<ParticleSystem>();
        flashFrontParticle = FlashFront.GetComponent<ParticleSystem>();

        profile.profile.TryGetSettings<DepthOfField>(out depthOfField);

        ResetDepthOfField();
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

        //UpdatePanCenterStage();
        //UpdateCourtSide();
        //UpdateBigFollow();

        /*
        UpdateCamLookAt();

        if (curCamState == AutoRunwayCameraState.DEFAULT) { UpdateToOrigin(); }
        else if (curCamState == AutoRunwayCameraState.CLOSE_UP) { UpdateCloseUp(); }
        else if (curCamState == AutoRunwayCameraState.CLOSE_UP_PAN) { UpdateCloseUpPan(); }
        else if (curCamState == AutoRunwayCameraState.ZOOM) { UpdateZoomUp(); }
        */
        UpdateFlashSound();
        UpdateVideoCam();
    }

    private void LateUpdate()
    {
        UpdateBigFollow();
        UpdateClassic();
        UpdateCourt();
        UpdateFOV();
        //UpdateScan();
    }

    private void ResetDepthOfField()
    {
        depthOfField.enabled.value = false;
        depthOfField.aperture.overrideState = false;
        depthOfField.focalLength.overrideState = false;
        depthOfField.aperture.value = 3.9f;
        depthOfField.focusDistance.value = 2.25f;
    }

    private void UpdateClassic()
    {
        if (modelsOnRunway.active.Count == 0) { return; }

        if (curCamState == AutoRunwayCameraState.CLASSIC)
        {
            
            if (currentClassicState == AutoRunwayCameraClassicState.FOLLOW)
            {
                UpdateFollow();
            }

            if (currentClassicState == AutoRunwayCameraClassicState.PAN_DOWN)
            {
                UpdateFollow();
                Vector3 target = new Vector3(Mathf.LerpAngle(activeCam.transform.eulerAngles.x, 7, Time.time), activeCam.transform.eulerAngles.y, activeCam.transform.eulerAngles.z);
                activeCam.transform.eulerAngles = Vector3.Lerp(activeCam.transform.eulerAngles, target, Time.deltaTime * 0.4f);
            }

            if (currentClassicState == AutoRunwayCameraClassicState.PAN_UP)
            {
                UpdateFollow();
                Vector3 target = new Vector3(Mathf.LerpAngle(activeCam.transform.eulerAngles.x, -15, Time.time), activeCam.transform.eulerAngles.y, activeCam.transform.eulerAngles.z);
                activeCam.transform.eulerAngles = Vector3.Lerp(activeCam.transform.eulerAngles, target, Time.deltaTime * 0.4f);
            }
        }
    }

    private void UpdateFollow()
    {
        Bounds b = modelsOnRunway.active[0].bounds;
        float offsetX = -3.0f;
        currentMovement = new Vector3(b.center.x + offsetX, b.center.y, b.center.z);
        activeCam.transform.position = Vector3.Lerp(activeCam.transform.position, currentMovement, Time.deltaTime);
    }

    private void UpdateBigFollow()
    {
        if (modelsOnRunway.active.Count == 0) { return; }

        if (curCamState == AutoRunwayCameraState.BIG_FOLLOW)
        {
            if (modelsOnRunway.active.Count == 0) { return; }

            Bounds b = modelsOnRunway.active[0].bounds;
            float offsetX = -3.0f;
            currentMovement = new Vector3(b.center.x + offsetX, b.center.y, b.center.z);
            activeCam.transform.position = Vector3.Lerp(activeCam.transform.position, currentMovement, Time.deltaTime);
        }
    }

    private void UpdateFOV()
    {
        activeCam.fieldOfView = Mathf.Lerp(activeCam.fieldOfView, targetFOV, 2.0f * Time.deltaTime);
    }

    private void RunPanCenterStageFromLeft()
    {
        activeCam.transform.position = centerPanPt1;
        activeCam.fieldOfView = targetFOV = 40;
        currentCourtState = AutoRunwayCameraCourtState.CENTER_PAN_LEFT;
    }

    private void RunPanCenterStageFromRight()
    {
        activeCam.transform.position = centerPanPt2;
        activeCam.fieldOfView = targetFOV = 40;
        currentCourtState = AutoRunwayCameraCourtState.CENTER_PAN_RIGHT;
    }

    private void RunCourtSide()
    {
        depthOfField.enabled.value = true;
        depthOfField.aperture.overrideState = false;
        depthOfField.focalLength.overrideState = true;

        depthOfField.focusDistance.value = 6.0f;
        depthOfField.focalLength.value = 68f;

        activeCam.transform.position = courtSideLeftPos;
        activeCam.transform.eulerAngles = courtSideLeftRotPt1;
        activeCam.fieldOfView = targetFOV = 30;
        currentCourtState = AutoRunwayCameraCourtState.COURT_LEFT;
    }

    private void RunPanDown(bool isBack = false)
    {
        depthOfField.enabled.value = true;
        depthOfField.aperture.overrideState = true;
        depthOfField.focalLength.overrideState = false;

        if (!isBack)
        {
            depthOfField.aperture.value = 3.9f;
            depthOfField.focusDistance.value = 2.25f;
        }
        else
        {
            depthOfField.aperture.value = 3.0f;
            depthOfField.focusDistance.value = 3.7f;
        }

        activeCam.transform.eulerAngles = new Vector3(-15, activeCam.transform.eulerAngles.y, activeCam.transform.eulerAngles.z);
        activeCam.fieldOfView = targetFOV = (isBack) ? 15 : 30;
        currentClassicState = AutoRunwayCameraClassicState.PAN_DOWN;
    }

    private void RunPanUp(bool isBack = false)
    {
        depthOfField.enabled.value = true;
        depthOfField.aperture.overrideState = true;
        depthOfField.focalLength.overrideState = false;

        if (!isBack)
        {
            depthOfField.aperture.value = 3.9f;
            depthOfField.focusDistance.value = 2.25f;
        }
        else
        {
            depthOfField.aperture.value = 3.0f;
            depthOfField.focusDistance.value = 3.7f;
        }
        activeCam.transform.eulerAngles = new Vector3(7, activeCam.transform.eulerAngles.y, activeCam.transform.eulerAngles.z);
        activeCam.fieldOfView = targetFOV = (isBack) ? 15 : 30;
        currentClassicState = AutoRunwayCameraClassicState.PAN_UP;
    }

    private void RunDefault(bool isCloseUp = false)
    {
        activeCam.transform.position = new Vector3(mainCamOriginPos.x, mainCamOriginPos.y, mainCamOriginPos.z);

        if(isCloseUp)
        {
            activeCam.transform.eulerAngles = new Vector3(5.8f, activeCam.transform.eulerAngles.y, activeCam.transform.eulerAngles.z);
            //Debug.Log(activeCam.transform.rotation);
            activeCam.fieldOfView = targetFOV = 40;
        } else
        {
            activeCam.transform.rotation = mainCamOriginRot;
            activeCam.fieldOfView = targetFOV = 60;
        }
        
        currentCourtState = AutoRunwayCameraCourtState.NONE;
        currentClassicState = AutoRunwayCameraClassicState.NONE;
    }

    private void UpdateCourt()
    {
        if (modelsOnRunway.active.Count == 0) { return; }

        if (curCamState == AutoRunwayCameraState.COURT)
        {
            if (currentCourtState == AutoRunwayCameraCourtState.FOLLOW)
            {
                UpdateFollow();
            }

            if (currentCourtState == AutoRunwayCameraCourtState.COURT_LEFT)
            {
                Vector3 target = new Vector3(Mathf.LerpAngle(activeCam.transform.eulerAngles.x, courtSideLeftRotPt2, Time.time * 1.0f), activeCam.transform.eulerAngles.y, activeCam.transform.eulerAngles.z);
                activeCam.transform.eulerAngles = Vector3.Lerp(activeCam.transform.eulerAngles, target, Time.deltaTime * 0.5f);
            }

            if (currentCourtState == AutoRunwayCameraCourtState.CENTER_PAN_RIGHT)
            {
                activeCam.transform.position = Vector3.Lerp(activeCam.transform.position, centerPanPt1, Time.deltaTime * 0.3f);
                activeCam.transform.LookAt(lookAtCenterStage);
            }

            if (currentCourtState == AutoRunwayCameraCourtState.CENTER_PAN_LEFT)
            {
                activeCam.transform.position = Vector3.Lerp(activeCam.transform.position, centerPanPt2, Time.deltaTime * 0.3f);
                activeCam.transform.LookAt(lookAtCenterStage);
            }
        }
    }

    void UpdateDepthOfField()
    {
        if (depthOfField == null) { return; }
        if (modelsOnRunway.active.Count == 0) { return; }

        Bounds b = modelsOnRunway.active[0].bounds;

        depthOfField.focusDistance.value = (activeCam.transform.position - b.center).magnitude;
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
    }

    private void UpdateCloseUpPan()
    {
        if (modelsOnRunway.active.Count == 0) { return; }

        activeCam.fieldOfView = 6.7f;

        activeCam.transform.LookAt(camLookAt);
        UpdateDepthOfField();
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

        UpdateDepthOfField();
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
        }
        else
        {
            MainCamera.enabled = true;
            SlowMoCamera.enabled = false;
            activeCam = MainCamera;
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

        switch (camState)
        {
            case AutoRunwayCameraState.ZOOM:
            case AutoRunwayCameraState.CLOSE_UP_PAN:
                depthOfField.enabled.value = true;
                break;
            default:
                depthOfField.enabled.value = false;
                break;
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
    private void ChooseCamMovement()
    {
        List<AutoRunwayCameraState> states = new List<AutoRunwayCameraState>(new AutoRunwayCameraState[] {
            AutoRunwayCameraState.CLASSIC, AutoRunwayCameraState.BIG_FOLLOW, AutoRunwayCameraState.COURT });
        int index = UnityEngine.Random.Range(0, states.Count);

        curCamState = states[index];
        //curCamState = AutoRunwayCameraState.BIG_FOLLOW;
        //curCamState = AutoRunwayCameraState.COURT;
    }

    private void TriggerCamActionEnter()
    {
        activeCam = MainCamera;
        ResetDepthOfField();

        if (curCamState == AutoRunwayCameraState.BIG_FOLLOW)
        {
            targetFOV = 60;
        }

        if (curCamState == AutoRunwayCameraState.CLASSIC)
        {
            targetFOV = 60;
            currentClassicState = AutoRunwayCameraClassicState.FOLLOW;
        }

        if (curCamState == AutoRunwayCameraState.COURT)
        {
            RunCourtSide();   
        }
    }

    private void TriggerCamActionMidEnterFront()
    {
        //ResetDepthOfField();

        if (curCamState == AutoRunwayCameraState.BIG_FOLLOW)
        {
            targetFOV = 40;
        }

        if (curCamState == AutoRunwayCameraState.CLASSIC)
        {
            int rand = UnityEngine.Random.Range(0, 2);
            if (rand == 0)
                RunPanDown();
            else
                RunPanUp();
        }
    }

    private void TriggerCamActionMidExitFront()
    {
        //ResetDepthOfField();

        if (curCamState == AutoRunwayCameraState.BIG_FOLLOW)
        {
            targetFOV = 60;
        }

        if (curCamState == AutoRunwayCameraState.CLASSIC)
        {
            ResetDepthOfField();
            RunDefault(true);
        }

        if (curCamState == AutoRunwayCameraState.COURT)
        {
            int rand = UnityEngine.Random.Range(0, 2);
            if (rand == 0)
                RunPanCenterStageFromLeft();
            else
                RunPanCenterStageFromRight();
        }
        /*
        if (curCamState == AutoRunwayCameraState.COURT)
        {
            RunDefault();
        }*/
    }

    private void TriggerCamActionMidEnterBack()
    {
        //ResetDepthOfField();

        if (curCamState == AutoRunwayCameraState.BIG_FOLLOW)
        {
            targetFOV = 20;
        }

        if (curCamState == AutoRunwayCameraState.CLASSIC)
        {
            int rand = UnityEngine.Random.Range(0, 2);
            if (rand == 0)
                RunPanDown(true);
            else
                RunPanUp(true);
        }
    }

    private void TriggerCamActionEndEnter()
    {
        
    }

    private void TriggerCamActionEndExit()
    {
        //

        if (curCamState == AutoRunwayCameraState.COURT)
        {
            RunDefault();
            currentCourtState = AutoRunwayCameraCourtState.FOLLOW;
        }
        
        if (curCamState == AutoRunwayCameraState.CLASSIC)
        {
            currentClassicState = AutoRunwayCameraClassicState.FOLLOW;
        }
    }

    private void OnRunwayEnter(Collider model)
    {
        //Debug.Log("HERE COMES A NEW CHALLENGER!" + model.ToString());
        if (modelsOnRunway.history.ContainsKey(model) == false)
        {
            modelsOnRunway.history.Add(model, Time.unscaledTime);
        }

        modelsOnRunway.active.Add(model);

        ChooseCamMovement();
        TriggerCamActionEnter();
        //SetCamera(AutoRunwayCamera.MAIN, AutoRunwayCameraState.ZOOM, AutoRunwayCameraTransition.SMOOTH);
    }

    private void OnRunwayMidEnter(Collider model)
    {
        //Debug.Log("SLOW MO TIME!  " + model.ToString());
        if (modelsInMidZone.history.ContainsKey(model) == false)
        {
            //Enter 1st time
            modelsInMidZone.history.Add(model, Time.unscaledTime);
            
            //SetCamera(AutoRunwayCamera.SLOW_MO, AutoRunwayCameraState.CLOSE_UP_PAN, AutoRunwayCameraTransition.CUT, AutoRunwayCameraSpeed.VERY_SLOW);
            FlashMid.SetActive(true);
            TriggerCamActionMidEnterFront();
        } else
        {
            //Enter 2nd time
            modelsInMidZone.active.Add(model);
            TriggerCamActionMidEnterBack();
            // SetCamera(AutoRunwayCamera.SLOW_MO, AutoRunwayCameraState.CLOSE_UP_PAN, AutoRunwayCameraTransition.CUT, AutoRunwayCameraSpeed.VERY_SLOW);
        }
    }

    private void OnRunwayMidExit(Collider model)
    {
        //SetCamera(AutoRunwayCamera.MAIN, AutoRunwayCameraState.DEFAULT, AutoRunwayCameraTransition.CUT);

        //Model Exiting Mid Zone 2nd time
        FlashMid.SetActive(false);
        if (modelsInMidZone.active.Count > 0)
        {
            modelsInMidZone.active.RemoveAt(0);
            ClearModelHistory(model);
        }
        else
        {
            TriggerCamActionMidExitFront();
        }
    }

    private void OnRunwayEndEnter(Collider model)
    {
        //SetCamera(AutoRunwayCamera.MAIN, AutoRunwayCameraState.CLOSE_UP, AutoRunwayCameraTransition.SMOOTH);
        FlashFront.SetActive(true);
        TriggerCamActionEndEnter();
    }

    private void OnRunwayEndExit(Collider model)
    {
        //SetCamera(AutoRunwayCamera.MAIN, AutoRunwayCameraState.DEFAULT, AutoRunwayCameraTransition.CUT);
        FlashFront.SetActive(false);
        TriggerCamActionEndExit();
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

    private void RemoveRunwayListeners()
    {
        RunwayMidEvents.OnTriggerEnterEvt -= OnRunwayMidEnter;
        RunwayMidEvents.OnTriggerExitEvt -= OnRunwayMidExit;

        RunwayEnterEvents.OnTriggerEnterEvt -= OnRunwayEnter;
        RunwayExitEvents.OnTriggerEnterEvt -= OnRunwayExit;

        RunwayEndEvents.OnTriggerEnterEvt -= OnRunwayEndEnter;
        RunwayEndEvents.OnTriggerExitEvt -= OnRunwayEndExit;
    }

    private void AddAllListeners()
    {
        AddRunwayListeners();
    }

    private void RemoveAllListeners()
    {
        RemoveRunwayListeners();
    }
}

public class ColliderInfoMap {
    public Dictionary<Collider, float> history = new Dictionary<Collider, float>();
    public List<Collider> active = new List<Collider>();
}
