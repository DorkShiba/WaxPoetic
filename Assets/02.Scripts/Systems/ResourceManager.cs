using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems
{
    public class ResourceManager
    {
        public T Load<T>(string path) where T : Object
        {
            return Resources.Load<T>(path);
        }

        public GameObject Instantiate(string path, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
        {
            GameObject prefab = Load<GameObject>($"Prefabs/{path}");
            if (prefab == null)
            {
                Debug.LogError($"Prefab at path {path} not found.");
                return null;
            }

            if (Managers.Pool.Contains(prefab.name))
            {
                return Managers.Pool.Pop(prefab, parent).gameObject;
            }

            return Object.Instantiate(prefab, position, rotation, parent);
        }

        public GameObject Instantiate(GameObject prefab, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
        {
            if (prefab == null)
            {
                Debug.LogError("Prefab is null.");
                return null;
            }

            if (Managers.Pool.Contains(prefab.name))
            {
                return Managers.Pool.Pop(prefab, parent).gameObject;
            }

            return Object.Instantiate(prefab, position, rotation, parent);
        }

        public void Destroy(GameObject go, float delay = 0f)
        {
            if (go == null) { return; }

            if (Managers.Pool.Contains(go.name))
            {
                Managers.Pool.Push(go.GetComponent<Poolable>());
                return;
            }

            Object.Destroy(go, delay);
        }

        public void Destroy(Transform transform, float delay = 0f)
        {
            if (transform == null) { return; }

            if (Managers.Pool.Contains(transform.gameObject.name))
            {
                
                Managers.Pool.Push(transform.GetComponent<Poolable>());
                return;
            }

            GameObject go = transform.gameObject;
            Destroy(go, delay);
        }
    }
}
