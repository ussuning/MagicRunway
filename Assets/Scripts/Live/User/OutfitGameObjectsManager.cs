using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutfitGameObjectsManager : MonoBehaviour {

    public static OutfitGameObjectsManager Instance;

    private Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();
    private Dictionary<long, Dictionary<string, GameObject>> outfitGOs = new Dictionary<long, Dictionary<string, GameObject>>();

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
    }

    public void GenerateOutfit(Outfit outfit, long ownerID)
    {
        if(outfitGOs.ContainsKey(ownerID))
        {
            Dictionary<string, GameObject> userOutfits = outfitGOs[ownerID];
            foreach(GameObject o in userOutfits.Values)
            {
                o.SetActive(false);
            }

            if(userOutfits.ContainsKey(outfit.prefab))
            {
                GameObject selectedOutfit = userOutfits[outfit.prefab];
                selectedOutfit.SetActive(true);
            }
            else
            {
                GameObject selectedOutfit = GetOutfit(outfit, ownerID);
                userOutfits.Add(outfit.prefab, selectedOutfit);
            }
        }
        else
        {
            Dictionary<string, GameObject> userOutfits = new Dictionary<string, GameObject>();

            GameObject selectedOutfit = GetOutfit(outfit, ownerID);
            userOutfits.Add(outfit.prefab, selectedOutfit);

            outfitGOs.Add(ownerID, userOutfits);
        }
    }

    public void OnUserLost(long userID)
    {
        if (outfitGOs.ContainsKey(userID))
        {
            Dictionary<string, GameObject> userOutfits = outfitGOs[userID];
            foreach (GameObject outfit in userOutfits.Values)
            {
                Destroy(outfit);
            }
            outfitGOs.Remove(userID);
        }
    }

    private GameObject GetOutfit(Outfit outfit, long ownerID)
    {
        GameObject outfitPrefab = GetOutfitPrefab(outfit);
        GameObject outfitGO = Instantiate(outfitPrefab, this.transform);
        AvatarControllerBootstrap bootstrap = outfitGO.AddComponent<AvatarControllerBootstrap>();
        bootstrap.Init(KinectManager.Instance.GetUserIndexById(ownerID));

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
            string path = "RunwayModels/" + sex + "/" + outfit.prefab;

            outfitPrefab = Resources.Load<GameObject>(path);
            prefabCache.Add(outfit.prefab, outfitPrefab);
        }

        return outfitPrefab;
    }
}
