using UnityEngine;
using System.Collections;
using System.IO;      
using System;
using System.Collections.Generic;

public class MRData : Singleton<MRData>
{
    public Wearables wearables;
    public Outfits outfits;
    public Collections collections;

    private string wearablesFileName = "wearables.json";
    private string outfitsFileName = "outfits.json";
    private string collectionsFileName = "collections.json";

    // Use this for initialization
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void LoadEverything() {
        GetWearables();
        GetOutfits();
        GetCollections();

        AssignWearablesToOutfits();
        AssignOutfitsToCollections();
    }

    public void GetWearables() {
        string filePath = Path.Combine(Application.streamingAssetsPath, wearablesFileName);

        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            wearables = JsonUtility.FromJson<Wearables>(dataAsJson);
            Debug.Log(wearables.wearables[0].name);
        }
        else
        {
            Debug.LogError("Cannot load data!");
        }
    }

    public void GetOutfits() {
        string filePath = Path.Combine(Application.streamingAssetsPath, outfitsFileName);

        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            outfits = JsonUtility.FromJson<Outfits>(dataAsJson);
            Debug.Log(outfits.outfits[0].id);
            Debug.Log(outfits.outfits[0].name);
            Debug.Log(outfits.outfits[0].sex);
            Debug.Log(outfits.outfits[0].desc);
        }
        else
        {
            Debug.LogError("Cannot load data!");
        }
    }

    public void GetCollections()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, collectionsFileName);

        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            collections = JsonUtility.FromJson<Collections>(dataAsJson);
            Debug.Log(collections.collections[0].name);
        }
        else
        {
            Debug.LogError("Cannot load data!");
        }
    }

    private void AssignWearablesToOutfits() {
        Dictionary<string, Wearable> dicWearables = wearables.to_dict();

        foreach (Outfit outfit in outfits.outfits)
        {
            foreach (string wearableid in outfit.wearableids)
            {
                try
                {
                    Wearable wearable = dicWearables[wearableid];
                    outfit.wearables.Add(wearable);
                }
                catch
                {
                    Debug.LogError("Cannot find wearable object!");
                }
            }
        }
    }

    private void AssignOutfitsToCollections()
    {
        Dictionary<string, Outfit> dicOutfits = outfits.to_dict();

        foreach (Collection collection in collections.collections)
        {
            foreach (string outfitid in collection.outfitids)
            {
                try
                {
                    Outfit outfit = dicOutfits[outfitid];
                    collection.outfits.Add(outfit);
                }
                catch
                {
                    Debug.LogError("Cannot find wearable object!");
                }
            }
        }
    }
}
