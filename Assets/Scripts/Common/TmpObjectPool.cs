using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TmpObjectPool : MonoBehaviour
{
    private static TmpObjectPool instance;
    public static TmpObjectPool Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    //private Dictionary<string, GameObject> prefabDic;
    private Dictionary<string, Queue<GameObject>> objectPool;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        //prefabDic = new Dictionary<string, GameObject>();
        objectPool = new Dictionary<string, Queue<GameObject>>();
    }
    //public void AddPrefabDic(GameObject prefab)
    //{
    //    if (!prefabDic.ContainsKey(prefab.name))
    //    {
    //        prefabDic.Add(prefab.name, prefab);
    //    }
    //}

    //public GameObject GetGameObject(string prefabName)
    //{
    //    if (prefabDic.ContainsKey(prefabName))
    //    {
    //        return prefabDic[prefabName];
    //    }
    //    return null;
    //}

    public void CreatePool(string prefabName, int poolSize)
    {
        GameObject prefab = CommonFunction.GetPrefab(prefabName);

        if (!objectPool.ContainsKey(prefab.name))
        {
            objectPool[prefab.name] = new Queue<GameObject>();
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.name = prefab.name;
            obj.SetActive(false);
            objectPool[prefab.name].Enqueue(obj);
        }
    }

    public GameObject GetPoolObject(string prefabName, Transform tf = null)
    {
        if (tf == null) tf = transform;

        if (objectPool.ContainsKey(prefabName))
        {
            Queue<GameObject> pool = objectPool[prefabName];

            foreach (GameObject obj in pool)
            {
                if (!obj.activeSelf)
                {
                    obj.transform.SetParent(tf);
                    obj.SetActive(true);

                    return objectPool[prefabName].Dequeue();
                }
            }

            GameObject newObj = Instantiate(CommonFunction.GetPrefab(prefabName), tf); // pool에 사용가능한 obj가 없을 때
            newObj.name = prefabName;
            newObj.SetActive(true);

            return newObj;
        }
        else                        // 생성해놓은 pool이 없을 때
        {
            GameObject newObj = Instantiate(CommonFunction.GetPrefab(prefabName), tf);
            newObj.name = prefabName;
            newObj.SetActive(true);

            return newObj;
        }
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.transform.SetParent(transform);
        obj.SetActive(false);
        objectPool[obj.name].Enqueue(obj);
    }
}
