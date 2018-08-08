using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSkeleton : MonoBehaviour {
    public GameObject cubePrefab;

	// Use this for initialization
	void Start () {
        AddCubesRecursive(transform);
	}
	
    void AddCubesRecursive(Transform t)
    {
        if (t.childCount == 0)
            return;

        for (int i=0; i<t.childCount; i++)
        {
            AddCubesRecursive(t.GetChild(i));
        }

        Debug.Log("Adding cube to " + t.name);
        GameObject go = GameObject.Instantiate(cubePrefab);
        go.transform.parent = t;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
    }
}
