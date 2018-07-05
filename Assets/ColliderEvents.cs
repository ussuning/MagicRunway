using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderEvents : MonoBehaviour {

    public delegate void ColliderEventHandler(Collider other);
    public event ColliderEventHandler OnTriggerEnterEvt;
    public event ColliderEventHandler OnTriggerExitEvt;
    public event ColliderEventHandler OnTriggerStayEvt;
    public delegate void CollisionEventHandler(Collision collision);
    public event CollisionEventHandler OnCollisionEnterEvt;
    public event CollisionEventHandler OnCollisionExitEvt;
    public event CollisionEventHandler OnCollisionStayEvt;

    void OnCollisionEnter(Collision collision) { OnCollisionEnterEvt?.Invoke(collision); }
    void OnCollisionExit(Collision collision) { OnCollisionExitEvt?.Invoke(collision); }
    void OnCollisionStay(Collision collision) { OnCollisionStayEvt?.Invoke(collision); }

    void OnTriggerEnter(Collider other) { OnTriggerEnterEvt?.Invoke(other); }
    void OnTriggerExit(Collider other) { OnTriggerExitEvt?.Invoke(other); }
    void OnTriggerStay(Collider other) { OnTriggerStayEvt?.Invoke(other); }
}
