﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class AutoRunwayManager : MonoBehaviour, IRunwayMode, KinectGestures.GestureListenerInterface
{
    public GameObject autoRunwayContainer;

    [SerializeField]
    private RunwayEventManager runwayEventManager;

    [SerializeField]
    private FaceCapture faceCapture;

    [SerializeField]
    private VideoWall videoWall;

    [SerializeField]
    private ScrollingVideoWall scrollingVideoWall;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private ParticleSystem confetti;

    private ShowcaseManager showcaseManager;
    private GameObject runwayModels;

    private List<string> resource = new List<string>();
    private List<GameObject> models = new List<GameObject>();
    private bool isCollectionEnding = false;
    private Vector3 startingPoint = new Vector3(5.7f, 0.1f, -5.0f);

    private bool isModeActive = false;

    //private float waveInactivityTime = 10.0f;

    void Awake()
    {
        autoRunwayContainer.SetActive(false);

        runwayModels = new GameObject("RunwayModels");
        runwayModels.transform.parent = autoRunwayContainer.transform;
        Application.backgroundLoadingPriority = ThreadPriority.Low;
    }

    public Mode GetMode()
    {
        return Mode.AUTO;
    }

    public void SetUp(int level = 0)
    {
        Debug.Log("SetUp Auto Runway");

        Application.targetFrameRate = 120;
        //Application.backgroundLoadingPriority = ThreadPriority.Low;
        //Shader.WarmupAllShaders();
        KinectManager.Instance.maxTrackedUsers = 6;

        faceCapture.enabled = false;

        AddRunwayEventListeners();

        KinectManager.Instance.ClearKinectUsers();

        UIManager.Instance.HideAll();

        if (KinectManager.Instance.GetAllUserIds().Count > 0)
            UIManager.Instance.ShowStartMenu(false);

        scrollingVideoWall.Run();

        autoRunwayContainer.SetActive(true);

        showcaseManager = new ShowcaseManager();
        showcaseManager.SetCollection(MRData.Instance.collections.collections);
        if (level == 0)
            showcaseManager.ReadyFirstShow();
        else
            showcaseManager.ReadyShowAt(level);
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

        faceCapture.enabled = false;

        autoRunwayContainer.SetActive(false);

        Debug.Log("Auto Runway Has Ended");
    }

    //----------------------------------------------------------------------
    // Runway Event Functions
    //----------------------------------------------------------------------
    
    private void PrepareCollectionRunwayModelPrefabs()
    {
        isCollectionEnding = false;

        List<Outfit> outfits = showcaseManager.PrepareShow();
        UIManager.Instance.ShowCollectionTitle(showcaseManager.currentCollection.name + " Collection",true);
        UIManager.Instance.HideForNextCollection();

        DestroyAllCharacters();
        
        resource = new List<string>();

        foreach (Outfit outfit in outfits)
        {
            string sex = (outfit.sex == "f") ? "Female" : "Male";
            string path = /*"RunwayModels/"+sex+"/"+*/ outfit.prefab;

            resource.Add(path);
        }
    }
    
    IEnumerator PrepareModelsAndBeginShow(float waitToStart = 1.5f)
    {
        Debug.Log("PrepareModelsAndBeginShow");

        PrepareCollectionRunwayModelPrefabs();
        yield return new WaitForSeconds(1.0f); //wait till title show so it wont look choppy
        yield return StartCoroutine(LoadAndPrepareModels());
        scrollingVideoWall.Run();
        videoWall.ChangeAndFadeIn(showcaseManager.currentCollection.splash);
        yield return new WaitForSeconds(waitToStart);
        UIManager.Instance.HideCollectionTitle(true);
        yield return PresentRunwayModel();
    }

    IEnumerator LoadAndPrepareModels()
    {
        Debug.Log("LoadAndPrepareModels");
        int total = 0;
        bool notReady = true;

        models = new List<GameObject>();

        while (notReady)
        {
            //ResourceRequest request = Resources.LoadAsync(resource[total]);
            //yield return request;

            //GameObject prefab = (GameObject)request.asset;
            GameObject prefab = AssetBundleManager.Instance.GetModelAsset(resource[total]);

            GameObject go = GameObject.Instantiate(prefab);
            go.SetActive(true);
            go.transform.SetParent(runwayModels.transform);
            go.transform.localScale = Vector3.one;
            go.transform.localEulerAngles = Vector3.zero;
            go.transform.localPosition = Vector3.zero;

            go.transform.localPosition = startingPoint;

            Animator animator = go.GetComponent<Animator>();
            //animator.runtimeAnimatorController = null;
            animator.enabled = false;

            yield return EnableObiCloth(go, false);
            yield return EnableRenderers(go, false);

            ModelValidator.ValidateModel(go);

            models.Add(go);
            
            if (total == (resource.Count - 1))
                notReady = false;
                
            total++;
        }
    }

    IEnumerator EnableRenderers(GameObject character, bool enable)
    {
        Renderer[] renderers = character.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = enable;
            if (enable)
                yield return new WaitForSeconds(0.01f);
        }
    }

    IEnumerator EnableObiCloth(GameObject character, bool enable)
    {
        ObiSolver[] oss = character.GetComponentsInChildren<ObiSolver>();

        foreach (ObiSolver os in oss)
        {
            os.enabled = enable;
            //if(enable == true)
            //    os.UpdateOrder = ObiSolver.SimulationOrder.LateUpdate;
        }
            
        ObiCloth[] ocs = character.GetComponentsInChildren<ObiCloth>();

        foreach (ObiCloth oc in ocs)
        {
            oc.enabled = enable;
            if(enable == true)
                oc.ResetActor();
            if (enable)
                yield return new WaitForSeconds(0.01f);
        }
    }

    IEnumerator NextCollection()
    {
        Resources.UnloadUnusedAssets();

        videoWall.FadeOut();

        showcaseManager.ReadyNextShow();

        yield return new WaitForSeconds(1.5f);
        scrollingVideoWall.Freeze();
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
        isCollectionEnding = showcaseManager.NextOutfit();
        if (isCollectionEnding)
        {
            UIManager.Instance.ShowUpNext(showcaseManager.nextCollection);
        } else
        {
            StartCoroutine(PresentRunwayModel());
        }
    }

    IEnumerator PresentRunwayModel()
    {
        Outfit outfit = showcaseManager.GetCurrentOutfit();

        if (outfit == null)
        {
            Debug.LogError("Something went wrong. Restarting AutoRunway");
            AppManager.Instance.TransitionToAuto();
            yield break;
        }

        GameObject model = models[showcaseManager.curOutfit];

        Animator animator = model.GetComponent<Animator>();
        
        string animation = ModelAnimationManager.GetPoseAnimation(outfit.sex);
        yield return EnableRenderers(model, true);
        RuntimeAnimatorController ani = (RuntimeAnimatorController)Resources.Load(animation, typeof(RuntimeAnimatorController));
        //animator.runtimeAnimatorController = (RuntimeAnimatorController)RuntimeAnimatorController.Instantiate(ani, model.transform);
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.enabled = true;
        yield return EnableObiCloth(model, true);
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
        /*
        List<string> crowd = new List<string>(new string[] { SfxManager.CROWD_SHORT, SfxManager.CROWD_LONG,  SfxManager.APPLAUSE_1, SfxManager.APPLAUSE_2 });
        int index = Random.Range(0, crowd.Count);

        AudioClip clip = SfxManager.LoadClip(crowd[index]);
        audioSource.PlayOneShot(clip);   
        */
    }

    private void OnRunwayFinish(Collider other)
    {
        GameObject go = other.gameObject.transform.parent.gameObject;
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

    private void OnRunwayEndExit(Collider other) {
        UIManager.Instance.HideOutfit();
        if (showcaseManager.curOutfit == (showcaseManager.totalOutfits - 1))
            confetti.Play();
    }

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

    private void Update()
    {
        if (isModeActive == false)
            return;

        long userId = KinectManager.Instance.GetPrimaryUserID();

        if (userId == 0)
        {
            HideStart();
            return;
        }

        long newPrimeUserId = CheckUsersZPosition();

        if (newPrimeUserId == 0)
            return;

        AppointNewPrimeUser(newPrimeUserId);
    }

    private long CheckUsersZPosition()
    {
        if (KinectManager.Instance.GetUsersCount() <= 1)
            return 0;

        Vector3 primePos = KinectManager.Instance.GetUserPosition(KinectManager.Instance.GetPrimaryUserID());

        int amount = KinectManager.Instance.GetAllUserIds().Count;
        
        for (int x = 0; x < amount; x++)
        {
            long userId = KinectManager.Instance.GetUserIdByIndex(x);

            if (userId == 0)
                continue;

            Vector3 pos = KinectManager.Instance.GetUserPosition(userId);

            if(pos.z < primePos.z)
                return userId;
        }
        
        return 0;
    }

    private void AppointNewPrimeUser(long userId)
    {
        long primerUserId = KinectManager.Instance.GetPrimaryUserID();
        KinectManager.Instance.DeleteGesture(primerUserId, KinectGestures.Gestures.Wave);
        KinectManager.Instance.DeleteGesture(primerUserId, KinectGestures.Gestures.Tpose);

        KinectManager.Instance.SetPrimaryUserID(userId);
        KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.Wave);
        KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.Tpose);
    }

    private void AddKinectGestures(long userId)
    {
        KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.Wave);
        KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.Tpose);
    }

    private void RemoveKinectGestures(long userId)
    {
        KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.Wave);
        KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.Tpose);
    }

    private void HideStart()
    {
        if (faceCapture.isActiveAndEnabled == false)
            return;

        faceCapture.enabled = false;
        UIManager.Instance.HideStartMenu(true);
    }

    public void UserDetected(long userId, int userIndex)
    {
        if (isModeActive == false)
            return;

        if (faceCapture.isActiveAndEnabled == false)
            faceCapture.enabled = true;

        UIManager.Instance.ShowStartMenu(false);
        
        if (userId == KinectManager.Instance.GetPrimaryUserID())
        {
            AddKinectGestures(userId);
        }
    }

    public void UserLost(long userId, int userIndex)
    {
        if (isModeActive == false)
            return;

        RemoveKinectGestures(userId);
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

            faceCapture.enabled = false;

            RemoveKinectGestures(userId);

            UIManager.Instance.HideStartMenu(false);
            AppManager.Instance.TransitionToLive();

            return false;
        }
        if (gesture == KinectGestures.Gestures.Tpose)
        {
            if (isModeActive == false)
                return true;

            UIManager.Instance.HideStartMenu(false);
            AppManager.Instance.TransitionToNextAutoLevel();
            
            return true;
        }
        return true;
    }

    public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture, KinectInterop.JointType joint)
    {
        return true;
    }
}