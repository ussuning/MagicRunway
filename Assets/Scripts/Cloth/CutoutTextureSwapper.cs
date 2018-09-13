using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CutoutTextureSwapper : MonoBehaviour
{

    public Texture2D alphaMap;
    public Texture2D alphaMap2;
    public Texture2D alphaMap3;
    public Texture2D alphaMap4;
    public Texture2D alphaMap5;

    internal Material originalMaterial;
    internal Material cutoutMaterial;

    // Use this for initialization
    void Start()
    {

        if (cutoutMaterial == null)
            GenerateCutoutMaterial();
    }

    public void ClearAlphaMaps()
    {
        alphaMap = alphaMap2 = alphaMap3 = alphaMap4 = alphaMap5 = null;
    }

    public void AddAlphaMap(Texture2D neoAlphaMap)
    {
        if (alphaMap == null)
            alphaMap = neoAlphaMap;
        else
        if (alphaMap2 == null)
            alphaMap2 = neoAlphaMap;
        else
        if (alphaMap3 == null)
            alphaMap3 = neoAlphaMap;
        else
        if (alphaMap4 == null)
            alphaMap4 = neoAlphaMap;
        else
        if (alphaMap5 == null)
            alphaMap5 = neoAlphaMap;
        else
            Debug.LogError("Ran out of alphaMap slots!");
    }

    void GenerateCutoutMaterial()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying == false)
        {
            Debug.LogError("Can not Generate Cutout Material while not playing!");
            return;
        }
#endif
        Texture2D[] alphaMaps = new Texture2D[] { alphaMap, alphaMap2, alphaMap3, alphaMap4, alphaMap5 };
        Vector2Int alphaMapSize = Vector2Int.one;
        if (validateAlphaMaps(alphaMaps, out alphaMapSize) == false)
        {
            Debug.LogError("AlphaMap arrays are not valid! Aborting!");
            return;
        }

        /* Take the original material, copy it, take its texture and make a 
         * new one that combines it with the alphaMap.
         * Apply alpha map to new (copied) material.
         * Swap original material with new material 
        */
        Material mat = GetComponent<Renderer>().sharedMaterial;
        originalMaterial = mat;
        Texture2D srcTex = mat.mainTexture as Texture2D;
        if (srcTex == null)
        {
            // Material had no texture, so we will just create one;
            srcTex = new Texture2D(alphaMapSize.x, alphaMapSize.y);
        }
        Color[] srcPixels = GetReadableTexture(srcTex).GetPixels();

        // Combine alpha maps, using the most transparent value for each pixel.
        for (int i = 0; i < alphaMaps.Length; i++)
        {
            Texture2D alphaMap = alphaMaps[i];
            // skip if null.
            if (alphaMap == null)
                continue;

            Color[] alphaPixels = GetReadableTexture(alphaMap).GetPixels();

            if (alphaPixels.Length != srcPixels.Length)
            {
                Debug.LogError("Mismatching texture sizes! This won't work! Aborting!");
                return;
            }

            for (int j = 0; j < srcPixels.Length; j++)
            {
                // if incoming alphapixels are more transparent (closer to 0), then use it.
                if (alphaPixels[j].r < srcPixels[j].a)
                    srcPixels[j].a = alphaPixels[j].r;
            }

        }
        Texture2D neoTex = new Texture2D(alphaMapSize.x, alphaMapSize.y);
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

    bool validateAlphaMaps(Texture2D[] alphaMaps, out Vector2Int alphaMapSize)
    {
        alphaMapSize = Vector2Int.one;
        if (alphaMaps == null || alphaMaps.Length == 0)
        {
            Debug.LogError("AlphaMap array is empty!");
            return false;
        }

        for (int i = 0; i < alphaMaps.Length; i++)
        {
            // Skip if null
            if (alphaMaps[i] == null)
                continue;

            // set alphaMapSize if not set
            if (alphaMapSize == Vector2.one && alphaMaps[i].width > 0 && alphaMaps[i].height > 0)
            {
                alphaMapSize = new Vector2Int(alphaMaps[i].width, alphaMaps[i].height);
                continue;
            }

            // If alphaMapSize is set, ensure alphaMap[i] is same size.
            if (alphaMapSize != Vector2.one)
            {
                if (alphaMaps[i].width != alphaMapSize.x ||
                    alphaMaps[i].height != alphaMapSize.y)
                {
                    Debug.LogError("Inconsistent pixel width and height between alpha maps!");
                    return false;
                }
            }
        }

        return true;
    }

    static Dictionary<string, RenderTexture> renderTexturesBySize = new Dictionary<string, RenderTexture>();

    static RenderTexture GetRenderTexture(int width, int height)
    {
        string key = "" + width + height;
        if (renderTexturesBySize.ContainsKey(key) == false)
        {
            RenderTexture tmp = RenderTexture.GetTemporary(
                                width,
                                height,
                                16,
                                RenderTextureFormat.Default,
                                RenderTextureReadWrite.sRGB);
            renderTexturesBySize.Add(key, tmp);
        }
        return renderTexturesBySize[key];
    }

    // From https://support.unity3d.com/hc/en-us/articles/206486626-How-can-I-get-pixels-from-unreadable-textures-
    Texture2D GetReadableTexture(Texture2D texture)
    {
        // Create a temporary RenderTexture of the same size as the texture
        RenderTexture tmp = GetRenderTexture(texture.width, texture.height);

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
        //RenderTexture.ReleaseTemporary(tmp);

        // "myTexture2D" now has the same pixels from "texture" and it's readable.
        return myTexture2D;
    }

    public void UseOriginal()
    {
        if (originalMaterial != null)
            GetComponent<Renderer>().sharedMaterial = originalMaterial;
        else
            Debug.LogError("No originalMaterial!");
    }

    public void UseCutout()
    {
        if (cutoutMaterial != null)
            GetComponent<Renderer>().sharedMaterial = cutoutMaterial;
        else
            Debug.LogError("No cutoutMaterial!");
    }

    public void Generate()
    {
        // Restore originalMaterial, discard old cutoutMaterial, generate new cutoutMaterial
        UseOriginal();
        cutoutMaterial = null;
        GenerateCutoutMaterial();
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
            myScript.UseOriginal();
        }
        if (GUILayout.Button("Cutout"))
        {
            myScript.UseCutout();
        }
        if (GUILayout.Button("Generate Cutout"))
        {
            myScript.Generate();
        }
    }
}
#endif
