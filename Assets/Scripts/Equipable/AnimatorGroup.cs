using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorGroup : MonoBehaviour {

	public RuntimeAnimatorController animatorController;
    protected RuntimeAnimatorController lastAnimatorController = null;
    protected EquipableSlots equipableSlots;

    private void Update()
    {
        if (equipableSlots == null)
        {
            equipableSlots = GetComponent<EquipableSlots>();
            equipableSlots.onEquip += OnEquipSwap;
        }

        if (animatorController != lastAnimatorController)
        {
            lastAnimatorController = animatorController;

            ResetAnim();
        }
    }

    private void ResetAnim()
    {
        Animator[] animators = GetComponentsInChildren<Animator>();
        foreach (Animator animator in animators)
        {
            animator.runtimeAnimatorController = null;
            animator.runtimeAnimatorController = animatorController;
        }
    }

    private void OnEquipSwap(EquipableSlot slot, GameObject equipment)
    {
        ResetAnim();
    }
}
