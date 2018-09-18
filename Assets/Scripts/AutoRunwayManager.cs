using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class AutoRunwayManager : MonoBehaviour, IRunwayMode, KinectGestures.GestureListenerInterface
{
    public GameObject autoRunwayContainer;

    [SerializeField]
    private RunwayEventManager runwayEventManager;

    [SerializeField]
    private VideoWall videoWall;

    private ShowcaseManager showcaseManager;
    private GameObject runwayModels;
    private AudioSource audioSource;

    private List<string> resource = new List<string>();
    private List<GameObject> models = new List<GameObject>();
    private bool isCollectionEnding = false;
    private Vector3 startingPoint = new Vector3(5.7f, 0.1f, -5.0f);

    private bool isModeActive = false;

    void Awake()
    {
        autoRunwayContainer.SetActive(false);

        runwayModels = new GameObject("RunwayModels");
        runwayModels.transform.parent = autoRunwayContainer.transform;

        audioSource = GetComponent<AudioSource>();
    }

    public Mode GetMode()
    {
        return Mode.AUTO;
    }

    public void SetUp()
    {
        Debug.Log("SetUp Auto Runway");

        Application.targetFrameRate = 120;

        AddRunwayEventListeners();

        KinectManager.Instance.ClearKinectUsers();
        UIManager.Instance.HideAll();
        autoRunwayContainer.SetActive(true);

        showcaseManager = new ShowcaseManager(MRData.Instance.collections.collections);
    }

    public void Begin() {
        Debug.Log("Begin Auto Runway");
        StartCoroutine(PrepareModelsAndBeginShow());
    }

    public void End()
    {
        StopAllCoroutines();

        RemoveRunwayEventListeners();

        Resources.UnloadUnusedAssets();
        DestroyAllCharacters();
        UIManager.Instance.HideAll();
        showcaseManager = null;

        KinectManager.Instance.ClearKinectUsers();

        autoRunwayContainer.SetActive(false);

        Debug.Log("Auto Runway Has Ended");
    }

    //----------------------------------------------------------------------
    // Runway Event Functions
    //----------------------------------------------------------------------
    
    private void PrepareCollectionRunwayModelPrefabs()
    {
        List<Outfit> outfits = showcaseManager.PrepareShow();
        // UIManager.Instance.ShowCollection(showcaseManager.currentCollection);
        UIManager.Instance.ShowCollectionTitle(showcaseManager.currentCollection.name + " Collection",true);
        UIManager.Instance.HideForNextCollection();

        DestroyAllCharacters();
        

        resource = new List<string>();

        foreach (Outfit outfit in outfits)
        {
            string sex = (outfit.sex == "f") ? "Female" : "Male";
            string path = "RunwayModels/"+sex+"/"+outfit.prefab;

            resource.Add(path);
        }
    }
    
    IEnumerator PrepareModelsAndBeginShow(float waitToStart = 3.0f)
    {
        PrepareCollectionRunwayModelPrefabs();
        yield return StartCoroutine(LoadAndPrepareModels());
        videoWall.ChangeAndFadeIn(showcaseManager.currentCollection.splash);
        yield return new WaitForSeconds(waitToStart);
        UIManager.Instance.HideCollectionTitle(true);
        PresentRunwayModel();
    }

    IEnumerator LoadAndPrepareModels()
    {
        int total = 0;
        bool notReady = true;

        models = new List<GameObject>();

        while (notReady)
        {
            ResourceRequest request = Resources.LoadAsync(resource[total]);
            yield return request;

            GameObject prefab = (GameObject)request.asset;

            GameObject go = GameObject.Instantiate(prefab);
            go.SetActive(true);
            go.transform.SetParent(runwayModels.transform);
            go.transform.localScale = Vector3.one;
            go.transform.localEulerAngles = Vector3.zero;
            go.transform.localPosition = Vector3.zero;
            
            Animator animator = go.GetComponent<Animator>();
            animator.enabled = false;

            EnableObiCloth(go, false);
            EnableRenderers(go, false);
           
            go.transform.localPosition = startingPoint;

            models.Add(go);

            if (total == (resource.Count - 1))
                notReady = false;

            total++;
        }
    }

    private void EnableRenderers(GameObject character, bool enable)
    {
        Renderer[] renderers = character.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
            renderer.enabled = enable;
    }

    private void EnableObiCloth(GameObject character, bool enable)
    {
        ObiSolver[] oss = character.GetComponentsInChildren<ObiSolver>();

        foreach (ObiSolver os in oss)
        {
            os.enabled = enable;
            if(enable == true)
                os.UpdateOrder = ObiSolver.SimulationOrder.LateUpdate;
        }
            
        ObiCloth[] ocs = character.GetComponentsInChildren<ObiCloth>();

        foreach (ObiCloth oc in ocs)
        {
            oc.enabled = enable;
            if(enable == true)
                oc.ResetActor();
        }
    }

    IEnumerator NextCollection()
    {
        Resources.UnloadUnusedAssets();

        //UIManager.Instance.HideCollection();

        videoWall.FadeOut();

        showcaseManager.ReadyNextShow();

        yield return new WaitForSeconds(3);

        StartCoroutine(PrepareModelsAndBeginShow());
    }

    private void DestroyAllCharacters()
    {
        foreach (Transform child in runwayModels.transform)
            Destroy(child.gameObject);

        models = new List<GameObject>();
    }

    private void QueueUp()
    {
        bool isEnding = showcaseManager.NextOutfit();
        if (isEnding)
        {
            isCollectionEnding = true;
            UIManager.Instance.ShowUpNext(showcaseManager.nextCollection);
        } else
        {
            PresentRunwayModel();
        }
    }

    private void PresentRunwayModel()
    {
        Outfit outfit = showcaseManager.GetCurrentOutfit();
        GameObject model = models[showcaseManager.curOutfit];

        Animator animator = model.GetComponent<Animator>();
        
        string animation = ModelAnimationManager.GetPoseAnimation(outfit.sex);
        EnableRenderers(model, true);
        RuntimeAnimatorController ani = (RuntimeAnimatorController)Resources.Load(animation, typeof(RuntimeAnimatorController));
        animator.runtimeAnimatorController = (RuntimeAnimatorController)RuntimeAnimatorController.Instantiate(ani, model.transform);
        animator.enabled = true;
        EnableObiCloth(model, true);
        model.SetActive(true);

        CutoutTextureSwapper cutout = model.GetComponentInChildren<CutoutTextureSwapper>();
        if (cutout != null)
            cutout.Generate();
    }
  
    //----------------------------------------------------------------------
    // Runway Event Functions
    //----------------------------------------------------------------------

    private void OnRunwayEnter(Collider model)
    {
        List<string> crowd = new List<string>(new string[] { SfxManager.APPLAUSE_1, SfxManager.APPLAUSE_2 });
        int index = Random.Range(0, crowd.Count);

        AudioClip clip = SfxManager.LoadClip(crowd[index]);

        audioSource.PlayOneShot(clip);
    }

    private void OnRunwayFinish(Collider other)
    {
        Destroy(other.gameObject.transform.parent.gameObject);
        if (isCollectionEnding == false) { return; }
        StartCoroutine(NextCollection());
    }

    private void OnRunwayMidExit(Collider other) {
        QueueUp();
    }

    private void OnRunwayEndEnter(Collider other)
    {
        UIManager.Instance.ShowOutfit(showcaseManager.GetCurrentOutfit());
    }

    private void OnRunwayEndExit(Collider other) { UIManager.Instance.HideOutfit(); }

    private void AddRunwayEventListeners()
    {
        RemoveRunwayEventListeners();
        isModeActive = true;

        runwayEventManager.RunwayEnterEvents.OnTriggerEnterEvt += OnRunwayEnter;
        runwayEventManager.RunwayMidExit.OnTriggerEnterEvt += OnRunwayMidExit;
        runwayEventManager.RunwayFinish.OnTriggerEnterEvt += OnRunwayFinish;
        runwayEventManager.RunwayEnd.OnTriggerEnterEvt += OnRunwayEndEnter;
        runwayEventManager.RunwayEnd.OnTriggerExitEvt += OnRunwayEndExit;
    }

    private void RemoveRunwayEventListeners()
    {
        isModeActive = false;

        runwayEventManager.RunwayMidExit.OnTriggerEnterEvt -= OnRunwayMidExit;
        runwayEventManager.RunwayFinish.OnTriggerEnterEvt -= OnRunwayFinish;
        runwayEventManager.RunwayEnd.OnTriggerEnterEvt -= OnRunwayEndEnter;
        runwayEventManager.RunwayEnd.OnTriggerExitEvt -= OnRunwayEndExit;
        runwayEventManager.RunwayEnterEvents.OnTriggerEnterEvt -= OnRunwayEnter;
    }

    private void ValidateModel()
    {
        //make sure if the model is correctly made OR ELSE!!
    }

    public void UserDetected(long userId, int userIndex)
    {
        if (isModeActive == false)
            return;

        UIManager.Instance.ShowStartMenu(true);
        KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.Wave);
    }

    public void UserLost(long userId, int userIndex)
    {
        if (isModeActive == false)
            return;

        UIManager.Instance.HideStartMenu(true);
        KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.Wave);
    }

    public void GestureInProgress(long userId, int userIndex, KinectGestures.Gestures gesture, float progress, KinectInterop.JointType joint, Vector3 screenPos)
    {
        //throw new System.NotImplementedException();
    }

    public bool GestureCompleted(long userId, int userIndex, KinectGestures.Gestures gesture, KinectInterop.JointType joint, Vector3 screenPos)
    {
        if (gesture == KinectGestures.Gestures.Wave)
        {
            if (isModeActive == false)
                return true;

            KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.Wave);
            UIManager.Instance.HideStartMenu(false);
            AppManager.Instance.TransitionToLive();

            return false;
        }
        return true;
    }

    public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture, KinectInterop.JointType joint)
    {
        return true;
    }
}