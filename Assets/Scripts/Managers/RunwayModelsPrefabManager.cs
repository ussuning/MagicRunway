using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunwayModelsPrefabManager : MonoBehaviour {

    private static readonly List<GameObject> m_Prefabs = new List<GameObject>();
    private static readonly List<string> m_Names = new List<string>();

	public static class RunwayModelsPrefabs
    {
        public const string testFemale = "RunwayModels/Female/test_outfit_01";
    }

    public static GameObject GetRunwayModel(string path)
    {
        GameObject go = null;

        if (!m_Names.Contains(path))
        {
            go = Resources.Load<GameObject>(path);

            if (go != null)
            {
                m_Prefabs.Add(go);
                m_Names.Add(path);
            }
        }
        else
        {
            for (int i = 0; i < m_Prefabs.Count; i++)
            {
                if (m_Names[i] == path)
                {
                    if (m_Prefabs[i] != null)
                    {
                        go = m_Prefabs[i];
                    }
                }
            }
        }

        return go;
    }

    public static GameObject InstantiateGameObject(string path, Transform parent)
    {
        GameObject go = GetRunwayModel(path);

        if (go == null)
        {
            return null;
        }

        go = GameObject.Instantiate(go);

        if (parent == null)
        {
            return go;
        }

        go.transform.SetParent(parent);
        go.transform.localScale = Vector3.one;
        go.transform.localEulerAngles = Vector3.zero;
        go.transform.localPosition = Vector3.zero;

        return go;
    }
}
