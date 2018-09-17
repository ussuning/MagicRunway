using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiveRunwayManager : MonoBehaviour, IRunwayMode {
    public GameObject stickman;
    public GameObject poseAcademy;
    public GameObject liveRunwayContainer;
    public GameObject outfits;
   // public GameObject canvas;
    public GameObject fittingRoom;
    //public GameObject userModel;

    //private Collection curCollection;
    //private Collection nextCollection;
    //private List<GameObject> models = new List<GameObject>();

    //private float collectionShowTime = 20.0f;
    //private float collectionWarningTime = 10.0f;
    //private int curCollectionIndex = 0;
    //private int totalCollections = 0;
    //private int totalOutfits = 0;

    private float gestureGenderShowLength = 3.0f;

    //private bool isCollectionEnding = false;
    void Awake()
    {
        liveRunwayContainer.SetActive(false);
    }

    public Mode GetMode()
    {
        return Mode.LIVE;
    }

    public void Begin()
    {
        throw new System.NotImplementedException();
    }

    public void SetUp()
    {
        liveRunwayContainer.SetActive(true);
        fittingRoom.SetActive(true);

        UIManager.Instance.ShowGestureGender(10.0f);
        UIManager.Instance.ShowStickManDelay(11.0f);
    }

    public void End()
    {
        UIManager.Instance.HideAll();
        
        fittingRoom.SetActive(false);
        liveRunwayContainer.SetActive(false);
    }

    public void ShowGestureGender()
    {
        UIManager.Instance.ShowGestureGender(gestureGenderShowLength);
    }

    /*
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
            string path = GetPathForOutfitPrefab(outfit.prefab, outfit.sex);
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
    */
    /*
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
    */

    public static string GetPathForOutfitPrefab(string prefabName, string gender)
    {
        string sex = (gender == "f") ? "Female" : "Male";
        string path = "RunwayModels/" + sex + "/" + prefabName;

        return path;
    }
}
