using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelColliderTrigger : MonoBehaviour {

    public Collider runwayEnd;
    public Collider runwayExit;
    public Collider runwayFinish;

    public Dictionary<string, bool> colliderTriggered = new Dictionary<string, bool>();

    protected Animator animator;
    AnimationEventHandler animEvtHandler;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animEvtHandler = GetComponent<AnimationEventHandler>();
        runwayEnd = GameObject.Find("RunwayEnd").GetComponent<Collider>();
        runwayExit = GameObject.Find("RunwayExit").GetComponent<Collider>();
        runwayFinish = GameObject.Find("RunwayFinish").GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("RunwayEnd:OnTriggerEnter " + other.name);
        if (other == runwayEnd)
        {
            if (colliderTriggered.ContainsKey(other.name) == false)
            {
                colliderTriggered.Add(other.name, true);
                animator.SetTrigger("RunwayEndReached");
            }
        }
        else if (other == runwayExit)
        {
            // Only trigger exit if we've already reached the end of the runway.
            if (colliderTriggered.ContainsKey(runwayEnd.name)) {
                animEvtHandler.ExitRunway();
            }
        }
        else if (other == runwayFinish)
        {
            transform.position = GameObject.Find("RunwaySpawn").transform.position;
            animEvtHandler.faceTransform2D("RunwaySpawnLookAt");
            animEvtHandler.Reset();
            animator.SetTrigger("Reset");
            Reset();
        }
    }

    public void Reset()
    {
        colliderTriggered.Clear();
    }
}
