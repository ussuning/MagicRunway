using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClosetGenderItem : ClosetItemPress {

    public Sprite MaleIcon;
    public Sprite FemaleIcon;
    public Sprite MaleSelectedIcon;
    public Sprite FemaleSelectedIcon;

    public Color MaleColor = new Color(0f, 148f / 255f, 1f);
    public Color FemaleColor = new Color(1f, 127f / 255f, 237f / 255f);

    private User.Gender curGender;

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if(isSelecting)
        {
            if (curGender == User.Gender.Male)
                SelectedFillImage.color = FemaleColor;
            else if (curGender == User.Gender.Female)
                SelectedFillImage.color = MaleColor;
        }
        else
        {
            if (curGender == User.Gender.Male)
                SelectedFillImage.color = MaleColor;
            else if (curGender == User.Gender.Female)
                SelectedFillImage.color = FemaleColor;
        }
    }

    public void SetGender(User.Gender g)
    {
        curGender = g;
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
