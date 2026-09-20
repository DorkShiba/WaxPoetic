using UnityEngine;
using System;

public class Util
{
    public static GameObject FindChild(GameObject go, string name, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform != null)
            return transform.gameObject;
        return null;
    }

    public static T FindChild<T>(GameObject go, string name, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        Transform[] transforms = go.GetComponentsInChildren<Transform>(recursive);

        foreach (Transform transform in transforms)
        {
            if (transform.name == name)
            {
                T component = transform.GetComponent<T>();
                if (component != null)
                    return component;
            }
        }

        return null;
    }
}