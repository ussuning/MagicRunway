using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutoutTextureSwapper : MonoBehaviour {

    public Texture2D alphaMap;
	// Use this for initialization
	void Start () {
        Material mat = GetComponent<Renderer>().sharedMaterial;
        Texture2D srcTex = mat.mainTexture as Texture2D;
        Color [] srcPixels = srcTex.GetPixels();
        Color[] alphaPixels = alphaMap.GetPixels();

        if (alphaPixels.Length != srcPixels.Length) {
            Debug.LogError("Mismatching texture sizes! This won't work!");
            return;
        }

        for (int i = 0; i < srcPixels.Length; i++)
        {
            srcPixels[i].a = alphaPixels[i].r;
        }

        // 
        Texture2D neoTex = new Texture2D(mat.mainTexture.width, mat.mainTexture.height);
        neoTex.SetPixels(srcPixels);
        neoTex.Apply();

        Material neoMat = new Material(mat);
        neoMat.mainTexture = neoTex;
        GetComponent<Renderer>().sharedMaterial = neoMat;

	}
}
