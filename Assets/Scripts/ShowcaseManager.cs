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
    private int _totalOutfits = 0;

    public ShowcaseManager(List<Collection> collections)
    {
        _collections = collections;
        _totalCollections = collections.Count;
        currentCollectionIndex = 0;
    }

    public List<Outfit> PrepareShow()
    {
        currentCollection = _collections[currentCollectionIndex];
        nextCollection = GetNextCollection();

        curOutfit = 0;
        _totalOutfits = currentCollection.outfits.Count;

        return currentCollection.outfits;
    }

    public Outfit GetCurrentOutfit()
    {
        return currentCollection.outfits[curOutfit];
    }

    public bool NextOutfit()
    {
        curOutfit++;

        bool queueEnd = (curOutfit == _totalOutfits) ? true : false;

        if (queueEnd == true)
        {
            return true;
        }

        return false;
    }

    public void ReadyNextShow()
    {
        currentCollectionIndex++;

        if (currentCollectionIndex == _totalCollections)
            currentCollectionIndex = 0;
    }

    private Collection GetNextCollection()
    {
        int nextCollectionIndex = currentCollectionIndex + 1;
        if (nextCollectionIndex == _totalCollections) { nextCollectionIndex = 0; }
        Collection nextCollection = _collections[nextCollectionIndex];
        return nextCollection;
    }
}
