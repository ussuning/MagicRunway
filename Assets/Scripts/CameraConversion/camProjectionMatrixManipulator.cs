using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camProjectionMatrixManipulator : MonoBehaviour {
    public Matrix4x4 projectionMatrix = Matrix4x4.identity;
    public Matrix4x4 offsetMatrix = Matrix4x4.zero;
    protected Camera cam;
	// Use this for initialization
	void Start () {
        cam = GetComponent<Camera>();
        projectionMatrix = cam.projectionMatrix;
        for (int row = 0; row < 4; row++)
        {
            Vector4 values = projectionMatrix.GetRow(row);
            Vector4 offsets = offsetMatrix.GetRow(row);
            values += offsets;
            projectionMatrix.SetRow(row, values);
        }

	}
	
	// Update is called once per frame
	void Update () {
		if (projectionMatrix != cam.projectionMatrix)
        {
            cam.projectionMatrix = projectionMatrix;
        }
	}
}
