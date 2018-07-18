using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiveRunwayManager : MonoBehaviour {
    public GameObject cameraGroup;
    public GameObject outfits;

    private Collection curCollection;
    private Collection nextCollection;
    private List<GameObject> models = new List<GameObject>();

    private float collectionShowTime = 20.0f;
    private float collectionWarningTime = 10.0f;
    private int curCollectionIndex = 0;
    private int totalCollections = 0;
    private int totalOutfits = 0;

    private float gestureGenderShowLength = 3.0f;

    private bool isCollectionEnding = false;

    void Awake()
    {
        UIEvents.OnUpNextCompleteCallback += UIEvents_OnUpNextComplete;
        cameraGroup.SetActive(false);
    }

    //Setup before starting live mode -- happens before fading in
    public void ReadyLiveRunway()
    {
        cameraGroup.SetActive(true);
        Setup();
    }

    //play live mode after fading in
    public void PlayLiveRunway()
    {
       // AutoRunwayEvents.CollectionStart(curCollection);
        UIManager.Instance.ShowCollection(curCollection);
        UIManager.Instance.ShowUpNext(curCollection);
        UIManager.Instance.RunUpNextTimer(nextCollection.name, collectionShowTime, collectionWarningTime);
    }

    public void StopLiveRunway()
    {
        cameraGroup.SetActive(false);
        UIManager.Instance.HideAll();
    }

    public void ShowGestureGender()
    {
        UIManager.Instance.ShowGestureGender(gestureGenderShowLength);
    }

    private void Setup()
    {
        curCollectionIndex = 0;

        totalCollections = MRData.Instance.collections.collections.Count;

        PrepareCollectionLiveModelPrefabs();
    }

    private void PrepareCollectionLiveModelPrefabs()
    {
        if (models.Count > 0)
        {
            ClearModels();
        }

        totalOutfits = 0;
        isCollectionEnding = false;

        curCollection = MRData.Instance.collections.collections[curCollectionIndex];

        totalOutfits = curCollection.outfits.Count;

        foreach (Outfit outfit in curCollection.outfits)
        {
            string sex = (outfit.sex == "f") ? "Female" : "Male";
            string path = "RunwayModels/" + sex + "/" + outfit.prefab;
            GameObject go = RunwayModelsPrefabManager.InstantiateGameObject(path, outfits.transform);
            //go.SetActive(false);
            models.Add(go);
        }

        int nextCollectionIndex = curCollectionIndex + 1;

        if (nextCollectionIndex == totalCollections)
        {
            nextCollectionIndex = 0;
        }

        nextCollection = MRData.Instance.collections.collections[nextCollectionIndex];
    }

    private void PrepareNextCollection()
    {
        UIManager.Instance.HideCollection();

        curCollectionIndex++;

        if (curCollectionIndex == totalCollections)
        {
            curCollectionIndex = 0;
        }

        PrepareCollectionLiveModelPrefabs();
        PlayLiveRunway();
    }

    private void ClearModels()
    {
        foreach (GameObject go in models)
        {
            Destroy(go);
        }

        models = new List<GameObject>();
    }

    void UIEvents_OnUpNextComplete()
    {
        PrepareNextCollection();
    }
}
