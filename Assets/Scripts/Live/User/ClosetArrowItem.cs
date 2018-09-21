using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClosetArrowItem : ClosetItem {

    public enum Direction
    {
        Up = 0,
        Down,
    };

    //public Image ArrowImage;
    public Direction ArrowDirection;
    
    public void ShowArrow()
    {
        //ArrowImage.enabled = true;
        ShowItem();
    }

    public void HideArrow()
    {
        //ArrowImage.enabled = false;
        HideItem();
    }

    public override void OnItemSelected()
    {
        base.OnItemSelected();
        switch(ArrowDirection)
        {
            case Direction.Up:
                closet.PageUp();
                break;
            case Direction.Down:
                closet.PageDown();
                break;
        }
    }
}
