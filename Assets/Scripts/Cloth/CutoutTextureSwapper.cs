using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CutoutTextureSwapper : MonoBehaviour {

    public Texture2D alphaMap;

    internal Material originalMaterial;
    internal Material cutoutMaterial;

	// Use this for initialization
	void Start () {
        /* Take the original material, copy it, take its texture and make a 
         * new one that combines it with the alphaMap.
         * Apply alpha map to new (copied) material.
         * Swap original material with new material 
        */
        Material mat = GetComponent<Renderer>().sharedMaterial;
        originalMaterial = mat;
        Texture2D srcTex = mat.mainTexture as Texture2D;
        if (srcTex == null) {
            // Material had no texture, so we will just create one;
            srcTex = new Texture2D(alphaMap.width, alphaMap.height);
        }
        Color [] srcPixels = GetReadableTexture(srcTex).GetPixels();
        Color[] alphaPixels = GetReadableTexture(alphaMap).GetPixels();

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
        cutoutMaterial = neoMat;
    }

    // From https://support.unity3d.com/hc/en-us/articles/206486626-How-can-I-get-pixels-from-unreadable-textures-
    Texture2D GetReadableTexture(Texture2D texture) {
        // Create a temporary RenderTexture of the same size as the texture
        RenderTexture tmp = RenderTexture.GetTemporary(
                            texture.width,
                            texture.height,
                            0,
                            RenderTextureFormat.Default,
                            RenderTextureReadWrite.Linear);

        // Blit the pixels on texture to the RenderTexture
        Graphics.Blit(texture, tmp);

        // Backup the currently set RenderTexture
        RenderTexture previous = RenderTexture.active;

        // Set the current RenderTexture to the temporary one we created
        RenderTexture.active = tmp;

        // Create a new readable Texture2D to copy the pixels to it
        Texture2D myTexture2D = new Texture2D(texture.width, texture.height);

        // Copy the pixels from the RenderTexture to the new Texture
        myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        myTexture2D.Apply();

        // Reset the active RenderTexture
        RenderTexture.active = previous;

        // Release the temporary RenderTexture
        RenderTexture.ReleaseTemporary(tmp);

        // "myTexture2D" now has the same pixels from "texture" and it's readable.
        return myTexture2D;
    }

    public void Original()
    {
        if (originalMaterial != null)
            GetComponent<Renderer>().sharedMaterial = originalMaterial;
        else
            Debug.LogError("No originalMaterial!");
    }

    public void Cutout()
    {
        if (cutoutMaterial != null)
            GetComponent<Renderer>().sharedMaterial = cutoutMaterial;
        else
            Debug.LogError("No cutoutMaterial!");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CutoutTextureSwapper))]
public class CutoutTextureSwapperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CutoutTextureSwapper myScript = (CutoutTextureSwapper)target;
        if (GUILayout.Button("Original"))
        {
            myScript.Original();
        }
        if (GUILayout.Button("Cutout"))
        {
            myScript.Cutout();
        }
    }
}
#endif
