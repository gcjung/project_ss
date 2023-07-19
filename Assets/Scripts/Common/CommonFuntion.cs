using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements;

public class CommonFuntion : MonoBehaviour
{
    static Dictionary<string, Object> prefabsPool = new Dictionary<string, Object>();
    static Dictionary<string, Object> atlasPool = new Dictionary<string, Object>();

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

    public static GameObject GetPrefab(Object prefab, Transform parentTransform)
    {
        GameObject obj = Instantiate(prefab) as GameObject;

        obj.transform.SetParent(parentTransform, false);

        return obj;
    }
    public static GameObject GetPrefab(string prefabName, Transform parentTransform)
    {
        LoadPrefab(prefabName);

        return GetPrefab(prefabsPool[prefabName], parentTransform);
    }

    public static void LoadAtlas(string prefabName)
    {
        string path = "Atlas/";
        if (!atlasPool.ContainsKey(prefabName))
        {
            SpriteAtlas newObj = Resources.Load<SpriteAtlas>(path + prefabName);
            atlasPool.Add(prefabName, newObj);
        }
        else if (atlasPool[prefabName] == null)
        {
            SpriteAtlas newObj = Resources.Load<SpriteAtlas>(path + prefabName);
            atlasPool[prefabName] = newObj;
        }
    }

    public static Sprite GetSprite_Atlas(string imageName, string atlasName)
    {
        LoadAtlas(atlasName);
        
        SpriteAtlas atlas = atlasPool[atlasName] as SpriteAtlas;

        if (atlas)
            return atlas.GetSprite(imageName);
        else
            return null;
    }


}

//atlasPool.Add(atlasName, null);
//atlasPool[atlasName] = ResourceLoader.Load_Atlas(atlasName);