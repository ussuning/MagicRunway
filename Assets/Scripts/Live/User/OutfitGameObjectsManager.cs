﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutfitGameObjectsManager : MonoBehaviour {

    public static OutfitGameObjectsManager Instance;

    private Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();
    private Dictionary<int, Dictionary<string, GameObject>> outfitGOs = new Dictionary<int, Dictionary<string, GameObject>>();
    private Dictionary<int, GameObject> curOutfit = new Dictionary<int, GameObject>();

    void Awake()
    {
        Instance = this;
    }

    void OnDisable()
    {
        foreach(Dictionary<string, GameObject> uOutfits in outfitGOs.Values)
        {
            foreach(GameObject outfit in uOutfits.Values)
            {
                Destroy(outfit);
            }
        }
        outfitGOs.Clear();
        curOutfit.Clear();
    }

    public void ShowUserOutfit(int userIdx)
    {
        if (curOutfit.ContainsKey(userIdx))
            curOutfit[userIdx].SetActive(true);
    }

    public void HideUserOutfit(int userIdx)
    {
        if (curOutfit.ContainsKey(userIdx))
            curOutfit[userIdx].SetActive(false);
    }

    public void GenerateOutfit(Outfit outfit, int ownerIdx)
    {
        GameObject selectedOutfit;
        if (outfitGOs.ContainsKey(ownerIdx))
        {
            Dictionary<string, GameObject> userOutfits = outfitGOs[ownerIdx];
            foreach(GameObject o in userOutfits.Values)
            {
                o.SetActive(false);
            }

            if(userOutfits.ContainsKey(outfit.prefab))
            {
                selectedOutfit = userOutfits[outfit.prefab];
                selectedOutfit.SetActive(true);
            }
            else
            {
                selectedOutfit = GetOutfit(outfit, ownerIdx);
                userOutfits.Add(outfit.prefab, selectedOutfit);
            }
        }
        else
        {
            Dictionary<string, GameObject> userOutfits = new Dictionary<string, GameObject>();

            selectedOutfit = GetOutfit(outfit, ownerIdx);
            userOutfits.Add(outfit.prefab, selectedOutfit);

            outfitGOs.Add(ownerIdx, userOutfits);
        }

        if (curOutfit.ContainsKey(ownerIdx))
        {
            curOutfit[ownerIdx] = selectedOutfit;
        }
        else
        {
            curOutfit.Add(ownerIdx, selectedOutfit);
        }
    }

    public void OnUserLost(int userIdx)
    {
        if (outfitGOs.ContainsKey(userIdx))
        {
            Dictionary<string, GameObject> userOutfits = outfitGOs[userIdx];
            foreach (GameObject outfit in userOutfits.Values)
            {
                Destroy(outfit);
            }
            outfitGOs.Remove(userIdx);
        }

        if (curOutfit.ContainsKey(userIdx))
            curOutfit.Remove(userIdx);
    }

    private GameObject GetOutfit(Outfit outfit, int ownerIdx)
    {
        GameObject outfitPrefab = GetOutfitPrefab(outfit);
        GameObject outfitGO = Instantiate(outfitPrefab, this.transform);
        AvatarControllerBootstrap bootstrap = outfitGO.AddComponent<AvatarControllerBootstrap>();
        bootstrap.Init(ownerIdx);

        return outfitGO;
    }

    private GameObject GetOutfitPrefab(Outfit outfit)
    {
        GameObject outfitPrefab;
        if (prefabCache.ContainsKey(outfit.prefab))
        {
            outfitPrefab = prefabCache[outfit.prefab];
        }
        else
        {
            string sex = (outfit.sex == "f") ? "Female" : "Male";
            string path = /*"RunwayModels/" + sex + "/" +*/ outfit.prefab;

            //outfitPrefab = Resources.Load<GameObject>(path);
            AssetBundle ab = AssetBundleManager.Instance.GetAssetBundle(AssetBundles.models);
            outfitPrefab = ab.LoadAsset<GameObject>(path);
            prefabCache.Add(outfit.prefab, outfitPrefab);
        }

        return outfitPrefab;
    }
}
