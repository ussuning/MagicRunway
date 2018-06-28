using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LimbObject : MonoBehaviour {

    public LimbObject mirror;
	
	// Update is called once per frame
	void Awake () {
        Init();
    }

    void Update()
    {
        DoUpdate();
    }

    protected void Init() {
    }

	protected void DoUpdate()
	{

        if (mirror != null) {
            Vector3 mirrorPosition = transform.localPosition;
            mirrorPosition.x = -mirrorPosition.x;
            if (mirror.transform.localPosition != mirrorPosition)
                mirror.transform.localPosition = mirrorPosition;
            
            if (mirror.transform.localScale != transform.localScale)
                mirror.transform.localScale = transform.localScale;

            Vector3 mirrorRotation = transform.localEulerAngles;
            mirrorRotation.z = -mirrorRotation.z;
            mirrorRotation.y = -mirrorRotation.y;

            if (mirror.transform.localEulerAngles != mirrorRotation)
                mirror.transform.localEulerAngles = mirrorRotation;
        }
	}
}
