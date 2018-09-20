using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosetArrowItem : ClosetItem {

    public enum Direction
    {
        Up = 0,
        Dowm,
    };

    public Direction ArrowDirection;

    public override void OnItemSelected()
    {
        base.OnItemSelected();
        switch(ArrowDirection)
        {
            case Direction.Up:
                closet.PageUp();
                break;
            case Direction.Dowm:
                closet.PageDown();
                break;
        }
    }
}
