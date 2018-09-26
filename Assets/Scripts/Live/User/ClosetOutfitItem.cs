using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClosetOutfitItem : ClosetItem {

    public Image OutfitImage;
    private Outfit outfit;
    
    public void SetOutfit(Outfit outfit)
    {
        this.outfit = outfit;
        OutfitImage.sprite = GetOutfitThumb(outfit.icon);
        OutfitImage.enabled = true;
        ShowItem();
    }

    public void ClearOutfit()
    {
        this.outfit = null;
        OutfitImage.sprite = null;
        OutfitImage.enabled = false;
        HideItem();
    }

    public override void OnItemSelected()
    {
        base.OnItemSelected();

        OutfitGameObjectsManager.Instance.GenerateOutfit(outfit, closet.OwnerIndex);
    }

    private Sprite GetOutfitThumb(string icon)
    {
        if (icon.Contains("."))
        {
            icon = icon.Substring(0, icon.IndexOf("."));
        }
        string iconPath = "Thumbs/" + icon;
        //return Resources.Load<Sprite>(iconPath);

        AssetBundle ab = AssetBundleManager.Instance.GetAssetBundle(AssetBundles.clothingIcons);
        return ab.LoadAsset<Sprite>(icon);
    }
    
}
