using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour
{
    private List<T> pool = new List<T>();
    private T prefab;
    private Transform trParent;

    public Transform TrPool { get; set; }

    public ObjectPool(T prefab, int count, Transform parent)
    {
        this.prefab = prefab;
        this.trParent = parent;

        TrPool = new GameObject().transform;
        TrPool.name = $"{prefab.name}Pool";
        TrPool.gameObject.SetParent(this.trParent);

        for (int i = 0; i < count; i++)
        {
            AddPool(CreateNewObject());
        }
    }

    private T CreateNewObject()
    {
        T obj = Object.Instantiate(prefab);
        obj.gameObject.SetActive(false);
        obj.gameObject.SetParent(TrPool);
        return obj;
    }

    private void AddPool(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Add(obj);
    }

    public T GetObjectPool()
    {
        T obj = null;

        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].gameObject.activeSelf)
            {
                obj = pool[i];
                break;
            }
        }

        if (obj == null)
        {
            Debug.Log("사용할 수 있는 오브젝트가 없음.");
            //새로 만들어야 하는데 어케 처리할지 고민
        }
        obj.gameObject.SetActive(true);
        obj.gameObject.SetParent(TrPool);
        return obj;
    }

    public void ReturnObject(T obj)
    {
        obj.gameObject.SetActive(false);
    }
}
