using Systems;
using UnityEngine;

public class PoolingTest: MonoBehaviour
{
    void Start()
    {
        Test();
    }

    public void Test()
    {
        GameObject prefab = Managers.Resource.Load<GameObject>("Prefabs/Monsters/M001");
        Managers.Pooling.Pop(prefab);

        for (int i = 0; i < 15; i++)
        {
            GameObject obj = Managers.Pooling.Pop(prefab).gameObject;
            Debug.Log($"Object {i + 1}: {obj.name}, Active: {obj.activeSelf}");
        }
    }
}