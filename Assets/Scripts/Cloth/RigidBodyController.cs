using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidBodyController : MonoBehaviour {
    Rigidbody rigidbody;
    Vector3 force = Vector3.zero;
	// Use this for initialization
	void Start () {
        rigidbody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.UpArrow)) {
            force += Vector3.forward * Time.deltaTime;
            Debug.Log("force = " + force);
        }
        else if (Input.GetKey(KeyCode.DownArrow)) {
            force -= Vector3.forward * Time.deltaTime;
            Debug.Log("force = " + force);
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow)) {
            force = Vector3.zero;
            transform.localPosition = Vector3.zero;
            rigidbody.isKinematic = true;
            rigidbody.isKinematic = false;
        }
	}

	private void FixedUpdate()
	{
        rigidbody.AddForce(force);
	}
}
