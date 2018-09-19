using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelValidator
{
    public static void ValidateModel(GameObject model)
    {
        //make sure if the model is correctly made OR ELSE!!
        bool passLayerCheck = ValidateLayer(model);
        if (!passLayerCheck)
        {
            Debug.LogError(model.name + " did not pass layer checker. Set all layers to 'models'.");
        }
        bool passHipCheck = ValidateHip(model);
        if (!passHipCheck)
        {
            Debug.LogError(model.name + " did not pass hip checker. Model requires 'mixamorig:Hips'.");
        }
        bool passColliderCheck = ValidateCollider(model);
        if (!passColliderCheck)
        {
            Debug.LogError(model.name + " did not pass collider checker. Model requires a capsule collider.");
        }
    }

    public static bool ValidateLayer(GameObject go)
    {
        if (go == null)
            return false;

        //models layer is 13
        if (go.layer != 13)
            return false;

        foreach (Transform child in go.transform)
        {
            if (child == null)
                continue;

            if (child.gameObject.layer != 13)
                return false;
        }

        return true;
    }

    public static bool ValidateHip(GameObject go)
    {
        foreach (Transform child in go.transform)
        {
            if (child == null)
                continue;

            if (child.gameObject.name == "mixamorig:Hips")
                return true;
        }

        return false;
    }

    public static bool ValidateCollider(GameObject go)
    {
        foreach (Transform child in go.transform)
        {
            if (child == null)
                continue;

            if (child.gameObject.GetComponent<CapsuleCollider>())
            {
                CapsuleCollider cc = child.gameObject.GetComponent<CapsuleCollider>();

                if (cc.radius < 0.26f || cc.height < 1.94f)
                    return false;

                return true;
            }
        }

        return false;
    }

    public static void SetLayerRecursively(GameObject go, int newLayer)
    {
        if (go == null)
            return;

        go.layer = newLayer;

        foreach (Transform child in go.transform)
        {
            if (child == null)
                continue;

            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
