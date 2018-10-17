using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingVideoWall : MonoBehaviour {
    public Renderer mesh;

    private float scrollSpeed = 0.005f;
    private float scrollSpeed2 = 0.005f;

    private Material videoMaterial;
    private bool isRunning = true;

    void Awake()
    {
        Material[] mats = mesh.sharedMaterials;
        videoMaterial = mats[1];
    }

    private void OnDestroy()
    {
        videoMaterial.mainTextureOffset = Vector2.zero;
    }

    public void Run()
    {
        isRunning = true;
    }

    public void Freeze()
    {
        isRunning = false;
    }

    private void FixedUpdate()
    {
        if (isRunning)
        {
            float offsetX = Time.time * scrollSpeed;
            float offsetY = Time.time * scrollSpeed2;

            videoMaterial.mainTextureOffset = new Vector2(offsetX, -offsetY);
        }
    }
}
