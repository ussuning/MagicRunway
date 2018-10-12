using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClosetArrowItem : ClosetItem
{

    public enum Direction
    {
        Up = 0,
        Down,
    };

    //public Image ArrowImage;
    public Direction ArrowDirection;

    public float SelectionCooldown = 1f;
    private bool isInSelectionCD = false;

    public override void FixedUpdate()
    {
        if (isHover && !isSelected)
        {
            if (!isInSelectionCD)
            {
                hoverDuration += Time.fixedDeltaTime;
            }
        }
        else
        {
            isInSelectionCD = false;
        }
    }

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
        switch (ArrowDirection)
        {
            case Direction.Up:
                closet.PageUp();
                break;
            case Direction.Down:
                closet.PageDown();
                break;
        }

        StartCoroutine(OnPagingCooldown());
    }

    IEnumerator OnPagingCooldown()
    {
        isInSelectionCD = true;
        yield return new WaitForSeconds(SelectionCooldown);
        isInSelectionCD = false;

        base.OnItemUnselected();
    }
}


