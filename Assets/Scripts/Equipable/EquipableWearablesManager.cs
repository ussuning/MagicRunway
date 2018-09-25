using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using MR;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EquipableWearablesManager : MonoBehaviour
{

    public EquipableSlots mannequin;

    public enum MannequinGender
    {
        female,
        male
    }

    public MannequinGender gender;

    public static Dictionary<EquipableSlot, Dictionary<string, Wearable>> wearablesBySlot; // wearables by slot
    public static Dictionary<string, Wearable> wearables; // wearables by id

    protected bool needGenerateBodyAlphaMap = false;
    protected float timeUntilGenerateBodyAlphaMap = regeneratePeriod;
    protected const float regeneratePeriod = 0.25f;

    // Use this for initialization
    void Start()
    {
        if (wearables == null)
        {
            wearables = new Dictionary<string, Wearable>();
            wearablesBySlot = new Dictionary<EquipableSlot, Dictionary<string, Wearable>>();

            // Initialize dictionaries for each slot.
            foreach (EquipableSlot slot in EquipableSlotIterator.allSlots)
            {
                wearablesBySlot[slot] = new Dictionary<string, Wearable>();
            }

            // Initialize MRData if necessary
            if (MRData.Instance.wearables == null || MRData.Instance.wearables.wearables == null || MRData.Instance.wearables.wearables.Count == 0)
                MRData.Instance.LoadEverything();

            // Put each wearable in the corresponding slot.
            foreach (Wearable wearable in MRData.Instance.wearables.wearables)
            {
                string slotStr = wearable.slot;
                if (slotStr != null && slotStr.Length > 0)
                {
                    EquipableSlot slot = (EquipableSlot)Enum.Parse(typeof(EquipableSlot), slotStr);
                    wearablesBySlot[slot].Add(wearable.id, wearable);
                    wearables.Add(wearable.id, wearable);
                }
            }
        }

        if (mannequin == null)
            mannequin = GetComponent<EquipableSlots>();

    }

    private void Update()
    {
        timeUntilGenerateBodyAlphaMap -= Time.unscaledDeltaTime;
        if (timeUntilGenerateBodyAlphaMap <= 0)
        {
            timeUntilGenerateBodyAlphaMap = regeneratePeriod;
            if (needGenerateBodyAlphaMap)
            {
                GameObject body = mannequin.slots[EquipableSlot.body] != null ? mannequin.slots[EquipableSlot.body].transform.FindDeepChild("body").gameObject : null;
                if (body != null)
                {
                    CutoutTextureSwapper cutoutTextureSwapper = body.GetComponent<CutoutTextureSwapper>();
                    if (cutoutTextureSwapper == null)
                        cutoutTextureSwapper = body.AddComponent<CutoutTextureSwapper>();

                    if (cutoutTextureSwapper.isGeneratingCutoutMaterial == false)
                    {
                        needGenerateBodyAlphaMap = false;
                        Debug.Log("Regenerating body alpha map.");
                        cutoutTextureSwapper.ClearAlphaMaps();

                        // Gather all cutout Texture references
                        foreach (EquipableSlot slot in EquipableSlotIterator.nonBodySlots)
                        {
                            // Get the CutoutTextureReference if available
                            CutoutTextureReference texRef = mannequin.slots[slot] != null ? mannequin.slots[slot].GetComponent<CutoutTextureReference>() : null;

                            // Add it to the cutoutTextureSwapper
                            if (texRef != null && texRef.alphaMap != null)
                                cutoutTextureSwapper.AddAlphaMap(texRef.alphaMap);
                        }

                        cutoutTextureSwapper.Generate();
                    }
                }
            }
        }
    }


    public void LoadEquipableSlotsWithWearableId(EquipableSlots equipableSlots, string wearableId)
    {
        Wearable wearable = wearables[wearableId];

        // Load wearable's prefab
        string path = GetPathForOutfitPrefab(wearable.prefab, wearable.sex);
        AssetBundle ab = AssetBundleManager.Instance.GetAssetBundle("models.assetbundle");
        if (ab == null)
        {
            Debug.LogError("Failed to load models.assetbundle!");
            return;
        }
        GameObject asset = ab.LoadAsset<GameObject>(path); // Resources.Load<GameObject>(path);
        GameObject go;
        if (asset != null)
            go = Instantiate(asset);
        else
        {
            Debug.LogError("Asset failed to load at " + path + ". Aborting Instantiation!");
            return;
        }

        // Append wearable info onto instance
        WearableInfo wearableInfo = go.AddComponent<WearableInfo>();
        wearableInfo.wearable = wearable;

        // Assign wearable instance as child of equipableSlots
        go.transform.parent = equipableSlots.transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        // Unload previous wearable if it is occupying target slot

        EquipableSlot slot = (EquipableSlot)Enum.Parse(typeof(EquipableSlot), wearable.slot);
        UnloadEquipableSlot(equipableSlots, slot);

        // Assign wearable instance into target slot
        equipableSlots.Equip(slot, go);

        needGenerateBodyAlphaMap = true;
    }

    public void UnloadEquipableSlot(EquipableSlots equipableSlots, EquipableSlot slot)
    {
        GameObject lastEquipped = equipableSlots.UnEquip(slot);
        if (lastEquipped != null)
        {
            lastEquipped.transform.parent = null;
            GameObject.Destroy(lastEquipped);
        }

        Resources.UnloadUnusedAssets();
    }

    internal void EquipRandom()
    {
        System.Random rnd = new System.Random();

        foreach (EquipableSlot slot in EquipableSlotIterator.allSlots)
        {
            List<Wearable> genderedWearables = GetWearablesFor(gender, slot);

            if (genderedWearables != null && genderedWearables.Count > 0)
            {
                int idx = rnd.Next(0, genderedWearables.Count); // creates a number between 0 and wearablesIds.Count-1
                LoadEquipableSlotsWithWearableId(mannequin, genderedWearables[idx].id);
            }
        }
    }

    internal List<Wearable> GetWearablesFor(MannequinGender gender, EquipableSlot slot)
    {
        List<Wearable> rawWearables = wearablesBySlot[slot].Values.ToList<Wearable>();
        List<Wearable> genderedWearables = new List<Wearable>();
        foreach (Wearable w in rawWearables)
        {
            if (w.sex.StartsWith("f"))
            {
                if (gender == MannequinGender.female)
                    genderedWearables.Add(w);
            }
            else
            {
                if (gender == MannequinGender.male)
                    genderedWearables.Add(w);
            }
        }
        return genderedWearables;
    }

    internal void LoadDefaultBody()
    {
        foreach (EquipableSlot slot in EquipableSlotIterator.allSlots)
            UnloadEquipableSlot(mannequin, slot);

        List<Wearable> wearables = GetWearablesFor(gender, EquipableSlot.body);
        if (wearables != null && wearables.Count > 0)
        {
            LoadEquipableSlotsWithWearableId(mannequin, wearables[0].id);
        }
    }

    internal void LoadPrevWearableForSlot(EquipableSlot slot)
    {
        List<Wearable> wearables = GetWearablesFor(gender, slot);
        if (wearables != null && wearables.Count > 0)
        {
            int currentIdx = FindCurrentIndex(slot, wearables);
            int prevIdx = (currentIdx - 1 + wearables.Count) % wearables.Count;
            LoadEquipableSlotsWithWearableId(mannequin, wearables[prevIdx].id);
        }
    }

    internal void LoadNextWearableForSlot(EquipableSlot slot)
    {
        List<Wearable> wearables = GetWearablesFor(gender, slot);
        if (wearables != null && wearables.Count > 0)
        {
            int currentIdx = FindCurrentIndex(slot, wearables);
            int nextIdx = (currentIdx + 1) % wearables.Count;
            LoadEquipableSlotsWithWearableId(mannequin, wearables[nextIdx].id);
        }
    }

    // Return -1 if not found.
    int FindCurrentIndex(EquipableSlot slot, List<Wearable> wearables)
    {
        int foundIdx = -1;
        WearableInfo wInfo = mannequin.slots[slot] != null ? mannequin.slots[slot].GetComponent<WearableInfo>() : null;

        if (wInfo != null)
        {
            // Search for equipped wearable id
            for (int i = 0; i < wearables.Count; i++)
            {
                if (wearables[i].id == wInfo.wearable.id)
                {
                    foundIdx = i;
                    break;
                }
            }
        }

        return foundIdx;
    }

    public static string GetPathForOutfitPrefab(string prefabName, string gender)
    {
        string sex = (gender == "f") ? "Female" : "Male";
        string path = /*"RunwayModels/" + sex + "/" + */ prefabName;

        return path;
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(EquipableWearablesManager))]
public class EquipableWearablesManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();


        EquipableWearablesManager myScript = (EquipableWearablesManager)target;
        if (GUILayout.Button("Load Default Body"))
        {
            myScript.LoadDefaultBody();
        }
        if (GUILayout.Button("Equip Random") && CheckIsPlaying())
        {
            myScript.EquipRandom();
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label("Body:   ");
        if (GUILayout.Button("Prev") && CheckIsPlaying())
        {
            myScript.LoadPrevWearableForSlot(EquipableSlot.body);
        }
        if (GUILayout.Button("Next") && CheckIsPlaying())
        {
            myScript.LoadNextWearableForSlot(EquipableSlot.body);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Top:   ");
        if (GUILayout.Button("Prev") && CheckIsPlaying())
        {
            myScript.LoadPrevWearableForSlot(EquipableSlot.top);
        }
        if (GUILayout.Button("Next") && CheckIsPlaying())
        {
            myScript.LoadNextWearableForSlot(EquipableSlot.top);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Bottom:");
        if (GUILayout.Button("Prev") && CheckIsPlaying())
        {
            myScript.LoadPrevWearableForSlot(EquipableSlot.bottom);
        }
        if (GUILayout.Button("Next") && CheckIsPlaying())
        {
            myScript.LoadNextWearableForSlot(EquipableSlot.bottom);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Shoes: ");
        if (GUILayout.Button("Prev") && CheckIsPlaying())
        {
            myScript.LoadPrevWearableForSlot(EquipableSlot.shoes);
        }
        if (GUILayout.Button("Next") && CheckIsPlaying())
        {
            myScript.LoadNextWearableForSlot(EquipableSlot.shoes);
        }
        GUILayout.EndHorizontal();
    }

    bool CheckIsPlaying()
    {
        if (Application.isPlaying == false)
            Debug.LogError("Can't execute if Application.isPlaying == false");
        return Application.isPlaying;
    }
}
#endif
