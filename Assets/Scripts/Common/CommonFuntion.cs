using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using TMPro;
using System.Runtime.CompilerServices;

public class CommonFuntion : MonoBehaviour
{
    static Dictionary<string, Object> prefabsPool = new Dictionary<string, Object>();
    static Dictionary<string, Object> atlasPool = new Dictionary<string, Object>();
    static Dictionary<string, Object> fontPool = new Dictionary<string, Object>();

    //public static void LoadObj(Dictionary<string, Object> dic, string prefabName)
    //{
    //    if (!dic.ContainsKey(prefabName))
    //    {
    //        //GameObject newObj = Resources.Load<GameObject>(prefabName);
    //        GameObject newObj = ResourceLoader.LoadUiPrefab(prefabName);
    //        dic.Add(prefabName, newObj);
    //    }
    //    else if (dic[prefabName] == null)
    //    {
    //        //GameObject newObj = Resources.Load<GameObject>(prefabName);
    //        GameObject newObj = ResourceLoader.LoadUiPrefab(prefabName);
    //        dic[prefabName] = newObj;
    //    }
    //}
    
    public static void LoadPrefab(string prefabName)
    {
        if (!prefabsPool.ContainsKey(prefabName))
        {
            //GameObject newObj = Resources.Load<GameObject>(prefabName);
            GameObject newObj = ResourceLoader.LoadUiPrefab(prefabName);
            prefabsPool.Add(prefabName, newObj);
        }
        else if (prefabsPool[prefabName] == null)
        {
            //GameObject newObj = Resources.Load<GameObject>(prefabName);
            GameObject newObj = ResourceLoader.LoadUiPrefab(prefabName);
            prefabsPool[prefabName] = newObj;
        }

    }

    public static GameObject GetPrefab(Object prefab, Transform parentTransform, string fontName ="")
    {
        GameObject obj = Instantiate(prefab) as GameObject;
        obj.transform.SetParent(parentTransform, false);

        if (string.IsNullOrEmpty(fontName))
            fontName = "Font/KimjungchulGothic-Regular SDF";
        
        var font = GetFont(fontName);
        Util.SetFontInChildrenText(obj.transform, font);

        return obj;
    }
    public static GameObject GetPrefab(string prefabName, Transform parentTransform)
    {
        LoadPrefab(prefabName);
        //LoadObj(prefabsPool, prefabName);

        return GetPrefab(prefabsPool[prefabName], parentTransform);
    }

    public static void LoadFont(string prefabName)
    {
        if (!fontPool.ContainsKey(prefabName))
        {
            //GameObject newObj = Resources.Load<GameObject>(prefabName);
            TMP_FontAsset newObj = Resources.Load<TMP_FontAsset>(prefabName);
            fontPool.Add(prefabName, newObj);
        }
        else if (fontPool[prefabName] == null)
        {
            //GameObject newObj = Resources.Load<GameObject>(prefabName);
            TMP_FontAsset newObj = Resources.Load<TMP_FontAsset>(prefabName);
            fontPool[prefabName] = newObj;
        }
    }

    public static TMP_FontAsset GetFont(string prefabName)
    {
        //LoadPrefab(prefabName);
        LoadFont(prefabName);

        return fontPool[prefabName] as TMP_FontAsset;
    }


    // 아틀라스
    public static void LoadAtlas(string prefabName)
    {
        if (!atlasPool.ContainsKey(prefabName))
        {
            SpriteAtlas newObj = ResourceLoader.LoadAtlas(prefabName);
            atlasPool.Add(prefabName, newObj);
        }
        else if (atlasPool[prefabName] == null)
        {
            SpriteAtlas newObj = ResourceLoader.LoadAtlas(prefabName);
            atlasPool[prefabName] = newObj;
        }
    }

    public static Sprite GetSprite_Atlas(string imageName, string atlasName)
    {
        LoadAtlas(atlasName);
        //LoadObj(atlasPool, imageName);

        SpriteAtlas atlas = atlasPool[atlasName] as SpriteAtlas;

        if (atlas)
            return atlas.GetSprite(imageName);
        else
            return null;
    }


}

