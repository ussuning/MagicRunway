using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEvents
{
    public delegate void OnCanvasFadeComplete(GameObject go, CanvasFade fade);
    public static event OnCanvasFadeComplete OnCanvaseFadeCompleteCallback;

    public static void CanvasFadeComplete(GameObject go, CanvasFade fade) {
        if (OnCanvaseFadeCompleteCallback != null) {
            OnCanvaseFadeCompleteCallback(go, fade);
        }
    }

    //Event when up next timer is completed
    public delegate void OnUpNextComplete();
    public static event OnUpNextComplete OnUpNextCompleteCallback;

    public static void UpNextComplete()
    {
        if (OnUpNextCompleteCallback != null)
        {
            OnUpNextCompleteCallback();
        }
    }

    // Event
    public delegate void OnNextCollectionExecute(Collection upNext);
    public static event OnNextCollectionExecute OnNextCollectionExecuteCallback;

    public static void NextCollectionExecute(Collection upNext)
    {
        if (OnNextCollectionExecuteCallback != null)
        {
            OnNextCollectionExecuteCallback(upNext);
        }
    }
}
