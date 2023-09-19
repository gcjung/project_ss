using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class CommonFunction : MonoBehaviour
{
    static Dictionary<string, Object> prefabPool = new Dictionary<string, Object>();
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
        if (!prefabPool.ContainsKey(prefabName))
        {
            //GameObject newObj = Resources.Load<GameObject>(prefabName);
            GameObject newObj = ResourceLoader.LoadPrefab(prefabName);
            prefabPool.Add(prefabName, newObj);
        }
        else if (prefabPool[prefabName] == null)
        {
            //GameObject newObj = Resources.Load<GameObject>(prefabName);
            GameObject newObj = ResourceLoader.LoadPrefab(prefabName);
            prefabPool[prefabName] = newObj;
        }
    }

    public static GameObject CreatePrefab(Object prefab, Transform parentTransform)
    {
        GameObject obj = Instantiate(prefab) as GameObject;
        obj.transform.SetParent(parentTransform, false);

        if (obj.GetComponent<RectTransform>())
        {
            Util.SetFontInChildrenText(obj.transform);
            Util.SetRawImageTexture(obj.transform);
        }
        else
        {
            Util.ReLinkShader(obj);
        }

        return obj;
    }
    public static GameObject GetPrefabInstance(string prefabName, Transform parentTransform)    // 자체 Instantiate 해줌
    {
        LoadPrefab(prefabName);
        //LoadObj(prefabsPool, prefabName);

        return CreatePrefab(prefabPool[prefabName], parentTransform);
    }

    public static GameObject GetPrefab(string prefabName)
    {
        LoadPrefab(prefabName);
        
        GameObject obj = prefabPool[prefabName] as GameObject;

        if (obj.GetComponent<RectTransform>())
        {
            Util.SetFontInChildrenText(obj.transform);
            Util.SetRawImageTexture(obj.transform);
        }
        else
        {
            Util.ReLinkShader(obj);
        }

        return obj;
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

    public static void CreateNotification(string contents,Transform transform)
    {
        float endValue = 1500.0f;
        float duration = 1.0f;

        GameObject obj = TmpObjectPool.Instance.GetPoolObject("Notification_Text", transform);  //오브젝트풀에서 가져오기

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.localPosition = Vector3.zero;  //위치 초기화

        TextMeshProUGUI tmp = obj.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        tmp.text = contents;

        rect.DOAnchorPosY(endValue, duration).SetEase(Ease.OutExpo).OnComplete(() => TmpObjectPool.Instance.ReturnToPool(obj)); //목표 위치까지 이동 후 오브젝트 풀에 반환
    }
}

