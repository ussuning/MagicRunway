using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class AnimationEventHandler : MonoBehaviour {
    ObiCloth[] cloths = null;

    static float baseVelocity = 1.25f;
    float velocity = 0;
    float turnRate = 0;
    float turnDuration = 0;

    public void Reset()
    {
        ResetCloth();
        velocity = turnRate = turnDuration = 0;
    }

    public void ResetCloth() {
        Debug.Log("ResetCloth()");
        if (cloths == null) {
            cloths = GetComponentsInChildren<ObiCloth>();
        }
        foreach (ObiCloth cloth in cloths)
            cloth.ResetActor();
    }

    public void IdleStartWalk() {
        Debug.Log("StartWalk");
        GetComponent<Animator>().SetTrigger("Start");
        turnRate = -0.5f;
        turnDuration = 2.0f;
        velocity = baseVelocity;
    }

    public void IdleToWalkStart(){
    }

    public void WalkStart()
    {
    }

    public void IdleStart() {
        velocity = 0;
    }

    public void IdleToTwistL() {
        velocity = 0;
    }

    public void Turn180End() {
        faceTransform2D("RunwayExit");
        velocity = baseVelocity;
    }

    public void ExitRunway() {
        transform.LookAt(new Vector3(999, 0, 0)); // face +X
        turnRate = -0.5f;
        turnDuration = 2.0f;
    }

    private void Update()
    {
        if (turnDuration != 0)
        {
            transform.RotateAround(transform.position, Vector3.up, turnRate * 90.0f * Time.deltaTime);
            turnDuration = Mathf.Clamp(turnDuration - Time.deltaTime, 0, float.MaxValue);
            if (turnDuration == 0) {
                if (GetComponent<ModelColliderTrigger>().colliderTriggered.ContainsKey("RunwayEnd") == false)
                {
                    // Walk towards runway;
                    faceTransform2D("RunwayEnd");
                }
            }
        }
        
        if (velocity != 0)
            transform.position += transform.forward * velocity * Time.deltaTime; 
        

    }

    public void faceTransform2D(string tName) {
        Transform target = GameObject.Find(tName).transform;
        transform.LookAt(target);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0); // only keep y rotation;
    }

    public void faceWorldPos2D(Vector3 worldPos) {
        transform.LookAt(worldPos);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0); // only keep y rotation;
    }
}
