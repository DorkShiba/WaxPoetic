using UnityEngine;
using System;
using System.Diagnostics.Tracing;

namespace Systems
{
    public class Poolable: MonoBehaviour
    {
        private Pool owner;
        public Pool Owner {  get { return owner; } }

        public void Init(Pool pool)
        {
            owner = pool;
        }

        public void OnActivate()
        {
            gameObject.transform.SetParent(null);
            gameObject.transform.localScale = Vector3.one;
            gameObject.transform.localPosition = Vector3.zero;
        }

        public void Deactivate()
        {

        }
    }
}