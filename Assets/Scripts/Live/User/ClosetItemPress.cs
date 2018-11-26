using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosetItemPress : ClosetItem
{
    protected bool isSelecting = false;

    private float CooldownTime = 1f;
    private bool isCD = false;


    public override void FixedUpdate()
    {
        if (isHover && !isSelected)
        {
            if (!isCD)
            {
                hoverDuration += Time.fixedDeltaTime;
                isSelecting = true;
            }
            else
            {
                isSelecting = false;
            }
        }
        else
        {
            isCD = false;
            isSelecting = false;
        }
    }

    public override void OnItemSelected()
    {
        base.OnItemSelected();
        StartCoroutine(OnPressCooldown());
    }

    IEnumerator OnPressCooldown()
    {
        isCD = true;
        yield return new WaitForSeconds(CooldownTime);
        isCD = false;

        base.OnItemUnselected();
    }
}
