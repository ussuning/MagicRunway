using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// manages outfit prefabs to send to models in Demo and Live mode
public class OutfitPool : MonoBehaviour {
    
    public GameObject outfitPrefab;
    private Dictionary<int, GameObject> inactiveInstances;

    // Returns an instance of the Outfit prefab from pool or create new 
    public GameObject GetObject(int outfitId)
    {
        GameObject spawnedGameObject;

        if (inactiveInstances.ContainsKey(outfitId))
        {
            spawnedGameObject = inactiveInstances[outfitId];
            inactiveInstances.Remove(outfitId);
        }
        else
        {
            spawnedGameObject = (GameObject)GameObject.Instantiate(outfitPrefab);
            PooledObject pooledObject = spawnedGameObject.AddComponent(typeof(PooledObject)) as PooledObject;
            pooledObject.pool = this;
        }

       
        spawnedGameObject.SetActive(true);
        return spawnedGameObject;
    }

    // Return an instance of the Outfit prefab to the pool when model changes outfit
    public void ReturnObject(int outfitId, GameObject toReturn)
    {
        PooledObject pooledObject = toReturn.GetComponent(typeof(PooledObject)) as PooledObject;

        // if the instance came from this pool, return it to the pool
        if (pooledObject != null && pooledObject.pool == this)
        {
            toReturn.SetActive(false);
            inactiveInstances.Add(outfitId, toReturn);
        }
        // otherwise, just destroy it
        else
        {
            Debug.LogWarning(toReturn.name + " was returned to a pool it wasn't spawned from. Kill it!");
            Destroy(toReturn);
        }
    }
}

public class PooledObject : MonoBehaviour
{
    public OutfitPool pool;
}

