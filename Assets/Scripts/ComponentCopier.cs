using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MR;
using Obi;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ComponentCopier : MonoBehaviour {

    public GameObject source;

    //public static T CopyComponent<T>(T original, GameObject destination) where T : Component
    //{
    //    System.Type type = original.GetType();
    //    Component copy = destination.AddComponent(type);
    //    System.Reflection.FieldInfo[] fields = type.GetFields();
    //    foreach (System.Reflection.FieldInfo field in fields)
    //    {
    //        field.SetValue(copy, field.GetValue(original));
    //    }
    //    return copy as T;
    //}
    public static T CopyComponent<T>(T original, GameObject destination) where T : Component
    {
        System.Type type = original.GetType();
        var dst = destination.GetComponent(type) as T;
        if (!dst) dst = destination.AddComponent(type) as T;
        var fields = type.GetFields();
        foreach (var field in fields)
        {
            if (field.IsStatic) continue;
            field.SetValue(dst, field.GetValue(original));
        }
        var props = type.GetProperties();
        foreach (var prop in props)
        {
            if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
            prop.SetValue(dst, prop.GetValue(original, null), null);
        }
        return dst as T;
    }

    public static void CopyComponents(GameObject source, GameObject destination)
    {
        SkinnedMeshRenderer meshRenderer = source.GetComponent<SkinnedMeshRenderer>();
        ComponentCopier.CopyComponent<SkinnedMeshRenderer>(meshRenderer, destination);

        Component[] components = source.GetComponents(typeof(Component));
        foreach (Component component in components)
        {
            //System.Type type = component.GetType();
            string typeString = component.GetType().ToString();
            Debug.Log(component.GetType());
            //if (component is ObiCloth)
            //{
            //    destination.AddComponent<ObiCloth>(component as ObiCloth); 
            //}
            //else 
            if (typeString.StartsWith("Obi"))
            {
                ComponentCopier.CopyComponent(component, destination);
            }
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(ComponentCopier))]
public class ComponentCopierEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ComponentCopier myScript = (ComponentCopier)target;
        if (GUILayout.Button("Copy Components"))
        {
            ComponentCopier.CopyComponents(myScript.source, myScript.gameObject);
        }
    }
}
#endif


