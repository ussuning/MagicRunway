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

    static Material additiveMaterial;
    static Material invertedAdditiveMaterial;
    static Texture2D blackTexture1x1;
    static Material alphaMaskMat;

    public bool isGeneratingCutoutMaterial = false;

    private void Awake()
    {
        if (additiveMaterial == null)
        {
            additiveMaterial = Resources.Load<Material>("Materials/BodyAlphaBlitter");
            if (additiveMaterial == null)
                Debug.LogError("Unabled to load blitter material at Materials/BodyAlphaBlitter");
        }
        if (invertedAdditiveMaterial == null)
        {
            invertedAdditiveMaterial = Resources.Load<Material>("Materials/InvertColors");
            if (invertedAdditiveMaterial == null)
                Debug.LogError("Unabled to load invertColorsMaterial at Materials/InvertColors");
        }
        if (blackTexture1x1 == null)
        {
            blackTexture1x1 = Resources.Load<Texture2D>("Textures/blackTexture1x1");
            if (blackTexture1x1 == null)
                Debug.LogError("Unabled to load blackTexture1x1 at Textures/blackTexture1x1");
        }
        if (alphaMaskMat == null)
        {
            alphaMaskMat = new Material(Shader.Find("Unlit/AlphaMask"));
        }
    }

    // Use this for initialization
    void Start()
    {

        //if (cutoutMaterial == null)
        //    GenerateCutoutMaterial();
        //Generate();
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
        StartCoroutine(DoGenerateCutoutMaterial());
    }

    // Variables for GenerateCoutoutMaterial()
    Texture2D[] alphaMaps;
    Vector2Int alphaMapSize;
    Texture2D srcTex;
    Material mat;
    Texture2D alphaMapsTexture;
    RenderTexture tmpRenderTex;

    IEnumerator DoGenerateCutoutMaterial()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying == false)
        {
            Debug.LogError("Can not Generate Cutout Material while not playing!");
            yield break;
        }
#endif
        // Check if already started.
        if (isGeneratingCutoutMaterial)
            yield break;

        isGeneratingCutoutMaterial = true;

        yield return StartCoroutine(Step1());
        yield return StartCoroutine(Step2());
        yield return StartCoroutine(Step3());
        yield return StartCoroutine(Step4());

        isGeneratingCutoutMaterial = false;
        Debug.Log("DONE DoGenerateCutoutMaterial()");
    }

    IEnumerator Step1()
    {
        alphaMaps = new Texture2D[] { alphaMap, alphaMap2, alphaMap3, alphaMap4, alphaMap5 };
        alphaMapSize = Vector2Int.one;
        if (validateAlphaMaps(alphaMaps, out alphaMapSize) == false)
        {
            Debug.LogError("AlphaMap arrays are not valid! Aborting!");
            yield break;
        }

        /* Take the original material, copy it, take its texture and make a 
         * new one that combines it with the alphaMap.
         * Apply alpha map to new (copied) material.
         * Swap original material with new material 
        */
        mat = GetComponent<Renderer>().sharedMaterial;
        originalMaterial = mat;
        srcTex = mat.mainTexture as Texture2D;
        if (srcTex == null)
        {
            // Material had no texture, so we will just create one;
            srcTex = GetReusableTexture(alphaMapSize.x, alphaMapSize.y, 0);
        }
        //Color [] srcPixels = GetReadableTexture(srcTex).GetPixels();

        Debug.Log("DONE Step1()");
    }

    IEnumerator Step2()
    {
        yield return new WaitForSeconds(0.1f);
        // Combine alpha maps, using the most transparent value for each pixel.
        alphaMapsTexture = GetReadableTexture(alphaMaps, 1);
        if (alphaMapsTexture == null)
        {
            Debug.Log("Invalid alpha maps. Aborting.");
            yield break;
        }
        Debug.Log("DONE Step2()");
    }

    IEnumerator Step3()
    {
        yield return new WaitForSeconds(0.1f);
        // Create temporary render texture and set the alphaMapsTexture as the alphaMask
        tmpRenderTex = RenderTexture.GetTemporary(
                    alphaMapSize.x,
                    alphaMapSize.y,
                    16,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.sRGB);
        alphaMaskMat.SetTexture("_AlphaTex", alphaMapsTexture);
        Graphics.Blit(srcTex, tmpRenderTex, alphaMaskMat);

    }

    IEnumerator Step4()
    {
        yield return new WaitForSeconds(0.1f);
        // Backup the currently set RenderTexture
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmpRenderTex;

        Texture2D neoTex = new Texture2D(tmpRenderTex.width, tmpRenderTex.height);
        neoTex.ReadPixels(new Rect(0, 0, tmpRenderTex.width, tmpRenderTex.height), 0, 0);
        neoTex.Apply();

        RenderTexture.active = previous;


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
        // Cleanup old cutoutMaterial
        cutoutMaterial = neoMat;

        // Cleanup
        RenderTexture.ReleaseTemporary(tmpRenderTex);
        Resources.UnloadUnusedAssets();

        Debug.Log("DONE Step4()");
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

    internal static Dictionary<string, RenderTexture> renderTexturesBySize = new Dictionary<string, RenderTexture>();

    static RenderTexture GetRenderTexture(int width, int height)
    {
        string key = "" + width + "_" + height;
        if (renderTexturesBySize.ContainsKey(key) == false)
        {
            RenderTexture tmp = RenderTexture.GetTemporary(
                                width,
                                height,
                                16,
                                RenderTextureFormat.Default,
                                RenderTextureReadWrite.sRGB);
            tmp.name = "tmpRender_" + key;
            renderTexturesBySize.Add(key, tmp);
        }
        return renderTexturesBySize[key];
    }

    internal static Dictionary<int, Dictionary<string, Texture2D>> readableTexturesBySizeAndLayer = new Dictionary<int, Dictionary<string, Texture2D>>();

    static Texture2D GetReusableTexture(int width, int height, int layer = 0)
    {
        // Ensure layer is initialized
        if (readableTexturesBySizeAndLayer.ContainsKey(layer) == false)
            readableTexturesBySizeAndLayer.Add(layer, new Dictionary<string, Texture2D>());

        // search for key.
        string key = "" + width + "_" + height;
        if (readableTexturesBySizeAndLayer[layer].ContainsKey(key) == false)
        {
            Texture2D tmp = new Texture2D(width, height, TextureFormat.ARGB32, false);
            tmp.name = "reusable_" + key;
            readableTexturesBySizeAndLayer[layer].Add(key, tmp);
        }
        return readableTexturesBySizeAndLayer[layer][key];
    }

    Texture2D GetReadableTexture(Texture2D texture, int layer = 0)
    {
        return GetReadableTexture(new Texture2D[] { texture }, layer);
    }

    // From https://support.unity3d.com/hc/en-us/articles/206486626-How-can-I-get-pixels-from-unreadable-textures-
    Texture2D GetReadableTexture(Texture2D[] texturesIn, int layer = 0)
    {
        // Filter out null textures.
        List<Texture2D> textures = new List<Texture2D>();
        foreach (Texture2D tex in texturesIn)
            if (tex != null)
                textures.Add(tex);

        if (textures.Count == 0)
            return null;

        // Create a temporary RenderTexture of the same size as the texture
        RenderTexture tmp = GetRenderTexture(textures[0].width, textures[0].height);

        // Clear the renderTexture to black
        Graphics.Blit(blackTexture1x1, tmp, new Vector2(tmp.width, tmp.height), Vector2.zero);

        // Blit the pixels on texture to the RenderTexture
        foreach (Texture2D texture in textures)
        {
            // layer 1 is for alpha maps, which actually need to be inverted during the additive blitting process because
            // the black regions are the ones that we want to add up, so we will invert the colors before blitting additively.
            Graphics.Blit(texture, tmp, layer == 1 ? invertedAdditiveMaterial : additiveMaterial);
        }

        // Backup the currently set RenderTexture
        RenderTexture previous = RenderTexture.active;

        // Set the current RenderTexture to the temporary one we created
        RenderTexture.active = tmp;

        // Create a new readable Texture2D to copy the pixels to it
        Texture2D myTexture2D = GetReusableTexture(textures[0].width, textures[0].height, layer);

        // Copy the pixels from the RenderTexture to the new Texture
        myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        myTexture2D.Apply();


        if (layer == 1)
        {
            // Clear the renderTexture to black
            Graphics.Blit(blackTexture1x1, tmp, new Vector2(tmp.width, tmp.height), Vector2.zero);
            // Invert the 
            Graphics.Blit(myTexture2D, tmp, invertedAdditiveMaterial);

            // Copy the pixels from the RenderTexture to the new Texture
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();
        }

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
            Debug.LogWarning("No originalMaterial!");
    }

    public void UseCutout()
    {
        if (cutoutMaterial != null)
            GetComponent<Renderer>().sharedMaterial = cutoutMaterial;
        else
            Debug.LogWarning("No cutoutMaterial!");
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
