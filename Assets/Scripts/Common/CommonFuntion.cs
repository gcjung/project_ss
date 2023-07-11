using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CommonFuntion : MonoBehaviour
{
    static Dictionary<string, GameObject> prefabsPool = new Dictionary<string, GameObject>();
    private void Awake()
    {
        prefabsPool.Clear();
    }

    public static void LoadPrefab(string prefabName)
    {
        if (!prefabsPool.ContainsKey(prefabName))
        {
            GameObject newObj = Resources.Load<GameObject>(prefabName);
            prefabsPool.Add(prefabName, newObj);
        }
        else if (prefabsPool[prefabName] == null)
        {
            GameObject newObj = Resources.Load<GameObject>(prefabName);
            prefabsPool[prefabName] = newObj;
        }
    }

    public static GameObject GetPrefab(string prefabName, Transform parentTransform)
    {
        LoadPrefab(prefabName);

        return GetPrefab(prefabsPool[prefabName], parentTransform);
    }

    public static GameObject GetPrefab(GameObject prefab, Transform parentTransform)
    {
        GameObject obj = Instantiate(prefab);

        obj.transform.SetParent(parentTransform, false);

        return obj;
    }
}
