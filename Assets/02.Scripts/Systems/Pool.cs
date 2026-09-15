using UnityEngine;
using UnityEngine.Pool;

// 참고
// https://deff-dev.tistory.com/121

namespace Systems
{
    public class Pool
    {
        private ObjectPool<GameObject> pool;
        private const int maxSize = 100;
        private int initSize;

        private GameObject prefab;

        public void Init(GameObject prefab, int initSize = 20)
        {
            this.initSize = initSize;
            this.prefab = prefab;
            pool = new ObjectPool<GameObject>(CreateObject, ActivatePoolObject, DisablePoolObject, DestroyPoolObject, false, initSize, maxSize);
        }

        private GameObject CreateObject() // 오브젝트 생성
        {
            return Managers.Resource.Instantiate(prefab);
        }

        private void ActivatePoolObject(GameObject obj) // 오브젝트 활성화
        {
            obj.SetActive(true);
            Poolable poolable = obj.GetComponent<Poolable>();
            if (poolable != null)
            {
                poolable.OnActivate();
            }
        }

        private void DisablePoolObject(GameObject obj) // 오브젝트 비활성화
        {
            obj.SetActive(false);
        }

        private void DestroyPoolObject(GameObject obj) // 오브젝트 삭제
        {
            Managers.Resource.Destroy(obj);
        }
        
        public GameObject GetObject()
        {
            GameObject ret = null;

            if (pool.CountActive >= maxSize) // maxSize를 넘는다면 임시 객체 생성 및 반환
            {
                ret = CreateObject();
                ret.tag = "PoolOverObj";
            }
            else 
            {
                ret = pool.Get();
            }

            return ret;
        }

        public void ReleaseObject(GameObject obj)
        {
            if (obj.CompareTag("PoolOverObj"))
            {
                Managers.Resource.Destroy(obj);
            }
            else
            {
                pool.Release(obj);
            }
        }
    }
}