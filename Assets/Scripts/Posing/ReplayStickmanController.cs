using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReplayStickmanController : MonoBehaviour {

    public int replayMin = 3;
    public float walkingTranslationSpeed = 1f;
    public float walkingScalingSpeed = 0.005f;
    public float poseDuration = 0.5f;

    public RectTransform rendTrans;
    public GameObject replayPanel;

    private Animator anim;

    private Vector3 initAnchoredPos;
    private Vector3 initScale;

    private List<int> comboBuffer = new List<int>();
    private int replayIdx = 0;


    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void OnEnable()
    {
        EventMsgDispatcher.Instance.registerEvent(EventDef.Combo_Broken_Detected, ReplayCombo);
    }

    void OnDisable()
    {
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.Combo_Broken_Detected, ReplayCombo);
    }

    public void ReplayCombo(object[] param)
    {
        List<int> comboPoses = (List<int>)param[1];
        if (comboPoses.Count >= replayMin)
        {
            comboBuffer = new List<int>(comboPoses);
            StartReplay();
        }   
    }

    void Start()
    {
        initAnchoredPos = rendTrans.anchoredPosition;
        initScale = rendTrans.localScale;
    }

    void StartReplay()
    {
        //ResetStickman();
        ShowRender();
        StartCoroutine(MoveTowardsScreen());
    }

    void ResetStickman()
    {
        rendTrans.anchoredPosition = initAnchoredPos;
        initScale = rendTrans.localScale;

        anim.SetInteger("pose", 0);
    }

    IEnumerator MoveTowardsScreen()
    {
        while (rendTrans.position.y > Screen.height / 2f)
        {
            rendTrans.Translate(Vector3.down * walkingTranslationSpeed);
            rendTrans.localScale += Vector3.one * walkingScalingSpeed;
            yield return new WaitForFixedUpdate();
        }
        rendTrans.position = new Vector3(rendTrans.position.x, Screen.height / 2f, rendTrans.position.z);
        replayIdx = 0;
        PlayComboPoses();
    }

    void PlayComboPoses()
    {
        anim.SetInteger("pose", comboBuffer[replayIdx]);
        replayIdx++;
        if (replayIdx < comboBuffer.Count)
            Invoke("PlayComboPoses", poseDuration);
        else
            Invoke("HideRender", poseDuration);
    }

    void ShowRender()
    {
        replayPanel.SetActive(true);
    }

    void HideRender()
    {
        ResetStickman();
        replayPanel.SetActive(false);
    }

}
