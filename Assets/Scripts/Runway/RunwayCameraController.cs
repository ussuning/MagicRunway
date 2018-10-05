using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public enum AutoRunwayCameraSequence { SMOOTH_FOLLOW, COURT, CLASSIC }
public enum AutoRunwayCameraDolly { NONE, RETURN_CENTER, RETURN_CENTER_CLOSE_UP, CENTER_STATIONARY, CENTER_PAN_LEFT, CENTER_PAN_RIGHT, COURT_LEFT,
    FOLLOW, COURT_RIGHT, PAN_DOWN, PAN_UP, PAN_UP_BACK }

public class RunwayCameraController : MonoBehaviour {

    [SerializeField] private RunwayEventManager runwayEventManager;
    [SerializeField] private PostProcessVolume profile;

    public Camera mainCamera;
    public Camera videoCamera;

    private Vector3 lookAtCenterStage = new Vector3(-4.71f, 1, 0);
    private AutoRunwayCameraSequence curCamState = AutoRunwayCameraSequence.CLASSIC;

    private float videoCamOriginFOV = 13f;
    private Quaternion mainCamOriginRot;
    private Vector3 mainCamOriginPos;

    //Camera Flashes
    [SerializeField] private GameObject flashMid;
    [SerializeField] private GameObject flashFront;
    private AudioSource flashAudioSource;
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

    private Int32 targetFOV = 60;
    private AutoRunwayCameraDolly currentCameraDolly;
    //private AutoRunwayCameraClassicState currentClassicState;

    internal ColliderInfoMap modelsInMidZone = new ColliderInfoMap();
    internal ColliderInfoMap modelsOnRunway = new ColliderInfoMap();

    //----------------------------------------------------------------------
    // 
    //----------------------------------------------------------------------

    void Awake() {
        mainCamOriginPos = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, mainCamera.transform.position.z);
        mainCamOriginRot = mainCamera.transform.rotation;

        flashMidParticle = flashMid.GetComponent<ParticleSystem>();
        flashFrontParticle = flashFront.GetComponent<ParticleSystem>();

        profile.profile.TryGetSettings<DepthOfField>(out depthOfField);

        flashAudioSource = GetComponent<AudioSource>();

        ResetDepthOfField();
    }

    void OnEnable()
    {
        AddRunwayEventListeners();
        flashMid.SetActive(false);
        flashFront.SetActive(false);
    }

    void OnDisable()
    {
        TimeManager.instance.timeScale = 1.0f;
        flashAudioSource.Stop();
        flashMid.SetActive(false);
        flashFront.SetActive(false);

        modelsInMidZone.active.Clear();
        modelsInMidZone.history.Clear();
        modelsOnRunway.active.Clear();
        modelsOnRunway.history.Clear();

        RemoveRunwayEventListeners();
    }

    void OnDestroy()
    {
        TimeManager.instance.timeScale = 1.0f;
        RemoveRunwayEventListeners();
    }

    void Update()
    {
        ClearNullRefsInActiveMaps();
        UpdateFlashSound();
        
    }

    private void LateUpdate()
    {
        //UpdateSmoothFollow();
        UpdateClassic();
        UpdateCourt();
        UpdateFollow();
        UpdateReturnToCenter();
        UpdateFOV();

        UpdateVideoCam();
    }

    private void ResetDepthOfField()
    {
        depthOfField.enabled.value = false;
        depthOfField.aperture.overrideState = false;
        depthOfField.focalLength.overrideState = false;
        depthOfField.aperture.value = 3.9f;
        depthOfField.focusDistance.value = 2.25f;
    }

    //----------------------------------------------------------------------
    // Main Update Loops
    //----------------------------------------------------------------------
    private void UpdateFollow()
    {
        if (currentCameraDolly == AutoRunwayCameraDolly.FOLLOW)
            UpdateTracker();
    }

    private void UpdateReturnToCenter()
    {
        if (currentCameraDolly == AutoRunwayCameraDolly.RETURN_CENTER ||
            currentCameraDolly == AutoRunwayCameraDolly.RETURN_CENTER_CLOSE_UP)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, mainCamOriginPos, Time.deltaTime * 2.0f);

            float originX = mainCamOriginRot.eulerAngles.x;
            if (currentCameraDolly == AutoRunwayCameraDolly.RETURN_CENTER_CLOSE_UP)
                originX = 5.8f;

            float targetX = Mathf.LerpAngle(mainCamera.transform.eulerAngles.x, originX, Time.time);
            float targetY = Mathf.LerpAngle(mainCamera.transform.eulerAngles.y, mainCamOriginRot.eulerAngles.y, Time.time);

            Vector3 target = new Vector3(targetX, targetY, mainCamera.transform.eulerAngles.z);
            mainCamera.transform.eulerAngles = Vector3.Lerp(mainCamera.transform.eulerAngles, target, Time.deltaTime * 2.0f);
        }
    }

    private void UpdateClassic()
    {
        if (modelsOnRunway.active.Count == 0) { return; }

        if (curCamState == AutoRunwayCameraSequence.CLASSIC)
        {
            //if (currentCameraDolly == AutoRunwayCameraDolly.FOLLOW)
            //    UpdateTracker();

            if (currentCameraDolly == AutoRunwayCameraDolly.PAN_DOWN)
            {
                UpdateTracker();
                UpdateTiltY(7.0f, 0.4f);
            }

            if (currentCameraDolly == AutoRunwayCameraDolly.PAN_UP)
            {
                UpdateTracker();
                UpdateTiltY(-15.0f, 0.4f);
            }
        }
    }
    /*
    private void UpdateSmoothFollow()
    {
        if (modelsOnRunway.active.Count == 0) { return; }

        if (curCamState == AutoRunwayCameraSequence.SMOOTH_FOLLOW)
        {
            //UpdateTracker();
        }
    }
    */
    private void UpdateCourt()
    {
        if (modelsOnRunway.active.Count == 0) { return; }

        if (curCamState == AutoRunwayCameraSequence.COURT)
        {
            //if (currentCameraDolly == AutoRunwayCameraDolly.FOLLOW)
            //    UpdateTracker();

            if (currentCameraDolly == AutoRunwayCameraDolly.COURT_LEFT)
                UpdateTiltY(courtSideLeftRotPt2, 0.5f);

            if (currentCameraDolly == AutoRunwayCameraDolly.CENTER_PAN_RIGHT)
                UpdateMoveAndLookAt(centerPanPt1, lookAtCenterStage, 0.3f);

            if (currentCameraDolly == AutoRunwayCameraDolly.CENTER_PAN_LEFT)
                UpdateMoveAndLookAt(centerPanPt2, lookAtCenterStage, 0.3f);
        }
    }

    //----------------------------------------------------------------------
    // Reuseable Update Loops
    //----------------------------------------------------------------------

    private void UpdateTiltY(float targetY, float speed = 1.0f)
    {
        Vector3 target = new Vector3(Mathf.LerpAngle(mainCamera.transform.eulerAngles.x, targetY, Time.time), mainCamera.transform.eulerAngles.y, mainCamera.transform.eulerAngles.z);
        mainCamera.transform.eulerAngles = Vector3.Lerp(mainCamera.transform.eulerAngles, target, Time.deltaTime * speed);
    }

    private void UpdateTracker()
    {
        Bounds b = modelsOnRunway.active[0].bounds;
        float offsetX = -3.0f;
        Vector3 target = new Vector3(b.center.x + offsetX, b.center.y, b.center.z);
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, target, Time.deltaTime);
    }

    private void UpdateFOV()
    {
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, 2.0f * Time.deltaTime);
    }

    private void UpdateMoveAndLookAt(Vector3 moveTarget, Vector3 lookAtTarget, float speed = 1.0f)
    {
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, moveTarget, Time.deltaTime * speed);
        mainCamera.transform.LookAt(lookAtTarget);
    }

    private void UpdateVideoCam()
    {
        if (modelsOnRunway.active.Count == 0) { return; }

        Bounds b = modelsOnRunway.active[0].bounds;

        Vector3 target = new Vector3(b.center.x-5.0f, b.center.y + 2.28f, b.center.z);
        videoCamera.transform.position = Vector3.Lerp(videoCamera.transform.position, target, Time.deltaTime);
        //VideoCamera.transform.LookAt(camLookAt);
    }
    //----------------------------------------------------------------------
    // Camera Dolly Execute
    //----------------------------------------------------------------------

    private void RunPanCenterStage(bool fromLeft)
    {
        ResetDepthOfField();

        mainCamera.transform.position = (fromLeft) ? centerPanPt1 : centerPanPt2;
        mainCamera.fieldOfView = targetFOV = 40;
        currentCameraDolly = (fromLeft) ? AutoRunwayCameraDolly.CENTER_PAN_LEFT : AutoRunwayCameraDolly.CENTER_PAN_RIGHT;

        TimeManager.instance.timeScale = 1.0f;
    }

    private void RunCourtSide()
    {
        depthOfField.enabled.value = true;
        depthOfField.aperture.overrideState = false;
        depthOfField.focalLength.overrideState = true;
        depthOfField.focusDistance.value = 6.0f;
        depthOfField.focalLength.value = 68f;

        mainCamera.transform.position = courtSideLeftPos;
        mainCamera.transform.eulerAngles = courtSideLeftRotPt1;
        mainCamera.fieldOfView = targetFOV = 30;
        currentCameraDolly = AutoRunwayCameraDolly.COURT_LEFT;

        TimeManager.instance.timeScale = 1.0f;
    }

    private void RunPan(bool isDown, bool isBack)
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

        float target = (isDown) ? -15.0f : 7.0f;

        mainCamera.transform.eulerAngles = new Vector3(target, mainCamera.transform.eulerAngles.y, mainCamera.transform.eulerAngles.z);
        mainCamera.fieldOfView = targetFOV = (isBack) ? 15 : 30;
        currentCameraDolly = (isDown) ? AutoRunwayCameraDolly.PAN_DOWN : AutoRunwayCameraDolly.PAN_UP;

        TimeManager.instance.timeScale = 0.5f;
    }


    private void RunReturnCenter(bool isStationary, bool isCloseUp)
    {
        ResetDepthOfField();

        if(isStationary)
            mainCamera.transform.position = new Vector3(mainCamOriginPos.x, mainCamOriginPos.y, mainCamOriginPos.z);

        if(isCloseUp)
        {
            targetFOV = 40;

            if (isStationary)
                mainCamera.transform.eulerAngles = new Vector3(5.8f, mainCamera.transform.eulerAngles.y, mainCamera.transform.eulerAngles.z);
        } else
        {
            targetFOV = 60;

            if (isStationary)
                mainCamera.transform.rotation = mainCamOriginRot;
        }

        if (isStationary)
            mainCamera.fieldOfView = targetFOV;

        AutoRunwayCameraDolly returnCenter = (isCloseUp) ? AutoRunwayCameraDolly.RETURN_CENTER_CLOSE_UP : AutoRunwayCameraDolly.RETURN_CENTER;
        currentCameraDolly = (isStationary) ? AutoRunwayCameraDolly.CENTER_STATIONARY : returnCenter;

        TimeManager.instance.timeScale = 1.0f;
    }

    
    /*
    void UpdateDepthOfField()
    {
        if (depthOfField == null) { return; }
        if (modelsOnRunway.active.Count == 0) { return; }

        Bounds b = modelsOnRunway.active[0].bounds;

        depthOfField.focusDistance.value = (mainCamera.transform.position - b.center).magnitude;
    }
    */
    

    

    private void UpdateFlashSound()
    {
        int count = flashMidParticle.particleCount;

        if (count > midFlashParticles)
            flashAudioSource.Play();

        midFlashParticles = count;

        int count2 = flashFrontParticle.particleCount;

        if (count2 > frontFlashParticles)
            flashAudioSource.Play();
  
        frontFlashParticles = count2;
    }









    

    //----------------------------------------------------------------------
    // Camera Triggers
    //----------------------------------------------------------------------

    private void ChooseCamMovement()
    {
        List<AutoRunwayCameraSequence> states = new List<AutoRunwayCameraSequence>(new AutoRunwayCameraSequence[] {
            AutoRunwayCameraSequence.CLASSIC, AutoRunwayCameraSequence.SMOOTH_FOLLOW, AutoRunwayCameraSequence.COURT });
        int index = UnityEngine.Random.Range(0, states.Count);

        curCamState = states[index];
        
        //curCamState = AutoRunwayCameraSequence.CLASSIC;
        //curCamState = AutoRunwayCameraSequence.SMOOTH_FOLLOW;
        //curCamState = AutoRunwayCameraSequence.COURT;
    }

    private void TriggerCamActionEnter()
    {
        if (curCamState == AutoRunwayCameraSequence.SMOOTH_FOLLOW)
        {
            targetFOV = 60;
            currentCameraDolly = AutoRunwayCameraDolly.FOLLOW;
            TimeManager.instance.timeScale = 1.0f;
        }

        if (curCamState == AutoRunwayCameraSequence.CLASSIC)
        {
            targetFOV = 60;
            currentCameraDolly = AutoRunwayCameraDolly.FOLLOW;
            TimeManager.instance.timeScale = 1.0f;
        }

        if (curCamState == AutoRunwayCameraSequence.COURT)
        {
            RunCourtSide();   
        }
    }

    private void TriggerCamActionMidEnterFront()
    {
        if (curCamState == AutoRunwayCameraSequence.SMOOTH_FOLLOW)
        {
            targetFOV = 40;
            TimeManager.instance.timeScale = 1.0f;
        }

        if (curCamState == AutoRunwayCameraSequence.CLASSIC)
        {
            int rand = UnityEngine.Random.Range(0, 2);
            if (rand == 0)
                RunPan(true, false);
            else
                RunPan(false, false);
        }
    }

    private void TriggerCamActionMidExitFront()
    {
        if (curCamState == AutoRunwayCameraSequence.SMOOTH_FOLLOW)
        {
            targetFOV = 60;
            TimeManager.instance.timeScale = 1.0f;
        }

        if (curCamState == AutoRunwayCameraSequence.CLASSIC)
        {
            RunReturnCenter(false, true);
        }

        if (curCamState == AutoRunwayCameraSequence.COURT)
        {
            int rand = UnityEngine.Random.Range(0, 2);
            if (rand == 0)
                RunPanCenterStage(true);
            else
                RunPanCenterStage(false);
        }
    }

    private void TriggerCamActionMidEnterBack()
    {
        if (curCamState == AutoRunwayCameraSequence.SMOOTH_FOLLOW)
        {
            targetFOV = 20;
            TimeManager.instance.timeScale = 1.0f;
        }

        if (curCamState == AutoRunwayCameraSequence.CLASSIC)
        {
            int rand = UnityEngine.Random.Range(0, 2);
            if (rand == 0)
                RunPan(true, true);
            else
                RunPan(false, true);
        }
    }

    private void TriggerCamActionMidExitBack()
    {
        RunReturnCenter(false, false);
    }

    private void TriggerCamActionEndEnter() {}

    private void TriggerCamActionEndExit()
    {
        if (curCamState == AutoRunwayCameraSequence.COURT)
        {
            RunReturnCenter(true,false);
            currentCameraDolly = AutoRunwayCameraDolly.FOLLOW;
        }
        
        if (curCamState == AutoRunwayCameraSequence.CLASSIC)
        {
            currentCameraDolly = AutoRunwayCameraDolly.FOLLOW;
            TimeManager.instance.timeScale = 1.0f;
        }
    }

    //----------------------------------------------------------------------
    // Model handling
    //----------------------------------------------------------------------

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

    private void ClearModelHistory(Collider model)
    {
        if (modelsOnRunway.active.Contains(model))
            modelsOnRunway.active.Remove(model);
        if (modelsOnRunway.history.ContainsKey(model))
            modelsOnRunway.history.Remove(model);
        if (modelsInMidZone.history.ContainsKey(model))
            modelsInMidZone.history.Remove(model);
    }

    //----------------------------------------------------------------------
    // Runway Event Functions
    //----------------------------------------------------------------------

    private void OnRunwayEnter(Collider model)
    {
        if (modelsOnRunway.history.ContainsKey(model) == false)
            modelsOnRunway.history.Add(model, Time.unscaledTime);

        modelsOnRunway.active.Add(model);

        ChooseCamMovement();
        TriggerCamActionEnter();
    }

    private void OnRunwayMidEnter(Collider model)
    {
        //Debug.Log("SLOW MO TIME!  " + model.ToString());
        if (modelsInMidZone.history.ContainsKey(model) == false)
        {
            //Enter 1st time
            modelsInMidZone.history.Add(model, Time.unscaledTime);

            flashMid.SetActive(true);
            TriggerCamActionMidEnterFront();
        }
        else
        {
            //Enter 2nd time
            modelsInMidZone.active.Add(model);
            TriggerCamActionMidEnterBack();
        }
    }

    private void OnRunwayMidExit(Collider model)
    {
        //Model Exiting Mid Zone 2nd time
        flashMid.SetActive(false);
        if (modelsInMidZone.active.Count > 0)
        {
            modelsInMidZone.active.RemoveAt(0);
            ClearModelHistory(model);

            TriggerCamActionMidExitBack();
        }
        else
        {
            TriggerCamActionMidExitFront();
        }
    }

    private void OnRunwayEndEnter(Collider model)
    {
        flashFront.SetActive(true);
        TriggerCamActionEndEnter();
    }

    private void OnRunwayEndExit(Collider model)
    {
        flashFront.SetActive(false);
        TriggerCamActionEndExit();
    }

    private void OnRunwayExit(Collider model) { ClearModelHistory(model); }

    private void AddRunwayEventListeners()
    {
        RemoveRunwayEventListeners();

        runwayEventManager.RunwayEnter.OnTriggerEnterEvt += OnRunwayEnter;

        runwayEventManager.RunwayMid.OnTriggerEnterEvt += OnRunwayMidEnter;
        runwayEventManager.RunwayMid.OnTriggerExitEvt += OnRunwayMidExit;

        runwayEventManager.RunwayEnd.OnTriggerEnterEvt += OnRunwayEndEnter;
        runwayEventManager.RunwayEnd.OnTriggerExitEvt += OnRunwayEndExit;

        runwayEventManager.RunwayExit.OnTriggerEnterEvt += OnRunwayExit;
    }

    private void RemoveRunwayEventListeners()
    {
        runwayEventManager.RunwayEnter.OnTriggerEnterEvt -= OnRunwayEnter;

        runwayEventManager.RunwayMid.OnTriggerEnterEvt -= OnRunwayMidEnter;
        runwayEventManager.RunwayMid.OnTriggerExitEvt -= OnRunwayMidExit;

        runwayEventManager.RunwayEnd.OnTriggerEnterEvt -= OnRunwayEndEnter;
        runwayEventManager.RunwayEnd.OnTriggerExitEvt -= OnRunwayEndExit;

        runwayEventManager.RunwayExit.OnTriggerEnterEvt -= OnRunwayExit;
    }


    /*
    private void UpdateCloseUpPan()
    {
        if (modelsOnRunway.active.Count == 0) { return; }

        mainCamera.fieldOfView = 6.7f;

        mainCamera.transform.LookAt(camLookAt);
        UpdateDepthOfField();
    }
    */
    /*
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
   */
    /*
     private void UpdateMainCameraZoom(Bounds b) {
         Vector3 max = new Vector3(b.center.x, b.max.y, b.center.z);
         Vector3 min = new Vector3(b.center.x, b.min.y, b.center.z);
         Vector2 max2D = mainCamera.WorldToScreenPoint(max);
         Vector2 min2D = mainCamera.WorldToScreenPoint(min);
         float height = max2D.y - min2D.y;
         float heightPercent = height / Screen.height;
         float fovDelta = (heightPercent - targetHeightPercent) * (fovSpeed * Time.deltaTime);
         mainCamera.fieldOfView += fovDelta;
         mainCamera.fieldOfView = Mathf.Clamp(mainCamera.fieldOfView, mainCamMinFOV, float.MaxValue);
     }
     */
    /*
    private void UpdateToOrigin()
    {
        if (curCamTransition == AutoRunwayCameraTransition.SMOOTH)
        {
            mainCamera.transform.eulerAngles = Vector3.Lerp(mainCamera.transform.eulerAngles, mainCamOriginRot.eulerAngles, 3.0f * Time.deltaTime);
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, mainCamOriginFOV, 2.0f * Time.deltaTime);
        }
        else
        {
            mainCamera.transform.rotation = mainCamOriginRot;
            mainCamera.fieldOfView = mainCamOriginFOV;
        }
    }

    private void UpdateCloseUp()
    {
        Vector3 pointDown = new Vector3(6, 90, 0);
        Int32 fov = 38;

        if (curCamTransition == AutoRunwayCameraTransition.SMOOTH)
        {
            mainCamera.transform.eulerAngles = Vector3.Lerp(mainCamera.transform.eulerAngles, pointDown, 3.0f * Time.deltaTime);
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, fov, 2.0f * Time.deltaTime);
        }
        else
        {
            mainCamera.transform.eulerAngles = pointDown;
            mainCamera.fieldOfView = fov;
        }
    }
    */
    /*
    private void UpdateZoomUp()
    {
        if (curCamTransition == AutoRunwayCameraTransition.SMOOTH)
        {
            mainCamera.transform.eulerAngles = Vector3.Lerp(mainCamera.transform.eulerAngles, zoomRot1, 3.0f * Time.deltaTime);
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, zoomFOV1, 2.0f * Time.deltaTime);
        }
        else
        {
            mainCamera.transform.eulerAngles = zoomRot1;
            mainCamera.fieldOfView = zoomFOV1;
        }

        UpdateDepthOfField();
    }
    */
    /*
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
            mainCamera = SlowMoCamera;
        }
        else
        {
            MainCamera.enabled = true;
            SlowMoCamera.enabled = false;
            mainCamera = MainCamera;
        }
    }
    */
    /*
    private void SeekCameraToModel()
    {
        if (modelsOnRunway.active.Count == 0) { return; }

        Bounds b = modelsOnRunway.active[0].bounds;

        camLookAt.transform.position = new Vector3(b.center.x,b.center.y + panStartY,b.center.z);
        mainCamera.transform.LookAt(camLookAt);
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
    */
}

public class ColliderInfoMap {
    public Dictionary<Collider, float> history = new Dictionary<Collider, float>();
    public List<Collider> active = new List<Collider>();
}
