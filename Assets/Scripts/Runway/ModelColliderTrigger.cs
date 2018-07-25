using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelColliderTrigger : MonoBehaviour {

    public ColliderEvents runwayEnd;
    public ColliderEvents runwayExit;
    public ColliderEvents runwayFinish;

    public Dictionary<string, bool> colliderTriggered = new Dictionary<string, bool>();

    protected Animator animator;
    AnimationEventHandler animEvtHandler;
    bool listenersAdded = false;

    private void Awake()
    {
        Init();
    }

    private void Init() { 
        animator = GetComponent<Animator>();
        animEvtHandler = GetComponent<AnimationEventHandler>();

        if (runwayEnd == null)
            runwayEnd = GameObject.Find("RunwayEnd")?.GetComponent<ColliderEvents>();
        if (runwayExit == null)
            runwayExit = GameObject.Find("RunwayExit")?.GetComponent<ColliderEvents>();
        if (runwayFinish == null)
            runwayFinish = GameObject.Find("RunwayFinish")?.GetComponent<ColliderEvents>();

        AddListeners();
    }

    private void AddListeners()
    {
        if (listenersAdded)
            return;

        runwayEnd.OnTriggerEnterEvt += OnRunwayEndTriggerEnter;
        runwayExit.OnTriggerEnterEvt += OnRunwayExitTriggerEnter;
        runwayFinish.OnTriggerEnterEvt += OnRunwayFinishTriggerEnter;
        listenersAdded = true;
    }

    private void RemoveListeners()
    {
        runwayEnd.OnTriggerEnterEvt -= OnRunwayEndTriggerEnter;
        runwayExit.OnTriggerEnterEvt -= OnRunwayExitTriggerEnter;
        runwayFinish.OnTriggerEnterEvt -= OnRunwayFinishTriggerEnter;
        listenersAdded = false;
    }

    private void OnDestroy()
    {
        RemoveListeners();
    }

    private void OnDisable()
    {
        RemoveListeners();
    }

    private void OnEnable()
    {
        Init();
    }


    private void OnRunwayFinishTriggerEnter(Collider other)
    {
        // Only care if RunwayFinish was collided into by this object. The collider on this gameObject of interest
        // is always on this.gameObject or a single level child (i.e. Mixamo::Hips)
        if (other.gameObject != this.gameObject && other.transform.parent.gameObject != this.gameObject)
            return;

        //Debug.Log("ModelColliderTrigger::OnRunwayFinishTriggerEnter " + other.name);
        if (colliderTriggered.ContainsKey(runwayFinish.name) == false)
        {
            colliderTriggered.Remove(other.name);
            transform.position = GameObject.Find("RunwaySpawn").transform.position;
            animEvtHandler.faceTransform2D("RunwaySpawnLookAt");
            animEvtHandler.Reset();
            animator.SetTrigger("Reset");
            Reset();
        }
    }

    private void OnRunwayExitTriggerEnter(Collider other)
    {
        // Only care if RunwayFinish was collided into by this object. The collider on this gameObject of interest
        // is always on this.gameObject or a single level child (i.e. Mixamo::Hips)
        if (other.gameObject != this.gameObject && other.transform.parent.gameObject != this.gameObject)
            return;

        //Debug.Log("ModelColliderTrigger::OnRunwayExitTriggerEnter " + other.name);
        // Only trigger exit if we've already reached the end of the runway.
        if (colliderTriggered.ContainsKey(runwayEnd.name))
        {
            animEvtHandler.ExitRunway();
        }
    }

    private void OnRunwayEndTriggerEnter(Collider other)
    {
        // Only care if RunwayFinish was collided into by this object. The collider on this gameObject of interest
        // is always on this.gameObject or a single level child (i.e. Mixamo::Hips)
        if (other.gameObject != this.gameObject && other.transform.parent.gameObject != this.gameObject)
            return;

        //Debug.Log("ModelColliderTrigger::OnRunwayEndTriggerEnter " + other.name);
        if (colliderTriggered.ContainsKey(runwayEnd.name) == false)
        {
            colliderTriggered.Add(runwayEnd.name, true);
        }
        animator.SetTrigger("RunwayEndReached");
    }

    public void Reset()
    {
        colliderTriggered.Clear();
    }
}
