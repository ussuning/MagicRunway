using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteFader : MonoBehaviour {

    private float cur_alpha = 1f;
    private bool m_isFading = false;

    [SerializeField]
    private float fadingDuration = 1.0f;    //Time for fading
    private float fadeSpeed = 1.0f;

    public SpriteRenderer [] renderers;

    void Start()
    {
        cur_alpha = 1f;
        m_isFading = false;

        if (fadingDuration > 0)
            fadeSpeed = 1 / fadingDuration;
    }
	
	void Update ()
    {
		if(m_isFading)
        {
            cur_alpha -= Time.deltaTime * fadeSpeed;
            if (cur_alpha < 0f)
                cur_alpha = 0f;

            foreach (SpriteRenderer renderer in renderers)
            {
                renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, cur_alpha);
            }

            if (cur_alpha == 0f)
            {
                m_isFading = false;
                gameObject.SetActive(false);
            }
        }
	}

    public void StartFadingOut()
    {
        m_isFading = true;
    }

    public void Hide()
    {
        if(gameObject.activeInHierarchy)
            gameObject.SetActive(false);
    }

    public void Show()
    {
        if(!gameObject.activeInHierarchy)
            gameObject.SetActive(true);
    }
}
