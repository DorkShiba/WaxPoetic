using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Systems
{
    public class PoolManager {

        Dictionary<string, Pool> dict_pool = new Dictionary<string, Pool>();
        Transform _root;

        public void Init() {
            if (_root == null) {
                _root = new GameObject { name = "@Pool_Root" }.transform;
                Object.DontDestroyOnLoad(_root);
            }
        }

        public void CreatePool(GameObject original, int initSize = 20) {
            Pool pool = new Pool();
            pool.Init(original, initSize);

            dict_pool.Add(original.name, pool);
        }

        public void Push(GameObject original) {
            string name = original.name;
            if (dict_pool.ContainsKey(name) == false) {
                // 풀이 없으면 그냥 제거
                Managers.Resource.Destroy(original);
                return;
            }

            dict_pool[name].ReleaseObject(original);
        }

        public Poolable Pop(GameObject original, Transform parent = null) {
            if (dict_pool.ContainsKey(original.name) == false)
                // 풀이 없으면 만들어서 오브젝트를 뽑아냄
                CreatePool(original);

            GameObject obj = dict_pool[original.name].GetObject();
            if (parent != null)
                obj.transform.SetParent(parent);

            return obj.GetComponent<Poolable>();
        }

        public bool Contains(string name) {
            return dict_pool.ContainsKey(name);
        }

        public void Clear() {
            foreach (Transform child in _root)
                Managers.Resource.Destroy(child.gameObject);

            dict_pool.Clear();
        }
    }
}