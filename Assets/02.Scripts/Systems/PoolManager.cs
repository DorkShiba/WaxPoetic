using System.Collections.Generic;
using UnityEngine;

namespace Systems
{
    public class PoolManager {
        #region Pool
        class Pool {
            public GameObject Original { get; private set; }
            public Transform Root { get; set; }

            Stack<Poolable> _poolStack = new Stack<Poolable>();

            public void Init(GameObject original, int count = 6) {
                Original = original;
                Root = new GameObject().transform;
                Root.name = $"{original.name}_Root";

                for (int i = 0; i < count; i++)
                    Push(Create());
            }

            Poolable Create() {
                GameObject go = Managers.Resource.Instantiate(Original);
                go.name = Original.name;
                Poolable poolable = go.GetComponent<Poolable>();
                if (poolable == null)
                    poolable = go.AddComponent<Poolable>();
                return poolable;
            }

            public void Push(Poolable poolable) {
                if (poolable == null)
                    return;

                poolable.transform.parent = Root;
                poolable.gameObject.SetActive(false);

                _poolStack.Push(poolable);
            }

            public Poolable Pop(Transform parent) {
                Poolable poolable;

                if (_poolStack.Count > 0)
                    poolable = _poolStack.Pop();
                else
                    poolable = Create();

                poolable.gameObject.SetActive(true);

                if (parent == null)
                    poolable.transform.parent = null;

                poolable.transform.parent = parent;

                return poolable;
            }
        }
        #endregion

        Dictionary<string, Pool> _pool = new Dictionary<string, Pool>();
        Transform _root;

        public void Init() {
            if (_root == null) {
                _root = new GameObject { name = "@Pool_Root" }.transform;
                Object.DontDestroyOnLoad(_root);
            }
        }

        public void CreatePool(GameObject original, int count = 6) {
            Pool pool = new Pool();
            pool.Init(original, count);
            pool.Root.parent = _root;

            _pool.Add(original.name, pool);
        }

        public void Push(Poolable poolable) {
            string name = poolable.gameObject.name;
            if (_pool.ContainsKey(name) == false) {
                // 풀이 없으면 그냥 제거
                Managers.Resource.Destroy(poolable.gameObject);
                return;
            }

            _pool[name].Push(poolable);
        }

        public Poolable Pop(GameObject original, Transform parent = null) {
            if (_pool.ContainsKey(original.name) == false)
                // 풀이 없으면 만들어서 오브젝트를 뽑아냄
                CreatePool(original);

            return _pool[original.name].Pop(parent);
        }

        public GameObject GetOriginal(string name) {
            if (_pool.ContainsKey(name) == false)
                return null;
            return _pool[name].Original;
        }

        public bool Contains(string name) {
            return _pool.ContainsKey(name);
        }

        public void Clear() {
            foreach (Transform child in _root)
                Managers.Resource.Destroy(child.gameObject);

            _pool.Clear();
        }
    }
}