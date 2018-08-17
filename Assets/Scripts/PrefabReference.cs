using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabReference : MonoBehaviour {

    public GameObject prefab;
    public GameObject instance;

    public void Load(int playerIndex)
    {
        if (prefab == null)
        {
            Debug.LogError("PrefabReference::Load() ERROR: Prefab reference is missing!");
            return;
        }
        if (instance != null)
        {
            Debug.LogWarning("There is already an instance of " + prefab.name);
        } else
        {
            instance = GameObject.Instantiate(prefab);
        }

        AvatarControllerBootstrap acBootstrap = instance.GetComponent<AvatarControllerBootstrap>();
        if (acBootstrap == null)
        {
            acBootstrap = instance.AddComponent<AvatarControllerBootstrap>();
        }

        acBootstrap.Init(playerIndex);
    }

    public void Unload()
    {
        if (instance != null)
        {
            GameObject.Destroy(instance);
        }
    }

    private void OnDestroy()
    {
        Unload();
    }
}
