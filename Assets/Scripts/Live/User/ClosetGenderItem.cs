using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClosetGenderItem : ClosetItem {

    public Sprite MaleIcon;
    public Sprite FemaleIcon;

    public void SetGender(User.Gender g)
    {
        if (g == User.Gender.Male)
            ItemImage.sprite = SelectedFillImage.sprite = MaleIcon;
        else if (g == User.Gender.Female)
            ItemImage.sprite = SelectedFillImage.sprite = FemaleIcon;
    }

    public override void OnItemSelected()
    {
        base.OnItemSelected();
        closet.SwapClosetGender();
    }
}
