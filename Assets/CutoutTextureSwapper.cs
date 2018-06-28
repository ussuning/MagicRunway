using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutoutTextureSwapper : MonoBehaviour {

    public Texture2D alphaMap;
	// Use this for initialization
	void Start () {
        /* Take the original material, copy it, take its texture and make a 
         * new one that combines it with the alphaMap.
         * Apply alpha map to new (copied) material.
         * Swap original material with new material 
        */
        Material mat = GetComponent<Renderer>().sharedMaterial;
        Texture2D srcTex = mat.mainTexture as Texture2D;
        if (srcTex == null) {
            // Material had no texture, so we will just create one;
            srcTex = new Texture2D(alphaMap.width, alphaMap.height);
        }
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

        Texture2D neoTex = new Texture2D(alphaMap.width, alphaMap.height);
        neoTex.SetPixels(srcPixels);
        neoTex.Apply();

        Material neoMat = new Material(mat);
        neoMat.name += "(Copy)";

        // This effectively sets the material to "Cutout" Rendering Mode.
        neoMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        neoMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        neoMat.SetInt("_ZWrite", 1);
        neoMat.EnableKeyword("_ALPHATEST_ON");
        neoMat.DisableKeyword("_ALPHABLEND_ON");
        neoMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        neoMat.renderQueue = 2450;

        neoMat.mainTexture = neoTex;
        GetComponent<Renderer>().sharedMaterial = neoMat;

	}
}
