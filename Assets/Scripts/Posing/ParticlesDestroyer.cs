using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesDestroyer : MonoBehaviour
{
    public float lifespan = 3f;

    void Start()
    {
        Invoke("SelfDestroy", lifespan);
    }

    void SelfDestroy()
    {
        Destroy(this.gameObject);
    }
}
