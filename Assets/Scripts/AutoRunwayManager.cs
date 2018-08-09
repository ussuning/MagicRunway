using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Obi;

public class AutoRunwayManager : MonoBehaviour
{
    public GameObject runway;
    public GameObject cameraGroup;
    public GameObject outfits;
    public GameObject autoRunwayContainer;
    public GameObject videoWall;
    public ColliderEvents RunwayMidExit;
    public ColliderEvents RunwayFinish;
    public ColliderEvents RunwayEnd;
    public List<GameObject> levels;
    public GameObject curLevel;

    private RunwayCameraController runwayCamera;
    private GameObject agents;


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
    private Vector3 startingPoint = new Vector3(5.7f, 0.04f, -5.0f);
    //private Vector3 startingPoint = new Vector3(6, 0, -2.4f);
    private byte videoFadeState;
    private float videoFadeStartTime;
    private float videoFadeDeltaTime;
    private float videoFadeDuration = 3.0f;
    private float videoColor = 0;
    private Material videoMaterial;
    private Texture2D videoSplash;

    void Awake()
    {
        if (autoRunwayContainer == null)
        {
            autoRunwayContainer = GameObject.Find("Auto Runway");
            
        }

        agents = new GameObject("Agents");
        agents.transform.parent = autoRunwayContainer.transform;

        runwayCamera = cameraGroup.GetComponent<RunwayCameraController>();

        SetCameraActive(false);

        SetupEvents();
    }

    void Start()
    {
        Material[] mats = videoWall.GetComponent<Renderer>().materials;
        videoMaterial = mats[0];
        VideoWallStartFadeOut(false);
    }

    private void Update()
    {
        UpdateVideoWall();
    }

    private void UpdateVideoWall()
    {
        if (videoFadeState == 1)
        {
            videoMaterial.SetColor("_EmissionColor", Color.Lerp(Color.black, Color.white, videoColor));
            videoMaterial.color = Color.Lerp(Color.black, Color.white, videoColor);
            if (videoColor < 1)
            {
                videoColor += Time.deltaTime / videoFadeDuration;
            } else
            {
                videoMaterial.SetColor("_EmissionColor", Color.white);
                videoMaterial.color = Color.white;

                videoFadeState = 0;
            }
        }

        if (videoFadeState == 2)
        {
            videoMaterial.SetColor("_EmissionColor", Color.Lerp(Color.white, Color.black, videoColor));
            videoMaterial.color = Color.Lerp(Color.white, Color.black, videoColor);
            if (videoColor < 1)
            {
                videoColor += Time.deltaTime / videoFadeDuration;
            }
            else
            {
                videoMaterial.SetColor("_EmissionColor", Color.black);
                videoMaterial.color = Color.black;

                videoFadeState = 0;
            }
        }
    }

    public void FadeOutVid()
    {
        VideoWallStartFadeOut();
    }

    private void VideoWallStartFadeIn()
    {
        videoMaterial.SetColor("_EmissionColor", Color.black);
        videoMaterial.color = Color.black;
        videoFadeStartTime = Time.realtimeSinceStartup;
        videoColor = 0;
        videoFadeState = 1;
    }

    private void VideoWallStartFadeOut(bool animate = true)
    {
        if (animate == false)
        {
            videoMaterial.SetColor("_EmissionColor", Color.black);
            videoMaterial.color = Color.black;
            return;
        }

        videoMaterial.SetColor("_EmissionColor", Color.white);
        videoMaterial.color = Color.white;
        videoFadeStartTime = Time.realtimeSinceStartup;
        videoColor = 0;
        videoFadeState = 2;
    }

    private void VideoWallChangePic()
    {

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
        Application.targetFrameRate = 120;

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

        SetCameraActive(true);
   
        Setup();
    }

    public void PlayAutoRunway()
    {
        if (curLevel == null)
        {
            curLevel = levels[0];
        }

        AttachOutfits();
        BeginRunwayShow();
    }

    public void StopAutoRunway()
    {
        SetCameraActive(false);

        ClearModels();
        DetachOutfits();
        UIManager.Instance.HideAll();
    }

    public void AttachOutfits()
    {
        outfits.SetActive(true);
    }

    public void DetachOutfits()
    {
        outfits.SetActive(false);
    }

    private void Setup()
    {
        curCollectionIndex = 0;
        
        totalCollections = MRData.Instance.collections.collections.Count;

        PrepareCollectionRunwayModelPrefabs();
        
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
            GameObject go = RunwayModelsPrefabManager.InstantiateGameObject(path, agents.transform);
            
            go.SetActive(true);

            Animator animator = go.GetComponent<Animator>();
            animator.enabled = false;

            ObiSolver[] oss = go.GetComponentsInChildren<ObiSolver>();

            foreach (ObiSolver os in oss)
            {
                os.enabled = false;
            }

            ObiCloth[] ocs = go.GetComponentsInChildren<ObiCloth>();

            foreach (ObiCloth oc in ocs)
            {
                oc.enabled = true;
                oc.ResetActor();
            }

            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = false;
            }

            //ObiSolver os = go.transform.Find("ObiSolver").GetComponent<ObiSolver>();

            go.transform.localPosition = startingPoint;
            models.Add(go);
        }
    }

    private void PrepareNextCollection()
    {
        //Debug.Log("READY FOR NEXT COLLECTION");
        UIManager.Instance.HideCollection();
        VideoWallStartFadeOut();

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

    private void ClearModel(GameObject model)
    {
        //Debug.Log("DELETE MODEL " + model.name);
        GameObject parent = model.transform.parent.gameObject;
        Destroy(parent);
        for (int x = 0; x < models.Count; x++)
        {
            if (models[x] == parent)
            {
                models.RemoveAt(x);
                return;
            }
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

    private void HideAllModels()
    {
        foreach (GameObject go in models)
        {
            go.SetActive(false);
        }
    }

    private void BeginRunwayShow()
    {
        //Collection collection = MRData.Instance.collections.collections[curCollectionIndex];
        AutoRunwayEvents.CollectionStart(curCollection);
        UIManager.Instance.ShowCollection(curCollection);
        UIManager.Instance.HideUpNext();
        UIManager.Instance.HideStartMenu(false);
        UIManager.Instance.HideGestureGender(false);
        curOutfit = 0;
        //RunModel(curOutfit);

        videoSplash = Resources.Load<Texture2D>(curCollection.splash);
        videoMaterial.SetTexture("_MainTex", videoSplash);
        videoMaterial.SetTexture("_EmissionMap", videoSplash);

        VideoWallStartFadeIn();

        StartCoroutine(BeginShow());
    }

    IEnumerator BeginShow()
    {
        yield return new WaitForSeconds(5);
        //HideAllModels();
        RunModel(curOutfit);
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

        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = true;
        }


        RuntimeAnimatorController ani = (RuntimeAnimatorController)Resources.Load(animation, typeof(RuntimeAnimatorController));
       
        animator.runtimeAnimatorController = (RuntimeAnimatorController)RuntimeAnimatorController.Instantiate(ani, model.transform);
        animator.enabled = true;
        
        ObiSolver[] oss = model.GetComponentsInChildren<ObiSolver>();

        foreach (ObiSolver os in oss)
        {
            
            os.enabled = true; 
        }

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

    private void SetCameraActive(bool active)
    {
        runwayCamera.enabled = active;
        cameraGroup.SetActive(active);
    }

    private void OnRunwayFinish(Collider other)
    {
        //animation on the same layer of collider
        //Animator animator = other.gameObject.GetComponent<Animator>();

        //animation is on the parent layer of collider
       
        GameObject parentModel = other.gameObject.transform.parent.gameObject;
        Animator animator = parentModel.GetComponent<Animator>();
        ObiSolver[] oss = parentModel.GetComponentsInChildren<ObiSolver>();

        foreach (ObiSolver os in oss)
        {
            os.enabled = false;
        }

        ObiCloth[] ocs = parentModel.GetComponentsInChildren<ObiCloth>();

        foreach (ObiCloth oc in ocs)
        {
            //oc.ResetActor();
            oc.enabled = false;
        }

        if (animator == null)
            animator = other.gameObject.GetComponentInParent<Animator>();

        animator.enabled = false;
        animator.runtimeAnimatorController = null;

        parentModel.SetActive(false);
       
        if (isCollectionEnding == false) { return; }

        if (showFinale == true && isInFinale == false)
        {
            //Debug.Log("WALK ENDED AND BEGINNING FINALE");
            AutoRunwayEvents.FinaleStart(curCollection);
            BeginFinale();
        }
        else
        {
            if (isInFinale == true)
            {
                //Debug.Log("MODEL FINISHED WALKING IN FINALE");
                ClearModel(other.gameObject);

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
                //Debug.Log("ALL MODELS FINISHED WALKING");
                AutoRunwayEvents.CollectionEnd(curCollection);
             
                PrepareNextCollection();
            }
        }
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