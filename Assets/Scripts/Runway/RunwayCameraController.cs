using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunwayCameraController : MonoBehaviour {
    
    public ColliderEvents RunwayEnterEvents;
    public ColliderEvents RunwayMidEvents;
    public ColliderEvents RunwayEndEvents;
    public ColliderEvents RunwayExitEvents;

    public Transform camLookAt;
    public Camera MainCamera;
    public Camera SlowMoCamera;
    public GameObject MidFlashes;
    public GameObject FrontFlashes;

    public float mainCamMinFOV = 20f;
    public float targetHeightPercent = 0.65f;
    public float fovSpeed = 60f; // fov/sec 

    protected Camera activeCam;

    protected float timeUntilEndFrontFlashes = 0f;


    internal ColliderInfoMap mid = new ColliderInfoMap();
    internal ColliderInfoMap onRunway = new ColliderInfoMap();

    protected float timeSinceSlowMo;

    bool newMid = false;

	// Use this for initialization
	void Start () {
        activeCam = MainCamera;

        RunwayMidEvents.OnTriggerEnterEvt += OnRunwayMidEnter;
        RunwayMidEvents.OnTriggerExitEvt += OnRunwayMidExit;

        RunwayEnterEvents.OnTriggerEnterEvt += OnRunwayEnter;
        RunwayExitEvents.OnTriggerEnterEvt += OnRunwayExit;

        RunwayEndEvents.OnTriggerEnterEvt += OnRunwayEndEnter;
        EnableMidFlashes(false);
        EnableFrontFlashes(false);
	}

    private void OnDestroy()
    {
        RunwayMidEvents.OnTriggerEnterEvt -= OnRunwayMidEnter;
        RunwayMidEvents.OnTriggerExitEvt -= OnRunwayMidExit;

        RunwayEnterEvents.OnTriggerEnterEvt -= OnRunwayEnter;
        RunwayExitEvents.OnTriggerEnterEvt -= OnRunwayExit;

        RunwayEndEvents.OnTriggerEnterEvt -= OnRunwayEndEnter;
    }

    private void OnRunwayEndEnter(Collider other)
    {
        Debug.Log("OnRunwayEndEnter " + other.name);

        EnableFrontFlashes(true);
        timeUntilEndFrontFlashes = 2.5f;
    }

    private void OnRunwayMidEnter(Collider other)
    {
        Debug.Log("OnRunwayMidEnter " + other.name);

        if (other.GetComponent<Animator>() == null)
            return;

        if (mid.history.ContainsKey(other) == false) {
            mid.history.Add(other, Time.unscaledTime);
            newMid = true;
        }

        mid.active.Add(other);

        CheckMidActives();
    }

    private void OnRunwayMidExit(Collider other)
    {
        Debug.Log("OnRunwayMidExit " + other.name);
        if (mid.active.Contains(other))
            mid.active.Remove(other);

        CheckMidActives();
    }

    private void OnRunwayExit(Collider other)
    {
        if (onRunway.active.Contains(other))
            onRunway.active.Remove(other);
        if (onRunway.history.ContainsKey(other))
            onRunway.history.Remove(other);
        if (mid.history.ContainsKey(other))
            mid.history.Remove(other);
    }

    private void OnRunwayEnter(Collider other)
    {
        if (other.GetComponent<Animator>() == null)
            return;
        
        if (onRunway.history.ContainsKey(other) == false)
        {
            onRunway.history.Add(other, Time.unscaledTime);
        }
        onRunway.active.Add(other);
    }

    void CheckMidActives() {
        if (mid.active.Count > 0) {
            if (newMid)
            {
                // Front view of model.
                Time.timeScale = 0.5f;
                newMid = false;
                EnableMidFlashes(true);
            }
            else {
                // Rear view of model
                Time.timeScale = 0.95f;
            }
            //Random.Range(0, 1) > 0.5f ? EnableMidFlashes(true) : EnableMidFlashes(false);
            timeSinceSlowMo = 0.0f;
            MainCamera.enabled = false;
            SlowMoCamera.enabled = true;
            activeCam = SlowMoCamera;
        } else {
            EnableMidFlashes(false);
            Time.timeScale = 1.0f;
            MainCamera.enabled = true;
            SlowMoCamera.enabled = false;
            activeCam = MainCamera;
        }
    }

    private void Update()
    {
        if (onRunway.active.Count > 0) {
            Bounds b = onRunway.active[0].bounds;
            if (Time.timeScale < 1.0f)
            {
                timeSinceSlowMo += Time.unscaledDeltaTime;
                float t = (Mathf.Cos(timeSinceSlowMo/2.0f) + 1.0f) / 2.0f;
                //Debug.Log("t=" + t);

                Vector3 max = new Vector3(b.center.x, Mathf.Lerp(b.center.y, b.max.y, 0.15f), b.center.z);
                Vector3 min = new Vector3(b.center.x, Mathf.Lerp(b.center.y, b.min.y, 0.1f), b.center.z);

                camLookAt.position = Vector3.Lerp(min, max, t);
            }
            else
            {
                camLookAt.position = b.center;
            }
            UpdateMainCameraZoom(b);
        }

        if (timeUntilEndFrontFlashes > 0) {
            timeUntilEndFrontFlashes = Mathf.Clamp(timeUntilEndFrontFlashes - Time.deltaTime, 0, float.MaxValue);
            if (timeUntilEndFrontFlashes == 0) {
                EnableFrontFlashes(false);
            }
        }
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

    private void EnableMidFlashes(bool enable) {
        MidFlashes.gameObject.SetActive(enable);
    }

    private void EnableFrontFlashes(bool enable)
    {
        FrontFlashes.gameObject.SetActive(enable);
    }


}

public class ColliderInfoMap {
    public Dictionary<Collider, float> history = new Dictionary<Collider, float>();
    public List<Collider> active = new List<Collider>();
}
