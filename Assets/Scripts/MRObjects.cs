using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Collections : System.Object
{
    public List<Collection> collections;
     
    public Dictionary<string, Collection> to_dict()
    {
        Dictionary<string, Collection> dict = new Dictionary<string, Collection>();

        for (int i = 0; i < collections.Count; i++)
        {
            dict.Add(collections[i].id, collections[i]);
        }

        return dict;
    }
}

[System.Serializable]
public class Collection : System.Object
{
    public string id;
    public string name;
    public List<string> outfitids;
    public List<Outfit> outfits;
    public string splash;
}

[System.Serializable]
public class Outfits : System.Object
{
    public List<Outfit> outfits;
    public List<Outfit> maleOutfits
    {
        get
        {
            string genderKey = "m";

            List<Outfit> filteredList = outfits
            .Where(row => row.sex == genderKey)
            .ToList();

            return filteredList;
        }
    }
    public List<Outfit> femaleOutfits
    {
        get
        {
            string genderKey = "f";

            List<Outfit> filteredList = outfits
            .Where(row => row.sex == genderKey)
            .ToList();

            return filteredList;
        }
    }

    public Dictionary<string, Outfit> to_dict()
    {
        Dictionary<string, Outfit> dict = new Dictionary<string, Outfit>();

        for (int i = 0; i < outfits.Count; i++)
        {
            dict.Add(outfits[i].id, outfits[i]);
        }

        return dict;
    }

    //Not used 
    public List<Outfit> filter_outfits(string gender)
    {
        if(gender == "m")
        {
            if(maleOutfits != null)
            {
                return maleOutfits;
            }
        }
      
        return femaleOutfits;
    }
}

[System.Serializable]
public class Outfit : System.Object
{
    public string id;
    public string name;
    public string sex;
    public string desc;
    public string prefab;
    public string icon;
    public List<string> wearableids;
    public List<Wearable> wearables;
}

[System.Serializable]
public class Wearables : System.Object
{
    public List<Wearable> wearables;
  
    public Dictionary<string, Wearable> to_dict()
    {
        Dictionary<string, Wearable> dict = new Dictionary<string, Wearable>();

        for (int i = 0; i < wearables.Count; i++)
        {
            dict.Add(wearables[i].id, wearables[i]);
        }

        return dict;
    }
}

[System.Serializable]
public class Wearable : System.Object
{
    public string id;
    public string name;
    public string img;
    public string sex;
    public string prefab;
    public string slot;
}