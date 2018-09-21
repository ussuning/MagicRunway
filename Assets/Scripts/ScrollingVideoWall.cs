using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingVideoWall : MonoBehaviour {
    public Renderer mesh;

    private float scrollSpeed = 0.01f;
    private float scrollSpeed2 = 0.01f;

    private Material videoMaterial;

    void Awake()
    {
        Material[] mats = mesh.sharedMaterials;
        videoMaterial = mats[1];
    }

    private void FixedUpdate()
    {
        float offsetX = Time.time * scrollSpeed;
        float offsetY = Time.time * scrollSpeed2;

        //renderer.material.mainTextureOffset = Vector2(offset2, -offset);
        videoMaterial.mainTextureOffset = new Vector2(offsetX, -offsetY);
    }
}
