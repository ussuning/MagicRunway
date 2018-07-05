using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunwayCamera : MonoBehaviour {

    public ColliderEvents RunwayEnterEvents;
    public ColliderEvents RunwayMidEvents;
    public ColliderEvents RunwayExitEvents;
    public Transform camLookAt;
    //Transform active;


    ColliderInfoMap mid = new ColliderInfoMap();
    ColliderInfoMap onRunway = new ColliderInfoMap();

    protected Camera cam;
    protected float timeSinceSlowMo;

	// Use this for initialization
	void Start () {
        cam = GetComponent<Camera>();

        RunwayMidEvents.OnTriggerEnterEvt += OnTriggerEnterRunwayMid;
        RunwayMidEvents.OnTriggerExitEvt += OnTriggerExitRunwayMid;

        RunwayEnterEvents.OnTriggerEnterEvt += OnRunwayEnter;
        RunwayExitEvents.OnTriggerEnterEvt += OnRunwayExit;
	}

    private void OnDestroy()
    {
        RunwayMidEvents.OnTriggerEnterEvt -= OnTriggerEnterRunwayMid;
        RunwayMidEvents.OnTriggerExitEvt -= OnTriggerExitRunwayMid;
    }

    private void OnTriggerEnterRunwayMid(Collider other)
    {
        Debug.Log("OnTriggerEnterRunwayMid " + other.name);

        if (other.GetComponent<Animator>() == null)
            return;

        if (mid.history.ContainsKey(other) == false) {
            mid.history.Add(other, true);
            mid.active.Add(other);
        }

        CheckActives();
    }

    private void OnTriggerExitRunwayMid(Collider other)
    {
        Debug.Log("OnTriggerExitRunwayMid " + other.name);
        if (mid.active.Contains(other))
            mid.active.Remove(other);

        CheckActives();
    }

    private void OnRunwayExit(Collider other)
    {
        if (onRunway.active.Contains(other))
            onRunway.active.Remove(other);
    }

    private void OnRunwayEnter(Collider other)
    {
        if (other.GetComponent<Animator>() == null)
            return;
        
        if (onRunway.history.ContainsKey(other) == false)
        {
            onRunway.history.Add(other, true);
            onRunway.active.Add(other);
        }
    }

    void CheckActives() {
        if (mid.active.Count > 0) {
            cam.fieldOfView = 20;
            Time.timeScale = 0.5f;
            timeSinceSlowMo = 0;
        } else {
            cam.fieldOfView = 60;
            Time.timeScale = 1.0f;
        }
    }

    private void Update()
    {
        if (onRunway.active.Count > 0) {
            Bounds b = onRunway.active[0].bounds;
            if (Time.timeScale < 1.0f)
            {
                timeSinceSlowMo += Time.unscaledDeltaTime;
                float t = Mathf.Sin(timeSinceSlowMo / 4.0f);

                camLookAt.position = Vector3.Lerp(b.center, new Vector3(b.center.x, b.min.y, b.center.z), t);
            }
            else
            {
                camLookAt.position = b.center;
                //active = onRunway.active[0].transform;
            }
        }
        cam.transform.LookAt(camLookAt);
    }

}

class ColliderInfoMap {
    public Dictionary<Collider, bool> history = new Dictionary<Collider, bool>();
    public List<Collider> active = new List<Collider>();
}
