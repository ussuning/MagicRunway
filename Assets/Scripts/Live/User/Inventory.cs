using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour {

    public int userIndex;    // 0 or 1
    protected Outfit selectedOutfit;    // current selected
    protected List<Outfit> userInventory;

    // Use this for initialization
    public void Initialize(int index, Outfits o)
    {
        userIndex = index;
    }

    public List<Outfit> getMaleOutfits()
    {
        return MRData.Instance.outfits.filter_outfits("m");

    }

    public List<Outfit> getFemaleOutfits()
    {
        return MRData.Instance.outfits.filter_outfits("f");
    }

    public void LoadInventory(string gender)
    {
        if(gender == "m")
        {
            userInventory = getMaleOutfits();
        }

        userInventory = getFemaleOutfits();

        // find Inventory gameObject and inject into scene - invoked from live runway manager
    }

    public void scrollOutfits()
    {
        // scroll outfit for each pull up or pull down gesture
    }

    protected Outfit getSelectedOutfit()
    {
        return selectedOutfit;
    }


}
