using UnityEngine;
using System;

namespace Systems
{
    public class Poolable: MonoBehaviour
    {

        public void Init()
        {
            
        }

        public void OnActivate()
        {
            gameObject.transform.SetParent(null);
            gameObject.transform.localScale = Vector3.one;
            gameObject.transform.localPosition = Vector3.zero;
        }
    }
}