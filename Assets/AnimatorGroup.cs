using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorGroup : MonoBehaviour {

	public RuntimeAnimatorController animatorController;
    protected RuntimeAnimatorController lastAnimatorController = null;

    private void Update()
    {
        if (animatorController != lastAnimatorController)
        {
            lastAnimatorController = animatorController;
            Animator[] animators = GetComponentsInChildren<Animator>();
            foreach (Animator animator in animators)
                animator.runtimeAnimatorController = animatorController;
        }
    }

}
