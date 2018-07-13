using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MR;

public class UpNext : MonoBehaviour
{
    private CanvasGroup m_canvasGroup;
    private Image m_circleImage;
    private Text m_title;

    private IEnumerator m_coroutine;
    private byte m_AnimState;
    private float m_AnimStartTime;
    private float m_AnimDeltaTime;
    private float m_AnimationDuration = 1.0f;

    private void Awake()
    {
        m_canvasGroup = GetComponent<CanvasGroup>();
        m_circleImage = gameObject.transform.Find("Image").GetComponent<Image>();
        m_title = gameObject.transform.Find("Text").GetComponent<Text>();
        m_title.text = "";
        m_AnimState = 0;
    }

    void Start()
    {
    }

    public void StartUpNext(string collectionName)
    {
        m_canvasGroup.alpha = 0;
        
        m_circleImage.gameObject.SetActive(false);
        m_title.text = "Up Next... " + collectionName + " Collection";

        m_AnimState = 1;
    }

    public void StartUpNext(string collectionName, float totalTimeSeconds = 10.0f, float warningTimeSeconds = 5.0f)
    {
        if(totalTimeSeconds <= 1.0f) {
            Debug.Log("Total time can't be lower than 1 second...cause I say so.");
            return;
        }

        if (warningTimeSeconds > totalTimeSeconds) {
            Debug.Log("Warning time should not be higher that total time");
            return;
        }
        ResetUpNext();

        m_title.text = "Up Next... " + collectionName + " Collection";

        float waitTime = totalTimeSeconds - warningTimeSeconds;

        m_coroutine = RadialProgress(waitTime, warningTimeSeconds);
        StartCoroutine(m_coroutine);
    }

    public void ResetUpNext()
    {
        m_AnimState = 0;
        m_canvasGroup.alpha = 0;
        m_circleImage.fillAmount = 0;

        if (m_coroutine == null) { return; }
        StopCoroutine(m_coroutine);
    }

    private void showWarning() {
        m_canvasGroup.alpha = 0;
        m_AnimStartTime = Time.realtimeSinceStartup;
        m_AnimState = 1;
    }

    public void Hide() {
        m_AnimStartTime = Time.realtimeSinceStartup;
        m_AnimState = 2;
    }

    IEnumerator RadialProgress(float waitTimeSeconds, float warningTimeSeconds)
    {
        yield return new WaitForSeconds(waitTimeSeconds);
        showWarning();
        float rate = 1 / warningTimeSeconds;
        float i = 0;
        while (i < 1)
        {
            i += Time.deltaTime * rate;

            m_circleImage.fillAmount = i;
            //image.color = Color.Lerp(Color.green, Color.white, i);
            yield return 0;
        }
        if (i >= 1)
        {
            complete();
        }
    }

    private void complete() {
        Hide();
        UIEvents.UpNextComplete();
    }

    void Update()
    {
        if (m_AnimState == 1)
        {
            m_AnimDeltaTime = Time.realtimeSinceStartup - m_AnimStartTime;

            if (m_AnimDeltaTime <= m_AnimationDuration)
            {
                m_canvasGroup.alpha = Tween.Linear(m_canvasGroup.alpha, 1.0f, m_AnimDeltaTime, m_AnimationDuration);
            }
            else
            {
                m_canvasGroup.alpha = 1.0f;
                m_AnimState = 0;
            }
        }

        if (m_AnimState == 2)
        {
            m_AnimDeltaTime = Time.realtimeSinceStartup - m_AnimStartTime;

            if (m_AnimDeltaTime <= m_AnimationDuration)
            {
                m_canvasGroup.alpha = Tween.Linear(m_canvasGroup.alpha, 0.0f, m_AnimDeltaTime, m_AnimationDuration);
            }
            else
            {
                ResetUpNext();
            }
        }
    }
}
