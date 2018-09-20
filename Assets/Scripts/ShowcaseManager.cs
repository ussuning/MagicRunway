using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowcaseManager {
    public Collection currentCollection;
    public Collection nextCollection;

    private List<Collection> _collections;
    
    private int _totalCollections = 0;
    public int currentCollectionIndex = 0;
    public int curOutfit = 0;
    public int totalOutfits = 0;

    public ShowcaseManager()
    {
        currentCollectionIndex = 0;
    }

    public void SetCollection(List<Collection> collections)
    {
        _collections = collections;
        _totalCollections = collections.Count;
    }

    public List<Outfit> PrepareShow()
    {
        currentCollection = _collections[currentCollectionIndex];
        nextCollection = GetNextCollection();

        curOutfit = 0;
        totalOutfits = currentCollection.outfits.Count;

        return currentCollection.outfits;
    }

    public Outfit GetCurrentOutfit()
    {
        if (curOutfit > (totalOutfits - 1))
            return null;

        return currentCollection.outfits[curOutfit];
    }

    public bool NextOutfit()
    {
        curOutfit++;

        bool queueEnd = (curOutfit == totalOutfits) ? true : false;

        if (queueEnd == true)
        {
            return true;
        }

        return false;
    }

    public void ReadyFirstShow()
    {
        currentCollectionIndex = 0;
        AppManager.Instance.currentAutoLevel = currentCollectionIndex;
    }

    public void ReadyNextShow()
    {
        currentCollectionIndex++;

        if (currentCollectionIndex == _totalCollections)
            currentCollectionIndex = 0;

        AppManager.Instance.currentAutoLevel = currentCollectionIndex;
    }

    public void ReadyShowAt(int level = 0)
    {
        currentCollectionIndex = level;
    }

    private Collection GetNextCollection()
    {
        int nextCollectionIndex = currentCollectionIndex + 1;
        if (nextCollectionIndex == _totalCollections) { nextCollectionIndex = 0; }
        Collection nextCollection = _collections[nextCollectionIndex];
        return nextCollection;
    }
}
