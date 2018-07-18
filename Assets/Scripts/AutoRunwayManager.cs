using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AutoRunwayManager : MonoBehaviour
{
    public GameObject runway;
    public GameObject cameraGroup;
    public ColliderEvents RunwayMidExit;
    public ColliderEvents RunwayFinish;
    public ColliderEvents RunwayEnd;
    public List<GameObject> levels;
    public GameObject curLevel;

    private bool loop = false;
    private int loopAmount = 1;
    private bool showFinale = false;
    private float pauseToFinale = 3;
    private float pauseToNextCollection = 3;

    private int curCollectionIndex = 0;
    private int curOutfit = 0;
    private int curLoop = 0;
    private int totalCollections = 0;
    private int totalOutfits = 0;

    private bool isInFinale = false;
    private bool isCollectionEnding = false;

    private Collection curCollection;

    private List<GameObject> models = new List<GameObject>();
    private Vector3 startingPoint = new Vector3(6, 0, -2.4f);

    void Start()
    {
        cameraGroup.SetActive(false);
    }
    public void HideAllLevels()
    {
        foreach (GameObject level in levels)
        {
            level.SetActive(false);
        }
    }

    public void ReadyAutoRunway(GameObject level = null)
    {
        HideAllLevels();

        if (level == null) {
            curLevel = levels[0];
        } else
        {
            curLevel = level;
        }

        curLevel.SetActive(true);

        if (curLevel == null)
        {
            Debug.Log("ERROR - LEVEL NOT FOUND");
        }
        cameraGroup.SetActive(true);
        Setup();
    }

    public void PlayAutoRunway()
    {
        if (curLevel == null)
        {
            curLevel = levels[0];
        }
        
        BeginRunwayShow();
    }

    public void StopAutoRunway()
    {
        cameraGroup.SetActive(false);
        ClearModels();
        UIManager.Instance.HideAll();
    }

    private void Setup()
    {
        curCollectionIndex = 0;
        
        MRData.Instance.LoadEverything();
        totalCollections = MRData.Instance.collections.collections.Count;

        PrepareCollectionRunwayModelPrefabs();
        SetupEvents();
    }

    private void SetupEvents()
    {
        if (RunwayMidExit == null)
            RunwayMidExit = GameObject.Find("RunwayMidExit")?.GetComponent<ColliderEvents>();
        if (RunwayFinish == null)
            RunwayFinish = GameObject.Find("RunwayFinish")?.GetComponent<ColliderEvents>();
        if (RunwayEnd == null)
            RunwayEnd = GameObject.Find("RunwayEnd")?.GetComponent<ColliderEvents>();

        RunwayMidExit.OnTriggerEnterEvt += OnRunwayMidExit;
        RunwayFinish.OnTriggerEnterEvt += OnRunwayFinish;
        RunwayEnd.OnTriggerEnterEvt += OnRunwayEndEnter;
        RunwayEnd.OnTriggerExitEvt += OnRunwayEndExit;

    }

    private void PrepareCollectionRunwayModelPrefabs()
    {
        if(models.Count > 0)
        {
            ClearModels();
        }

        curOutfit = 0;
        curLoop = 0;
        totalOutfits = 0;
        isInFinale = false;
        isCollectionEnding = false;

        curCollection = MRData.Instance.collections.collections[curCollectionIndex];

        totalOutfits = curCollection.outfits.Count;
        

        foreach (Outfit outfit in curCollection.outfits)
        {
            string sex = (outfit.sex == "f") ? "Female" : "Male";
            string path = "RunwayModels/"+sex+"/"+outfit.prefab;
            GameObject go = RunwayModelsPrefabManager.InstantiateGameObject(path, runway.transform);
            go.SetActive(false);
            go.transform.localPosition = startingPoint;
            models.Add(go);
        }
    }

    private void PrepareNextCollection()
    {
        //Debug.Log("READY FOR NEXT COLLECTION");
        UIManager.Instance.HideCollection();

        curCollectionIndex++;

        if (curCollectionIndex == totalCollections)
        {
            curCollectionIndex = 0;
        }

        PrepareCollectionRunwayModelPrefabs();

        if(pauseToNextCollection == 0)
        {
            BeginRunwayShow();
        } else
        {
            StartCoroutine(WaitToNextCollection(pauseToNextCollection));
        }
    }

    private void ClearModels()
    {
        foreach (GameObject go in models)
        {
            Destroy(go);
        }

        models = new List<GameObject>();
    }

    private void BeginRunwayShow()
    {
        //Collection collection = MRData.Instance.collections.collections[curCollectionIndex];
        AutoRunwayEvents.CollectionStart(curCollection);
        UIManager.Instance.ShowCollection(curCollection);
        UIManager.Instance.HideUpNext();
        curOutfit = 0;
        RunModel(curOutfit);
    }

    private void CheckCompletion(GameObject model)
    {
        if(isCollectionEnding == false) { return; }
        //Collection collection = MRData.Instance.collections.collections[curCollectionIndex];
        if (showFinale == true && isInFinale == false)
        {
            AutoRunwayEvents.FinaleStart(curCollection);
            BeginFinale();
        }
        else
        {
            if (isInFinale == true)
            {
                ClearModel(model);

                if (models.Count == 0)
                {
                    AutoRunwayEvents.CollectionEnd(curCollection);

                    if (showFinale == true)
                    {
                        AutoRunwayEvents.FinaleEnd(curCollection);
                    }

                    PrepareNextCollection();
                }
            } 
            else
            {
                AutoRunwayEvents.CollectionEnd(curCollection);
                PrepareNextCollection();
            }
        }
    }

    private void ClearModel(GameObject model)
    {
        Destroy(model);
        for (int x=0; x<models.Count; x++)
        {
            if (models[x] == model)
            {
                models.RemoveAt(x);
                return;
            }
        }
    }

    private void QueueUp()
    {
        if(isInFinale == true) { return; }

        curOutfit++;

        bool queueEnd = false;

        if (curOutfit == totalOutfits)
        {
            curLoop++;
            curOutfit = 0;

            if(curLoop == loopAmount)
            {
                queueEnd = true;
            }

            if(loop == false)
            {
                queueEnd = true;
            }
        }

        if(queueEnd == true)
        {
            isCollectionEnding = true;

            int nextCollectionIndex = curCollectionIndex+1;
            if (nextCollectionIndex == totalCollections) { nextCollectionIndex = 0; }
            Collection nextCollection = MRData.Instance.collections.collections[nextCollectionIndex];

            UIManager.Instance.ShowUpNext(nextCollection);
            //AutoRunwayEvents.LoopEnd(curCollection, curLoop);
        } else
        {
            RunModel(curOutfit);
        }
    }

    private void RunModel(int index)
    {
        Collection collection = MRData.Instance.collections.collections[curCollectionIndex];
        GameObject model = models[index];
        Animator animator = model.GetComponent<Animator>();

        Outfit outfit = collection.outfits[index];

        string animation = ModelAnimationManager.GetPoseAnimation(outfit.sex);

        animator.runtimeAnimatorController = (RuntimeAnimatorController)RuntimeAnimatorController.Instantiate(Resources.Load(animation), model.transform);
  
        model.SetActive(true);
    }

    private void BeginFinale()
    {
        isInFinale = true;

        if (pauseToFinale > 0)
        {
            StartCoroutine(WaitToShowFinale(pauseToFinale));
        }
        else {
            StartCoroutine(TrainLoop());
        }
    }

    IEnumerator TrainLoop()
    {
        int count = 0;
        while (count < totalOutfits)
        {
            RunModel(count);
            count++;
            
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator WaitToShowFinale(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        StartCoroutine(TrainLoop());
    }

    IEnumerator WaitToNextCollection(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        BeginRunwayShow();
    }

    private void OnRunwayFinish(Collider other)
    {
        other.gameObject.SetActive(false);
        Animator animator = other.gameObject.GetComponent<Animator>();
        animator.runtimeAnimatorController = null;

        CheckCompletion(other.gameObject);
    }

    private void OnRunwayMidExit(Collider other)
    {
        QueueUp();
    }

    private void OnRunwayEndEnter(Collider other)
    {
        if(isInFinale == true) { return; }

        Outfit outfit = curCollection.outfits[curOutfit];
        UIManager.Instance.ShowOutfit(outfit);
    }

    private void OnRunwayEndExit(Collider other)
    {
        if (isInFinale == true) { return; }
        UIManager.Instance.HideOutfit();
    }

    private void OnDestroy()
    {
        RunwayMidExit.OnTriggerEnterEvt -= OnRunwayMidExit;
        RunwayFinish.OnTriggerEnterEvt -= OnRunwayFinish;
        RunwayEnd.OnTriggerEnterEvt -= OnRunwayEndEnter;
        RunwayEnd.OnTriggerExitEvt -= OnRunwayEndExit;
    }
}