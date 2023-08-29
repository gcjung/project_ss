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
using System.Linq;
using System.IO;

public class ResourceLoader : MonoBehaviour
{
    private static ResourceLoader instance;
    public static ResourceLoader Instance
    {
        get
        {
            if (instance == null)
                return null;

            return instance;
        }
    }

    public static AssetBundle[] assetBundleArr;
    long[] downloadedResourceSize;
    string firebaseStorageURL = "gs://projectss-c99e7.appspot.com";
    StorageReference storageReference;

    private void Awake()
    {
        if (instance == null) instance = this;

        assetBundleArr = new AssetBundle[Enum.GetValues(typeof(BundleType)).Length];
        downloadedResourceSize = new long[Enum.GetValues(typeof(BundleType)).Length];
        storageReference = FirebaseStorage.DefaultInstance.GetReferenceFromUrl(firebaseStorageURL);
    }
    public enum BundleType
    {
        atlas = 0,
        prefab,
    }


    public static SpriteAtlas LoadAtlas(string atlasName)
    {
        SpriteAtlas obj = assetBundleArr[(int)BundleType.atlas].LoadAsset(atlasName) as SpriteAtlas;

        return obj;
    }
    public static GameObject LoadPrefab(string prefabName)
    {
        GameObject obj = assetBundleArr[(int)BundleType.prefab].LoadAsset(prefabName) as GameObject;
        //Debug.Log($"UI ���ºҷ����� : {obj.name}");
        return obj;
    }
    //public static GameObject LoadCharPrefab(string prefabName)
    //{
    //    GameObject obj = assetBundleArr[(int)BundleType.charprefab].LoadAsset(prefabName) as GameObject;
    //    //Debug.Log($"UI ���ºҷ����� : {obj.name}");
    //    return obj;
    //}


    public void LoadAllAssetBundle(bool loadFromServer)
    {
        if (loadFromServer)
        {
            foreach (BundleType type in Enum.GetValues(typeof(BundleType)))
            {
                GetUrlFromFirebaseStorage(type);
            }
        }
        else
        {
            foreach (BundleType type in Enum.GetValues(typeof(BundleType)))
            {
                LoadLocalAssetBundle(type);
            }

            downloadComplete = true;
        }
    }
    public void LoadLocalAssetBundle(BundleType bundleType)
    {
        string localPath = Application.persistentDataPath;
        if (File.Exists(Path.Combine(localPath,bundleType.ToString()))) // ���ÿ� ���� �ٿ�ε�
        {
            assetBundleArr[(int)bundleType] ??= AssetBundle.LoadFromFile(localPath + "/" + bundleType);
        }
        else
        {
            if (!Directory.Exists(localPath)) //������ �������� ������
            {
                Directory.CreateDirectory(localPath); //���� ����
            }

            // ����â ����
        }
    }


    long totalResourceSize = 0;
    public async Task<long> GetTotalResourceSize()
    {
        List<Task> taskList = new List<Task>(Enum.GetValues(typeof(BundleType)).Length);
        foreach (BundleType type in Enum.GetValues(typeof(BundleType)))
        {
            string assetBundleDir = "/AssetBundle/" + type.ToString().ToLower();
            var storageref = storageReference.Child(assetBundleDir);

            taskList.Add(storageref.GetMetadataAsync().ContinueWithOnMainThread(task =>
            {
                if (!task.IsFaulted && !task.IsCanceled)
                {
                    totalResourceSize += task.Result.SizeBytes;
                }
                else
                    Debug.Log("����");
            }));
        }
        await Task.WhenAll(taskList);

        return totalResourceSize;
    }
    async void GetUrlFromFirebaseStorage(BundleType bundleType)
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
                Debug.Log($"\"{bundleType}\" ���¹��� ã�� �� ����");
            }
        });

        if (builder != null)
            StartCoroutine(GetAssetBundleFromUrl(bundleType, builder.Uri));
    }

    [HideInInspector]
    public bool downloadComplete = false;
    private IEnumerator GetAssetBundleFromUrl(BundleType bundleType, Uri uri)
    {
        var startTime = Time.time;
        var sc = FindObjectOfType<StartScene>();
        UnityWebRequest unityWebRequest = UnityWebRequest.Get(uri);

        // �ٿ�ε� ����
        unityWebRequest.SendWebRequest();

        while (!unityWebRequest.isDone)
        {
            downloadedResourceSize[(int)bundleType] = (long)unityWebRequest.downloadedBytes;
            sc.SetResourceDownloadSlider(downloadedResourceSize.Sum(x => x), totalResourceSize);

            yield return CommonIEnumerator.WaitForEndOfFrame;
        }

        downloadedResourceSize[(int)bundleType] = (long)unityWebRequest.downloadedBytes;
        sc.SetResourceDownloadSlider(downloadedResourceSize.Sum(x => x), totalResourceSize);

        if (unityWebRequest.error == null)
        {
            if (downloadedResourceSize.Sum(x => x) == totalResourceSize) // ���¹��� �� �ٿ���� ���
            {
                downloadComplete = true;

                int version = GlobalManager.Instance.DBManager.GetGameData<int>(GameDataType.AssetBundleVersion);
                PlayerPrefs.SetInt(GameDataType.AssetBundleVersion.ToString(), version);
            }

            string localPath = Application.persistentDataPath;
            if (!Directory.Exists(localPath)) //������ �������� ������
            {
                Directory.CreateDirectory(localPath); //���� ����
            }

            File.WriteAllBytes(localPath + "/" + bundleType, unityWebRequest.downloadHandler.data); // ������ ���¹����� ���ÿ� ����
            assetBundleArr[(int)bundleType] ??= AssetBundle.LoadFromFile(localPath + "/" + bundleType);
        }
        else
        {
            Debug.Log($"���� ���� �ε� ���� : {unityWebRequest.error}");
        }

        Debug.Log(bundleType + "���鰡������ �� : " + (Time.time - startTime));
        unityWebRequest.Dispose();
    }

}

#region �Ⱦ��� �ڵ�
/*
private IEnumerator GetAssetBundleFromGoogleDrive(string url)
{
    Debug.Log("���鰡������ " + Time.time);
    //���� ���� ��������
    UnityWebRequest unityWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(url);

    //������ �� ���� ���
    yield return unityWebRequest.SendWebRequest();

    //������ ���ٸ�
    if (unityWebRequest.error == null)
    {
        AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(unityWebRequest);
        GameObject go = assetBundle.LoadAsset<GameObject>(AssetName);
        Instantiate(go, Vector2.zero, Quaternion.Euler(0, 0, 0));
        Debug.Log(AssetName);

    }
    //���� �߻�
    else
    {
        Debug.Log($"���� ���� �ε� ���� : {unityWebRequest.error}");
    }
    Debug.Log("���鰡������ �� " + Time.time);
}*/

//if (meta != null)
//{
//    Debug.Log("============================================");
//    Debug.Log($"bucket : {meta.Bucket}");
//    Debug.Log($"Generation : {meta.Generation}");
//    Debug.Log($"path : {meta.Path}");
//    Debug.Log($"Name : {meta.Name}");
//    Debug.Log($"size : {meta.SizeBytes}, ��ȯ : {Util.ConvertBytes(meta.SizeBytes)}");
//    Debug.Log($"CreateTime : {meta.CreationTimeMillis}");
//    Debug.Log($"UpdateTime : {meta.UpdatedTimeMillis}");
//    Debug.Log($"Contents : {meta.ContentType}");
//    Debug.Log($"CacheControl : {meta.CacheControl}");
//    Debug.Log($"ContentDisposition : {meta.ContentDisposition}");
//    //Debug.Log($"ContentDisposition : {meta.AsStr}");
//    Debug.Log("============================================");
//}
#endregion �Ⱦ��� �ڵ�