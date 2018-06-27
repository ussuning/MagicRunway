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
}
