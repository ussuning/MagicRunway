using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRunwayEvents : MonoBehaviour {

    public delegate void OnCollectionStart(Collection collection);
    public static event OnCollectionStart OnCollectionStartCallback;

    public static void CollectionStart(Collection collection)
    {
        if (OnCollectionStartCallback != null)
        {
            OnCollectionStartCallback(collection);
        }
    }

    public delegate void OnCollectionEnd(Collection collection);
    public static event OnCollectionEnd OnCollectionEndCallback;

    public static void CollectionEnd(Collection collection)
    {
        if (OnCollectionEndCallback != null)
        {
            OnCollectionEndCallback(collection);
        }
    }

    public delegate void OnLoopStart(Collection collection, int count);
    public static event OnLoopStart OnLoopStartCallback;

    public static void LoopStart(Collection collection, int count)
    {
        if (OnLoopStartCallback != null)
        {
            OnLoopStartCallback(collection, count);
        }
    }

    public delegate void OnLoopEnd(Collection collection, int count);
    public static event OnLoopEnd OnLoopEndCallback;

    public static void LoopEnd(Collection collection, int count)
    {
        if (OnLoopEndCallback != null)
        {
            OnLoopEndCallback(collection, count);
        }
    }

    public delegate void OnFinaleStart(Collection collection);
    public static event OnFinaleStart OnFinaleStartCallback;

    public static void FinaleStart(Collection collection)
    {
        if (OnFinaleStartCallback != null)
        {
            OnFinaleStartCallback(collection);
        }
    }

    public delegate void OnFinaleEnd(Collection collection);
    public static event OnFinaleEnd OnFinaleEndCallback;

    public static void FinaleEnd(Collection collection)
    {
        if (OnFinaleEndCallback != null)
        {
            OnFinaleEndCallback(collection);
        }
    }
}
