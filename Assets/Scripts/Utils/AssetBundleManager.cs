using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleManager : Singleton<AssetBundleManager> {

    protected Dictionary<AssetBundles, string> assetBundleNameLookup = new Dictionary<AssetBundles, string>()
    {
        {AssetBundles.models, "models.assetbundle" },
        {AssetBundles.clothingIcons, "clothing_icons.assetbundle" },
        {AssetBundles.videowall, "videowall.assetbundle" },
        {AssetBundles.cutouttextureswapper, "cutouttextureswapper.assetbundle" },
        {AssetBundles.textdata, "textdata.assetbundle" }
    };

    public Dictionary<string, AssetBundle> loadedBundles = new Dictionary<string, AssetBundle>();

    public AssetBundle GetAssetBundle(AssetBundles bundle)
    {
        string name = assetBundleNameLookup[bundle];
        if (name == null)
        {
            Debug.LogError("No string name for AssetBundle enum " + bundle.ToString());
            return null;
        }

        AssetBundle ab = GetAssetBundleByName(name);
        if (ab == null)
        {
            Debug.LogError("Failed to load asset bundle named " + name);
            return null;
        }

        return ab;
    }

    public AssetBundle GetAssetBundleByName(string name) { 
        if (loadedBundles.ContainsKey(name) == false ||
            loadedBundles[name] == null)
        {
            string platform = "/"; ///StandaloneWindows
            string fullFilePath = Application.streamingAssetsPath + platform + name;
            Debug.Log("Loading AssetBundle at " + fullFilePath);
            AssetBundle myLoadedAssetBundle = AssetBundle.LoadFromFile(fullFilePath);
            if (myLoadedAssetBundle == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return null;
            }
            loadedBundles.Add(name, myLoadedAssetBundle);
        }
        return loadedBundles[name];
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        foreach (AssetBundle ab in loadedBundles.Values)
            ab.Unload(false);

        loadedBundles.Clear();
    }
}


public enum AssetBundles
{
    models,
    clothingIcons,
    videowall,
    cutouttextureswapper,
    textdata
}
