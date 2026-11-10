using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Systems
{
    public class PoolingManager {

        Dictionary<GameObject, Pool> dict_pool = new Dictionary<GameObject, Pool>();
        Transform _root;

        internal void Init() {
            if (_root == null) {
                _root = new GameObject { name = "@Pool_Root" }.transform;
                Object.DontDestroyOnLoad(_root);
            }
        }

        public void CreatePool(GameObject prefab, int initSize = 20) {
            Pool pool = new Pool();
            pool.Init(prefab, initSize);

            dict_pool.Add(prefab, pool);
        }

        public void Push(GameObject instance) {
            if (instance == null) return;

            Poolable poolable = instance.GetComponent<Poolable>();
            Pool pool = poolable.Owner;
            if (dict_pool.ContainsValue(pool) == false) {
                // 풀이 없으면 그냥 제거
                Managers.Resource.Destroy(instance);
                return;
            }

            pool.Release(instance);
        }

        public Poolable Pop(GameObject prefab, Transform parent = null) {
            if (dict_pool.ContainsKey(prefab) == false)
                // 풀이 없으면 만들어서 오브젝트를 뽑아냄
                CreatePool(prefab);

            GameObject obj = dict_pool[prefab].GetObject();
            if (parent != null)
                obj.transform.SetParent(parent);

            Poolable poolable = obj.GetComponent<Poolable>();
            if (poolable == null)
            {
                poolable = obj.AddComponent<Poolable>();
            }
            return poolable;
        }

        public bool Contains(GameObject prefab) {
            return dict_pool.ContainsKey(prefab);
        }

        public void Clear() {
            foreach (Transform child in _root)
                Managers.Resource.Destroy(child.gameObject);

            dict_pool.Clear();
        }
    }
}