using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSkeleton : MonoBehaviour {
    public GameObject cubePrefab;
    protected List<GameObject> cubeInstances = new List<GameObject>();

    // Use this for initialization
    private void OnEnable()
    {
        if (cubePrefab == null)
            cubePrefab = AssetBundleManager.Instance.GetModelAsset("CubeJoint");

        AddCubesRecursive(transform);
    }

    private void OnDisable()
    {
        foreach (GameObject cube in cubeInstances)
            GameObject.Destroy(cube);
        cubeInstances.Clear();
    }

    void AddCubesRecursive(Transform t)
    {
        if (t.childCount == 0)
            return;


        bool skipChildren = false;
        // Skip fingers.
        if (t.name.Contains("Hand"))
            skipChildren = true;

        if (skipChildren == false)
        {
            for (int i = 0; i < t.childCount; i++)
            {
                AddCubesRecursive(t.GetChild(i));
            }
        }

        bool addCube = true;

        if (t.name.Contains("Spine2"))
            addCube = false;

        if (addCube)
        {
            //Debug.Log("Adding cube to " + t.name);
            GameObject go = GameObject.Instantiate(cubePrefab);
            cubeInstances.Add(go);
            go.transform.parent = t;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.SetAsLastSibling();
        }
    }
}
