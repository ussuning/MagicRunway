using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosetItemPress : ClosetItem
{
    public float CooldownTime = 1f;
    private bool isCD = false;

    public override void FixedUpdate()
    {
        if (isHover && !isSelected)
        {
            if (!isCD)
            {
                hoverDuration += Time.fixedDeltaTime;
            }
        }
        else
        {
            isCD = false;
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
