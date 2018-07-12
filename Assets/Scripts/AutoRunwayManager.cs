using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRunwayManager : MonoBehaviour
{
    public GameObject runway;
    public ColliderEvents RunwayMidExit;
    public ColliderEvents RunwayFinish;

    [HideInInspector]
    public bool loop = false;

    [HideInInspector]
    public int loopAmount = 1;

    [HideInInspector]
    public bool showFinale = true;

    [HideInInspector]
    public float pauseToFinale = 3;

    [HideInInspector]
    public float pauseToNextCollection = 3;

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
        Setup();
        BeginRunwayShow();
        //GameObject go = RunwayModelsPrefabManager.InstantiateGameObject("RunwayModels/Female/test_outfit_01", runway.transform);
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

        RunwayMidExit.OnTriggerEnterEvt += OnRunwayMidExit;
        RunwayFinish.OnTriggerEnterEvt += OnRunwayFinish;
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
        } else
        {
            //Debug.Log("Queue Up Next Collection");
            ClearModel(model);

            if(models.Count == 0) {
                
                AutoRunwayEvents.CollectionEnd(curCollection);

                if(showFinale == true)
                {
                    AutoRunwayEvents.FinaleEnd(curCollection);
                }

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

    private void OnDestroy()
    {
        RunwayMidExit.OnTriggerEnterEvt -= OnRunwayMidExit;
        RunwayFinish.OnTriggerEnterEvt -= OnRunwayFinish;
    }
}