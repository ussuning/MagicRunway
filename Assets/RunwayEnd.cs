using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunwayEnd : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("RunwayEnd:OnTriggerEnter " + other.name);
        Animator animator = other.GetComponent<Animator>();
        if (animator != null) {
            other.enabled = false;
            animator.SetTrigger("RunwayEndReached");
        }
    }
}