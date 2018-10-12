using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleManager : Singleton<AssetBundleManager> {

    protected Dictionary<AssetBundles, string> assetBundleNameLookup = new Dictionary<AssetBundles, string>()
    {
        {AssetBundles.models01, "models01.assetbundle" },
        {AssetBundles.models02, "models02.assetbundle" },
        {AssetBundles.models03, "models03.assetbundle" },
        {AssetBundles.models04, "models04.assetbundle" },
        {AssetBundles.models05, "models05.assetbundle" },
        {AssetBundles.models06, "models06.assetbundle" },
        {AssetBundles.models07, "models07.assetbundle" },
        {AssetBundles.models08, "models08.assetbundle" },
        {AssetBundles.clothingIcons, "clothing_icons.assetbundle" },
        {AssetBundles.videowall, "videowall.assetbundle" },
        {AssetBundles.cutouttextureswapper, "cutouttextureswapper.assetbundle" },
        {AssetBundles.textdata, "textdata.assetbundle" }
    };

    public Dictionary<string, AssetBundle> loadedBundles = new Dictionary<string, AssetBundle>();
    public MultiBundleManager modelsBundleManager;

    protected void Start()
    {
        modelsBundleManager = new MultiBundleManager(this, new AssetBundles[]
        {
            AssetBundles.models01,
            AssetBundles.models02,
            AssetBundles.models03,
            AssetBundles.models04,
            AssetBundles.models05,
            AssetBundles.models06,
            AssetBundles.models07,
            AssetBundles.models08
        });
    }

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

    internal GameObject GetModelAsset(string modelName)
    {
        Debug.Log("Loading model " + modelName);
        return modelsBundleManager.LoadAsset<GameObject>(modelName);
    }
}

public class MultiBundleManager 
{
    AssetBundles[] assetBundles;
    protected Dictionary<string, AssetBundles> lookup = null;
    AssetBundleManager abManager;

    public MultiBundleManager(AssetBundleManager assetBundleManager, AssetBundles[] bundles)
    {
        abManager = assetBundleManager;
        assetBundles = bundles;
    }

    public void InitLookups()
    {
        lookup = new Dictionary<string, AssetBundles>();
        foreach (AssetBundles abEnum in assetBundles)
        {
            AssetBundle ab = abManager.GetAssetBundle(abEnum);
            foreach (string abAsset in ab.GetAllAssetNames())
            {
                string [] abAssetParts = abAsset.Split('/');
                string abAssetName = abAssetParts[abAssetParts.Length - 1];
                string assetName = abAssetName.Split('.')[0]; // remove extension
                Debug.Log(assetName + " -> " + abEnum);
                lookup.Add(assetName, abEnum);
            }
        }
    }

    public T LoadAsset<T>(string assetName) where T : UnityEngine.Object
    {
        // Initialize lookups
        if (lookup == null)
            InitLookups();

if(!lookup.ContainsKey(assetName))
{
    Debug.Log("can't find " + assetName);
    Debug.Break();
}
        AssetBundles abName = lookup[assetName];
        AssetBundle ab = abManager.GetAssetBundle(abName);
        Type type = typeof(T);
        return ab.LoadAsset<T>(assetName);
    }
}





public enum AssetBundles
{
    models01,
    models02,
    models03,
    models04,
    models05,
    models06,
    models07,
    models08,
    clothingIcons,
    videowall,
    cutouttextureswapper,
    textdata
}
