using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Showcase : MonoBehaviour
{
    public GameObject container;
    public GameObject showcaseEntry;

    private const int m_startY = 100;
    private const int m_spacing = 50;
    private const int m_entryLimit = 10;

    private List<GameObject> m_entries;

    private void Start()
    {
        m_entries = new List<GameObject>();
    }

    public void Show(Outfit outfit) {
        if(m_entries.Count > 0) {
            Hide();
        }

        float currentY = -m_startY;

        for (int x = 0; x < m_entryLimit; x++) {
            //Debug.Log(x);
            GameObject go = makeShowcaseEntry(outfit.wearables[0]);

            RectTransform rectTransform = go.GetComponent<RectTransform>();
            rectTransform.localPosition = new Vector3(0, currentY, 0);

            Debug.Log(rectTransform.rect.height);
            m_entries.Add(go);

            currentY = currentY - rectTransform.rect.height - m_spacing;
        }
    }

    public void Hide() {
        foreach(GameObject go in m_entries) {
            Destroy(go);
        }
        m_entries = new List<GameObject>();
    }

    private GameObject makeShowcaseEntry(Wearable wearable) {
        string filePath = "thumbs/" + wearable.img;

        GameObject de = Instantiate(showcaseEntry,this.gameObject.transform) as GameObject;
        de.name = wearable.id;
        de.transform.Find("Text").GetComponent<Text>().text = wearable.name;
        de.transform.Find("Image").GetComponent<Image>().sprite = GetThumb(filePath);
        de.transform.localScale = Vector3.one;
        de.SetActive(true);

        return de;
    }

    private Sprite GetThumb(string path) {
        return Resources.Load<Sprite>(path);
    }
}
