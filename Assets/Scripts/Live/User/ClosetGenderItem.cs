using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClosetGenderItem : ClosetItemPress {

    public Sprite MaleIcon;
    public Sprite FemaleIcon;
    public Sprite MaleSelectedIcon;
    public Sprite FemaleSelectedIcon;

    public void SetGender(User.Gender g)
    {
        if (g == User.Gender.Male)
        {
            ItemImage.sprite = MaleIcon;
            SelectedFillImage.sprite = MaleSelectedIcon;
        }
        else if (g == User.Gender.Female)
        {
            ItemImage.sprite = FemaleIcon;
            SelectedFillImage.sprite = FemaleSelectedIcon;
        }
    }

    public override void OnItemSelected()
    {
        base.OnItemSelected();
        closet.SwapClosetGender();
    }
}
