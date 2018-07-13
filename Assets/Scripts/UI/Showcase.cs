﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Showcase : MonoBehaviour
{
    public GameObject showcaseEntry;

    private const int m_startY = 200;
    private const int m_spacing = 75;
    private const int m_entryLimit = 5;
    private const float m_interval = 0.2f;

    private List<GameObject> m_entries;

    private void Awake()
    {
        m_entries = new List<GameObject>();
    }

    private void Start()
    {
        
    }

    public void Show(Outfit outfit) 
    {
        if(m_entries.Count > 0) {
            Hide();
        }

        float currentY = -m_startY;

        int count = (outfit.wearables.Count < m_entryLimit) ? outfit.wearables.Count : m_entryLimit;

        for (int x = 0; x < count; x++) {
            GameObject go = makeShowcaseEntry(outfit.wearables[x]);

            RectTransform rectTransform = go.GetComponent<RectTransform>();
            rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, currentY, 0);

            m_entries.Add(go);

            ShowcaseEntry se = go.GetComponent<ShowcaseEntry>();
            se.Open(x*m_interval);

            currentY = currentY - rectTransform.rect.height - m_spacing;
        }
    }

    public void Hide(bool animate = false) {
        /*
        foreach(GameObject go in m_entries) {
            if (animate == true)
            {
                ShowcaseEntry se = go.GetComponent<ShowcaseEntry>();
                se.Close();
            }
            else
            {
                Destroy(go);
            }
        }
*/
        for (int x = 0; x < m_entries.Count; x++)
        {
            GameObject go = m_entries[x];
            if (animate == true)
            {
                ShowcaseEntry se = go.GetComponent<ShowcaseEntry>();
                se.Close(x * m_interval);
            }
            else
            {
                Destroy(go);
            }
        }
        m_entries = new List<GameObject>();
    }

    private GameObject makeShowcaseEntry(Wearable wearable) {
        string filePath = "thumbs/" + wearable.img;

        GameObject go = Instantiate(showcaseEntry,this.gameObject.transform) as GameObject;
        go.name = wearable.id;
        go.transform.Find("Text").GetComponent<Text>().text = wearable.name;
        go.transform.Find("Image").GetComponent<Image>().sprite = GetThumb(filePath);
        go.transform.localScale = Vector3.one;
        go.SetActive(true);

        return go;
    }

    private Sprite GetThumb(string path) {
        return Resources.Load<Sprite>(path);
    }
}
