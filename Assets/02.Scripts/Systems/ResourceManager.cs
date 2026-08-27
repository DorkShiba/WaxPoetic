using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Object = UnityEngine.Object;

public class ResourceManager
{
    public T Load<T>(string path) where T : Object
    {
        return Resources.Load<T>(path);
    }

    public GameObject Instantiate(string path, Vector3 position = default, Quaternion rotation = default)
    {
        GameObject prefab = Load<GameObject>($"Prefabs/{path}");
        if (prefab == null)
        {
            Debug.LogError($"Prefab at path {path} not found.");
            return null;
        }

        return Object.Instantiate(prefab, position, rotation);
    }

    public GameObject Instantiate(GameObject prefab, Vector3 position = default, Quaternion rotation = default)
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab is null.");
            return null;
        }

        return Object.Instantiate(prefab, position, rotation);
    }

    public void Destroy(GameObject go, float delay = 0f)
    {
        if (go == null) { return; }

        Object.Destroy(go, delay);
    }

    public void Destroy(Transform transform, float delay = 0f)
    {
        if (transform == null) { return; }
        GameObject go = transform.gameObject;
        Destroy(go, delay);
    }
}
