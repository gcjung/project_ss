using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using UnityEngine.U2D;
using Firebase.Storage;
using Firebase.Extensions;
using System;
using TMPro;

public class ResourceLoader : MonoBehaviour
{


    public static AssetBundle[] assetBundleArr;

    public AssetBundle fontAsset;
    public string firebaseStorageURL = "gs://projectss-c99e7.appspot.com";
    StorageReference storageReference;

    private void Awake()
    {
        assetBundleArr = new AssetBundle[Enum.GetValues(typeof(BundleType)).Length];
        storageReference = FirebaseStorage.DefaultInstance.GetReferenceFromUrl(firebaseStorageURL);
    }
    public enum BundleType
    {
        atlas = 0,
        uiPrefab,
        font,
    }


    public static SpriteAtlas LoadAtlas(string atlasName)
    {
        SpriteAtlas obj = assetBundleArr[(int)BundleType.atlas].LoadAsset(atlasName) as SpriteAtlas;
        return obj;
    }
    public static GameObject LoadUiPrefab(string prefabName)
    {
        GameObject obj = assetBundleArr[(int)BundleType.uiPrefab].LoadAsset(prefabName) as GameObject;
        return obj;
    }
    private void Start()
    {
        foreach (BundleType type in Enum.GetValues(typeof(BundleType)))
        {
            GetUriFromFirebaseStorage(type);
        }
    }
    async void GetUriFromFirebaseStorage(BundleType bundleType)
    {
        UriBuilder builder = null;

        string assetBundleDir = "/AssetBundle/" + bundleType.ToString().ToLower();
        var storageref = storageReference.Child(assetBundleDir);

        await storageref.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                builder = new UriBuilder(task.Result);  // Storage URL 
            }
            else
            {
                Debug.Log($"\"{bundleType}\" 에셋번들 찾을 수 없음");
            }
        });

        if(builder != null)
            StartCoroutine(GetAssetBundleFromUri(bundleType, builder.Uri));
    }

    private IEnumerator GetAssetBundleFromUri(BundleType bundleType, Uri uri)
    {
        Debug.Log(bundleType + "번들가져오기 " + Time.time);
        UnityWebRequest unityWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(uri);
        yield return unityWebRequest.SendWebRequest();

        if (unityWebRequest.error == null)
        {
            assetBundleArr[(int)bundleType] ??= DownloadHandlerAssetBundle.GetContent(unityWebRequest);
        }
        else
        {
            Debug.Log($"에셋 번들 로드 실패 : {unityWebRequest.error}");
        }
        
        Debug.Log(bundleType + "번들가져오기 끝 " + Time.time);
    }
    

}

#region 안쓰는 코드
/*
private IEnumerator GetAssetBundleFromGoogleDrive(string url)
{
    Debug.Log("번들가져오기 " + Time.time);
    //에셋 번들 가져오기
    UnityWebRequest unityWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(url);

    //가져올 때 까지 대기
    yield return unityWebRequest.SendWebRequest();

    //에러가 없다면
    if (unityWebRequest.error == null)
    {
        AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(unityWebRequest);
        GameObject go = assetBundle.LoadAsset<GameObject>(AssetName);
        Instantiate(go, Vector2.zero, Quaternion.Euler(0, 0, 0));
        Debug.Log(AssetName);

    }
    //에러 발생
    else
    {
        Debug.Log($"에셋 번들 로드 실패 : {unityWebRequest.error}");
    }
    Debug.Log("번들가져오기 끝 " + Time.time);
}*/
#endregion 안쓰는 코드