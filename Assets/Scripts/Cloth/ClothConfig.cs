using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothConfig : MonoBehaviour {
    Cloth cloth;
	// Use this for initialization
	void Start () {
        cloth = GetComponent<Cloth>();
        if (cloth == null) {
            Debug.LogError("No Cloth component found for ClothConfig!");
        }
        Debug.Log("cloth.useVirtualParticles" + cloth.useVirtualParticles);
        cloth.useVirtualParticles = 0;
        List<uint> indices = new List<uint> { 0, 1, 2};

        cloth.SetVirtualParticleIndices(indices);
        List<Vector3> weights = new List<Vector3>();
        weights.Add(new Vector3(0.66f, 0.33f, 0.33f));
        weights.Add(new Vector3(0.33f, 0.66f, 0.33f));
        weights.Add(new Vector3(0.33f, 0.33f, 0.66f));
        cloth.SetVirtualParticleWeights(weights);

        ClothSkinningCoefficient[] newConstraints = cloth.coefficients;
        newConstraints[0].maxDistance = 0.01f;
        //cloth.coefficients = newConstraints;
	}
}
