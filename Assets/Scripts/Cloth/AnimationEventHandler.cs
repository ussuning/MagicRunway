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
        turnRate = -0.25f;
        turnDuration = 4.0f;
    }

    public void IdleToWalkStart(){
        velocity = baseVelocity;
    }

    public void WalkStart()
    {
        velocity = baseVelocity;
    }

    public void IdleStart() {
        velocity = 0;
    }

    public void IdleToTwistL() {
        velocity = 0;
    }

    public void Turn180End() {
        
    }

    private void Update()
    {
        if (turnDuration != 0)
        {
            transform.RotateAround(transform.position, Vector3.up, turnRate * 90.0f * Time.deltaTime);
            turnDuration = Mathf.Clamp(turnDuration - Time.deltaTime, 0, float.MaxValue);
            if (turnDuration == 0) {
                // Walk towards runway;
                transform.LookAt(GameObject.Find("RunwayEnd").transform);
                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0); // only keep y rotation;
            }
        }
        
        if (velocity != 0)
            transform.position += transform.forward * velocity * Time.deltaTime; 


    }
}
