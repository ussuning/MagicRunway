using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum CanvasFade { IN, OUT };

public class CanvasFader : MonoBehaviour
{
    private float m_alpha = 1.0f;
    private bool m_isFading = false;

    [SerializeField]
    private CanvasFade fadeType = CanvasFade.IN;

    [SerializeField]
    private float fadingDuration = 1.0f;    //Time for fading
    private float fadeSpeed = 1.0f;

    [SerializeField]
    private bool m_startFadeOnAwake = false;

    private CanvasGroup _canvasGroup;

    void Awake()
    {
        if (fadeType == CanvasFade.IN)
            m_alpha = 0;
        else if (fadeType == CanvasFade.OUT)
            m_alpha = 1.0f;

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            Debug.LogError("[warning] CanvasFadeScript: please add a Canvas Group to the Canvas");

        if (fadingDuration > 0)
            fadeSpeed = 1 / fadingDuration;

        if (m_startFadeOnAwake)
            StartFading(fadeType);
    }

    public void StartFading(CanvasFade fade)
    {
        fadeType = fade;

        setCanvasAlpha();
        m_isFading = true;
    }

    void setCanvasAlpha()
    {
        if (_canvasGroup != null) _canvasGroup.alpha = m_alpha;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_isFading)
        {
            if (fadeType == CanvasFade.IN)
            {
                m_alpha += Time.deltaTime * fadeSpeed;
                if (m_alpha > 0.95f)
                    onFadeCompleted();
            }
            else if (fadeType == CanvasFade.OUT)
            {
                m_alpha -= Time.deltaTime * fadeSpeed;
                if (m_alpha < 0.05f)
                    onFadeCompleted();
            }

            setCanvasAlpha();
        }
    }

    public void onFadeCompleted()
    {
        m_isFading = false;

        UIEvents.CanvasFadeComplete(this.gameObject, fadeType);
    }
}
