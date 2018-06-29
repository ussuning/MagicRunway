using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CollectionDisplay : MonoBehaviour
{
    public GameObject collectionEntry;

    private GameObject m_shownEntry;

    public void ShowCollection(Collection collection) {
        if (m_shownEntry != null)
        {
            Hide(true);
        }

        GameObject go = makeCollectionEntry(collection.name);

        CollectionEntry ce = go.GetComponent<CollectionEntry>();
        ce.Open();
        m_shownEntry = go;
    }

    public void Hide(bool animate = false)
    {
        if (m_shownEntry == null) { return; }

        if (animate == true)
        {
            CollectionEntry ce = m_shownEntry.GetComponent<CollectionEntry>();
            ce.Close();
        }
        else
        {
            Destroy(m_shownEntry);
        }

        m_shownEntry = null;
    }

    private GameObject makeCollectionEntry(string collectionName)
    {
        GameObject go = Instantiate(collectionEntry, this.gameObject.transform) as GameObject;
        go.name = collectionName;
        go.transform.Find("Text").GetComponent<Text>().text = collectionName;
        go.transform.localScale = Vector3.one;
        go.SetActive(true);

        return go;
    }
}
