using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class AnimationEventHandler : MonoBehaviour {

    ObiCloth[] cloths = null;

    public void ResetCloth() {
        Debug.Log("ResetCloth()");
        if (cloths == null) {
            cloths = GetComponentsInChildren<ObiCloth>();
        }
        foreach (ObiCloth cloth in cloths)
            cloth.ResetActor();
    }
}
