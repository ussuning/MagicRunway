//  Copyright 2017 MaterialUI for Unity http://materialunity.com
//  Please see license file for terms and conditions of use, and more information.

using System;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MR
{
    /// <summary>
    /// Static class with <see cref="Func{T}"/> extension methods.
    /// </summary>
    public static class FuncExtension
    {
        /// <summary>
        /// Invokes a Func if not null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <returns></returns>
        public static T InvokeIfNotNull<T>(this Func<T> func)
        {
            if (func != null)
            {
                return func();
            }

            return default(T);
        }
    }

    /// <summary>
    /// Static class with <see cref="Transform"/> extension methods.
    /// </summary>
    public static class TransformExtension
    {
        /// <summary>
        /// Sets the parent and scale of a Transform.
        /// </summary>
        /// <param name="transform">The transform to modify.</param>
        /// <param name="parent">The new parent to set.</param>
        /// <param name="localScale">The local scale to set.</param>
        /// <param name="worldPositionStays">if set to <c>true</c> [world position stays].</param>
        public static void SetParentAndScale(this Transform transform, Transform parent, Vector3 localScale, bool worldPositionStays = false)
        {
            transform.SetParent(parent, worldPositionStays);
            transform.localScale = localScale;
        }

        /// <summary>
        /// Gets the root canvas from a transform.
        /// </summary>
        /// <param name="transform">The transform to use.</param>
        /// <returns>Returns root canvas if one found, otherwise returns null.</returns>
        public static Canvas GetRootCanvas(this Transform transform)
        {
            if (transform == null)
            {
                return null;
            }

            Canvas[] parentCanvases = transform.GetComponentsInParent<Canvas>();

            if (parentCanvases == null || parentCanvases.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < parentCanvases.Length; i++)
            {
                Canvas canvas = parentCanvases[i];
                if (canvas.isRootCanvas)
                {
                    return canvas;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Static class with <see cref="Action"/> extension methods.
    /// </summary>
    public static class ActionExtension
    {
        /// <summary>
        /// Invokes an <see cref="Action"/> if not null.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        public static void InvokeIfNotNull(this Action action)
        {
            if (action != null)
            {
                action.Invoke();
            }
        }

        /// <summary>
        /// Invokes an <see cref="Action{T}"/> if not null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action">The action.</param>
        /// <param name="parameter">The parameter.</param>
        public static void InvokeIfNotNull<T>(this Action<T> action, T parameter)
        {
            if (action != null)
            {
                action.Invoke(parameter);
            }
        }
    }

    /// <summary>
    /// Static class with <see cref="UnityEvent"/> extension methods.
    /// </summary>
    public static class UnityEventExtension
    {
        /// <summary>
        /// Invokes a <see cref="UnityEvent"/> if not null.
        /// </summary>
        /// <param name="unityEvent">The UnityEvent to invoke.</param>
        public static void InvokeIfNotNull(this UnityEvent unityEvent)
        {
            if (unityEvent != null)
            {
                unityEvent.Invoke();
            }
        }

        /// <summary>
        /// Invokes a <see cref="UnityEvent{T}"/> if not null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="unityEvent">The UnityEvent to invoke.</param>
        /// <param name="parameter">The argument used in the invocation.</param>
        public static void InvokeIfNotNull<T>(this UnityEvent<T> unityEvent, T parameter)
        {
            if (unityEvent != null)
            {
                unityEvent.Invoke(parameter);
            }
        }
    }

    /// <summary>
    /// Static class with <see cref="GameObject"/> extension methods.
    /// </summary>
    public static class GameObjectExtension
    {
        /// <summary>
        /// Gets a Component on a GameObject if it exists, otherwise add one.
        /// </summary>
        /// <typeparam name="T">The type of Component to add.</typeparam>
        /// <param name="gameObject">The game object to check/add to.</param>
        /// <returns>The Component instance.</returns>
        public static T GetAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject.GetComponent<T>() != null)
            {
                return gameObject.GetComponent<T>();
            }
            else
            {
                return gameObject.AddComponent<T>();
            }

        }

        /// <summary>
        /// Gets a child Component by name and type.
        /// </summary>
        /// <typeparam name="T">The type of Component.</typeparam>
        /// <param name="gameObject">The game object.</param>
        /// <param name="name">The name to search.</param>
        /// <returns>The Component found, otherwise null.</returns>
        public static T GetChildByName<T>(this GameObject gameObject, string name) where T : Component
        {
            T[] items = gameObject.GetComponentsInChildren<T>(true);

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].name == name)
                {
                    return items[i];
                }
            }

            return null;
        }

#if UNITY_EDITOR
        public static bool IsPrefabInstance(this GameObject gameObject)
        {
            return PrefabUtility.GetCorrespondingObjectFromSource(gameObject) != null || PrefabUtility.GetPrefabObject(gameObject) != null;
        }
#endif
    }

    /// <summary>
    /// Static class with <see cref="MonoBehaviour"/> extension methods.
    /// </summary>
    public static class MonoBehaviourExtension
    {
        /// <summary>
        /// Gets a Component on a GameObject if it exists, otherwise add one.
        /// </summary>
        /// <typeparam name="T">The type of Component to add.</typeparam>
        /// <returns>The Component instance.</returns>
        public static T GetAddComponent<T>(this MonoBehaviour monoBehaviour) where T : Component
        {
            if (monoBehaviour.GetComponent<T>() != null)
            {
                return monoBehaviour.GetComponent<T>();
            }

            return monoBehaviour.gameObject.AddComponent<T>();
        }

        /// <summary>
        /// Gets a child Component by name and type.
        /// </summary>
        /// <typeparam name="T">The type of Component.</typeparam>
        /// <param name="monoBehaviour">The MonoBehaviour.</param>
        /// <param name="name">The name to search.</param>
        /// <returns>The Component found, otherwise null.</returns>
        public static T GetChildByName<T>(this MonoBehaviour monoBehaviour, string name) where T : Component
        {
            return monoBehaviour.gameObject.GetChildByName<T>(name);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class ComponentExtension
    {
        /// <summary>
        /// Gets the name of the child by.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component">The component.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static T GetChildByName<T>(this Component component, string name) where T : Component
        {
            return component.gameObject.GetChildByName<T>(name);
        }
    }

    /// <summary>
    /// Static class with <see cref="Color"/> extension methods.
    /// </summary>
    public static class ColorExtension
    {
        /// <summary>
        /// Gets a color with a specified alpha level.
        /// </summary>
        /// <param name="color">The color to get.</param>
        /// <param name="alpha">The desired alpha level.</param>
        /// <returns>A Color with 'rgb' values from color argument, and 'a' value from alpha argument.</returns>
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        /// <summary>
        /// Uses <see cref="Mathf.Approximately"/> on the color level values of two colors to compare them.
        /// </summary>
        /// <param name="thisColor">The first Color to compare.</param>
        /// <param name="otherColor">The second Color to compare.</param>
        /// <param name="compareAlpha">Should the alpha levels also be compared?</param>
        /// <returns>True if the first Color is approximately the second Color, otherwise false.</returns>
        public static bool Approximately(this Color thisColor, Color otherColor, bool compareAlpha = false)
        {
            if (!Mathf.Approximately(thisColor.r, otherColor.r)) return false;
            if (!Mathf.Approximately(thisColor.g, otherColor.g)) return false;
            if (!Mathf.Approximately(thisColor.b, otherColor.b)) return false;
            if (!compareAlpha) return true;
            return Mathf.Approximately(thisColor.a, otherColor.a);
        }
    }

    /// <summary>
    /// Static class with <see cref="RectTransform"/> extension methods.
    /// </summary>
    public static class RectTransformExtension
    {
        /// <summary>Sometimes sizeDelta works, sometimes rect works, sometimes neither work and you need to get the layout properties.
        ///	This method provides a simple way to get the size of a RectTransform, no matter what's driving it or what the anchor values are.
        /// </summary>
        /// <param name="rectTransform">The rect transform to check.</param>
        /// <returns>The proper size of the RectTransform.</returns>
        public static Vector2 GetProperSize(this RectTransform rectTransform) //, bool attemptToRefreshLayout = false)
        {
            Vector2 size = new Vector2(rectTransform.rect.width, rectTransform.rect.height);

            if (size.x == 0 && size.y == 0)
            {
                LayoutElement layoutElement = rectTransform.GetComponent<LayoutElement>();

                if (layoutElement != null)
                {
                    size.x = layoutElement.preferredWidth;
                    size.y = layoutElement.preferredHeight;
                }
            }
            if (size.x == 0 && size.y == 0)
            {
                LayoutGroup layoutGroup = rectTransform.GetComponent<LayoutGroup>();

                if (layoutGroup != null)
                {
                    size.x = layoutGroup.preferredWidth;
                    size.y = layoutGroup.preferredHeight;
                }
            }

            if (size.x == 0 && size.y == 0)
            {
                size.x = LayoutUtility.GetPreferredWidth(rectTransform);
                size.y = LayoutUtility.GetPreferredHeight(rectTransform);
            }

            return size;
        }

        /// <summary>
        /// Gets the position regardless of pivot.
        /// </summary>
        /// <param name="rectTransform">The rect transform.</param>
        /// <returns>The position in world space.</returns>
        public static Vector3 GetPositionRegardlessOfPivot(this RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            return (corners[0] + corners[2]) / 2;
        }

        /// <summary>
        /// Gets the local position regardless of pivot.
        /// </summary>
        /// <param name="rectTransform">The rect transform.</param>
        /// <returns>The position in local space.</returns>
        public static Vector3 GetLocalPositionRegardlessOfPivot(this RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetLocalCorners(corners);
            return (corners[0] + corners[2]) / 2;
        }

        /// <summary>
        /// Sets the x value of a RectTransform's anchor.
        /// </summary>
        /// <param name="rectTransform">The rect transform.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        public static void SetAnchorX(this RectTransform rectTransform, float min, float max)
        {
            rectTransform.anchorMin = new Vector2(min, rectTransform.anchorMin.y);
            rectTransform.anchorMax = new Vector2(max, rectTransform.anchorMax.y);
        }

        /// <summary>
        /// Sets the y value of a RectTransform's anchor
        /// </summary>
        /// <param name="rectTransform">The rect transform.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        public static void SetAnchorY(this RectTransform rectTransform, float min, float max)
        {
            rectTransform.anchorMin = new Vector2(rectTransform.anchorMin.x, min);
            rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, max);
        }

        /// <summary>
        /// Gets the root canvas of a RectTransform.
        /// </summary>
        /// <param name="rectTransform">The rect transform to get the root canvas of.</param>
        //public static Canvas GetRootCanvas(this RectTransform rectTransform)
        //{
        //    Canvas[] parentCanvases = rectTransform.GetComponentsInParent<Canvas>();

        //    for (int i = 0; i < parentCanvases.Length; i++)
        //    {
        //        Canvas canvas = parentCanvases[i];
        //        if (canvas.isRootCanvas)
        //        {
        //            return canvas;
        //        }
        //    }

        //    return null;
        //}
    }


    // From https://answers.unity.com/questions/799429/transformfindstring-no-longer-finds-grandchild.html
    public static class TransformDeepChildExtension
    {
        //Breadth-first search
        public static Transform FindDeepChild(this Transform aParent, string aName)
        {
            if (aParent.name == aName)
                return aParent;

            var result = aParent.Find(aName);
            if (result != null)
                return result;
            foreach (Transform child in aParent)
            {
                result = child.FindDeepChild(aName);
                if (result != null)
                    return result;
            }
            return null;
        }
    }

    public static class GameObjectFindAnyExtension
    {
        public static T FindAny<T>(this GameObject gameObj,  string name) where T : Component
        {
            T[] objs = Resources.FindObjectsOfTypeAll<T>();
            foreach (T obj in objs)
                if (obj.name == name)
                    return obj;

            return null;
        }
    }

    public static class GameObjectAddComponent
    {
        public static T GetCopyOf<T>(this Component comp, T other) where T : Component
        {
            Type type = comp.GetType();
            if (type != other.GetType()) return null; // type mis-match
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }
            FieldInfo[] finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }
            return comp as T;
        }

        public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
        {
            return go.AddComponent<T>().GetCopyOf(toAdd) as T;
        }
    }

    public static class Vector3RotateAroundPivotExtension
    {
        // From https://answers.unity.com/questions/532297/rotate-a-vector-around-a-certain-point.html -HH
        public static Vector3 RotatePointAroundPivot(this Vector3 point, Vector3 pivot, Vector3 angles) {
            Vector3 dir = point - pivot; // get point direction relative to pivot
            dir = Quaternion.Euler(angles) * dir; // rotate it
            point = dir + pivot; // calculate rotated point
            return point; // return it
        }
    }

    public class Vector3Helper
    {
        public static Vector3 SmoothStep(Vector3 from, Vector3 to, float t)
        {
            return new Vector3(
                Mathf.SmoothStep(from.x, to.x, t),
                Mathf.SmoothStep(from.y, to.y, t),
                Mathf.SmoothStep(from.z, to.z, t));
        }
    }
}