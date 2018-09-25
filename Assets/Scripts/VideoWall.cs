using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoWall : MonoBehaviour {
    public GameObject videoWall;

    private byte videoFadeState;
    private float videoFadeStartTime;
    private float videoFadeDeltaTime;
    private float videoFadeDuration = 1.5f;
    private float videoColor = 0;

    private Material videoMaterial;
    private Texture2D videoSplash;

    void Awake () {
        Material[] mats = videoWall.GetComponent<Renderer>().sharedMaterials;
        videoMaterial = mats[1];
    }

    private void OnEnable()
    {
        FadeOut(false);
    }

    private void OnDisable()
    {
        FadeOut(false);
        Resources.UnloadAsset(videoSplash);
    }

    public void ChangeAndFadeIn(string resourceImg)
    {
        Change(resourceImg);
        FadeIn();
    }

    public void Change(string resourceImg)
    {
        if(videoSplash != null)
            Resources.UnloadAsset(videoSplash);

        //videoSplash = Resources.Load<Texture2D>(resourceImg);
        AssetBundle ab = AssetBundleManager.Instance.GetAssetBundle("videowall.assetbundle");
        videoSplash = ab.LoadAsset<Texture2D>(resourceImg);
        videoMaterial.SetTexture("_MainTex", videoSplash);
        videoMaterial.SetTexture("_EmissionMap", videoSplash);
    }

    public void FadeIn()
    { 
        videoMaterial.SetColor("_EmissionColor", Color.black);
        
        videoMaterial.color = Color.black;
        videoFadeStartTime = Time.realtimeSinceStartup;
        videoColor = 0;
        videoFadeState = 1;
    }

    public void FadeOut(bool animate = true)
    {
        if (animate == false)
        {
            videoFadeState = 0;
            videoMaterial.SetColor("_EmissionColor", Color.black);
            videoMaterial.color = Color.black;
            return;
        }

        videoMaterial.SetColor("_EmissionColor", Color.white);
        videoMaterial.color = Color.white;
        videoFadeStartTime = Time.realtimeSinceStartup;
        videoColor = 0;
        videoFadeState = 2;
    }

    void Update () {
        if (videoFadeState == 1)
        {
            videoMaterial.SetColor("_EmissionColor", Color.Lerp(Color.black, Color.white * 1.5f, videoColor));
            videoMaterial.color = Color.Lerp(Color.black, Color.white, videoColor);
            if (videoColor < 1)
            {
                videoColor += Time.deltaTime / videoFadeDuration;
            }
            else
            {
                videoMaterial.SetColor("_EmissionColor", Color.white * 1.5f);
                videoMaterial.color = Color.white;

                videoFadeState = 0;
            }
        }

        if (videoFadeState == 2)
        {
            videoMaterial.SetColor("_EmissionColor", Color.Lerp(Color.white, Color.black, videoColor));
            videoMaterial.color = Color.Lerp(Color.white, Color.black, videoColor);
            if (videoColor < 1)
            {
                videoColor += Time.deltaTime / videoFadeDuration;
            }
            else
            {
                videoMaterial.SetColor("_EmissionColor", Color.black);
                videoMaterial.color = Color.black;

                videoFadeState = 0;
            }
        }
    }
}
