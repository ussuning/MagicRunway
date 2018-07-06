using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelQueueManager : MonoBehaviour {
    public List<GameObject> queue;
    public ColliderEvents RunwayMidExit;
    public ColliderEvents RunwayFinish;
	// Use this for initialization
	void Start () {
        RunwayMidExit.OnTriggerEnterEvt += OnRunwayMidExit;
        RunwayFinish.OnTriggerEnterEvt += OnRunwayFinish;
        for (int i = 1; i < queue.Count; i++) {
            queue[i].SetActive(false);
        }
	}

    private void OnRunwayFinish(Collider other)
    {
        other.gameObject.SetActive(false);
    }

    private void OnRunwayMidExit(Collider other)
    {
        if (other.gameObject == queue[0]) {
            queue.RemoveAt(0);
            queue.Add(other.gameObject);
            queue[0].SetActive(true);
        }
    }
}
