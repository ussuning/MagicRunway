using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTester : MonoBehaviour {

public float curTime = 0f;


	// Use this for initialization
	void Start () {
		curTime = 0f;
        TimeManager.instance.timeScale = 1f;
	}
	
	// Update is called once per frame
	void Update () {
        curTime += Time.deltaTime;
	}
}
