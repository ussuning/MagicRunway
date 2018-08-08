using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class aspectRatioSetter : MonoBehaviour {
    [Range(-0.01f, 10.0f)]
    public float aspectRatio = 1.0f;
    Camera cam;
	//// Use this for initialization
	//void Start () {
 //       cam = GetComponent<Camera>();
 //       aspectRatio = cam.aspect;
	//}
	
	// Update is called once per frame
	void Update () {
        if (cam.aspect != aspectRatio)
            cam.aspect = aspectRatio;
	}
}
