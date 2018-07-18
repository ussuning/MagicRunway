using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiveRunwayManager : MonoBehaviour {
    public GameObject cameraGroup;

    private void Start()
    {
        cameraGroup.SetActive(false);    
    }

    //Setup before starting live mode -- happens before fading in
    public void ReadyLiveRunway()
    {
        cameraGroup.SetActive(true);
    }

    //play live mode after fading in
    public void PlayLiveRunway()
    {
    }

    public void StopLiveRunway()
    {
        cameraGroup.SetActive(false);
        UIManager.Instance.HideAll();
    }
}
