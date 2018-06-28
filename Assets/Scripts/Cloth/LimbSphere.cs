using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LimbSphere : LimbObject {

    public float scale = 1.0f;
    public SphereCollider sphereCollider;
	
	// Update is called once per frame
	void Awake () {
        if (sphereCollider == null)
            sphereCollider = GetComponent<SphereCollider>();
        
        // Preserve original local scale on init.
        if (transform.localScale.x != scale)
        {
            scale = transform.localScale.x;
        }

        Init();
	}

	private void Update()
    {
        if (scale != transform.localScale.x ||
            scale != transform.localScale.y ||
            scale != transform.localScale.z)
        {
            transform.localScale = new Vector3(scale, scale, scale);
        }
        DoUpdate();
	}
}
