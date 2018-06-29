using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MR;

public class ShowcaseEntry : MonoBehaviour
{
    private RectTransform m_RectTransform;
    private Image m_reveal;
    private byte m_AnimState;
    private float m_AnimStartTime;
    private float m_AnimDeltaTime;
    private float m_AnimationDuration = 1.0f;

    private Vector2 m_CurrentPos;
    private float m_MaxPosition;
    private float m_MinPosition;

    private void Awake()
    {
        m_RectTransform = gameObject.GetComponent<RectTransform>();
        m_reveal = gameObject.transform.Find("Reveal").GetComponent<Image>();
    }

    void Start()
    {
        m_MaxPosition = 0;
        m_MinPosition = -m_RectTransform.rect.width;
    }

    public void Open(float delay = 0.0f)
    {
        if (delay > 0.0f) {
            StartCoroutine(WaitThenOpen(delay));
        } else {
            StartOpen();
        }
    }

    public void Close(float delay = 0.0f) {
        if (delay > 0.0f)
        {
            StartCoroutine(WaitThenClose(delay));
        }
        else
        {
            StartClose();
        }
    }

    IEnumerator WaitThenOpen(float delay) {
        yield return new WaitForSeconds(delay);
        StartOpen();
    }

    private void StartOpen() {
        m_CurrentPos = m_RectTransform.anchoredPosition;
        m_reveal.color = new Color(255.0f, 255.0f, 255.0f, 1.0f);
        m_AnimStartTime = Time.realtimeSinceStartup;
        m_AnimState = 1;
    }

    IEnumerator WaitThenClose(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartClose();
    }

    private void StartClose()
    {
        m_CurrentPos = m_RectTransform.anchoredPosition;
        m_reveal.color = new Color(255.0f, 255.0f, 255.0f, 0.0f);
        m_AnimStartTime = Time.realtimeSinceStartup;
        m_AnimState = 2;
    }

    void Update()
    {
        if (m_AnimState == 1)
        {
           
            m_AnimDeltaTime = Time.realtimeSinceStartup - m_AnimStartTime;

            if (m_AnimDeltaTime <= m_AnimationDuration)
            {
                m_RectTransform.anchoredPosition = Tween.SeptOut(m_CurrentPos, new Vector2(m_MaxPosition, m_RectTransform.anchoredPosition.y), m_AnimDeltaTime, m_AnimationDuration);
                m_reveal.color = Tween.Linear(m_reveal.color, new Color(255.0f, 255.0f, 255.0f, 0.0f), m_AnimDeltaTime, m_AnimationDuration);
            }
            else
            {
                m_RectTransform.anchoredPosition = new Vector2(m_MaxPosition, m_RectTransform.anchoredPosition.y);
                m_reveal.color = new Color(255.0f, 255.0f, 255.0f, 0.0f);

                m_AnimState = 0;
            }
        }

        if (m_AnimState == 2)
        {
            m_AnimDeltaTime = Time.realtimeSinceStartup - m_AnimStartTime;

            if (m_AnimDeltaTime <= m_AnimationDuration)
            {
                m_RectTransform.anchoredPosition = Tween.SeptIn(m_CurrentPos, new Vector2(m_MinPosition, m_RectTransform.anchoredPosition.y), m_AnimDeltaTime, m_AnimationDuration);
                m_reveal.color = Tween.Linear(m_reveal.color, new Color(255.0f, 255.0f, 255.0f, 1.0f), m_AnimDeltaTime, m_AnimationDuration);
            }
            else
            {
                m_RectTransform.anchoredPosition = new Vector2(m_MaxPosition, m_RectTransform.anchoredPosition.y);
                m_reveal.color = new Color(255.0f, 255.0f, 255.0f, 1.0f);

                m_AnimState = 0;

                Destroy(this.gameObject);
            }
        }

        m_RectTransform.anchoredPosition = new Vector2(Mathf.Clamp(m_RectTransform.anchoredPosition.x, m_MinPosition, m_MaxPosition), m_RectTransform.anchoredPosition.y);
    }
}
