using UnityEditor;
using System.IO;
using Firebase.Storage;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Firestore;
using System;


public class AssetBundleBuildManager
{
    [MenuItem("CustomTool/Build AssetBundle")]
    static void AssetBundleBuild()
    {
        string assetBunbleDirectoty = Application.persistentDataPath;
        //string assetBunbleDirectoty = Path.Combine(Application.dataPath,"AssetBundle");

        if (!Directory.Exists(assetBunbleDirectoty))
            Directory.CreateDirectory(assetBunbleDirectoty);

        BuildPipeline.BuildAssetBundles(assetBunbleDirectoty, BuildAssetBundleOptions.None, BuildTarget.Android);

        EditorUtility.DisplayDialog("���� ���� ����", "���� ���� ���� �Ϸ�", "�Ϸ�");
    }

    [MenuItem("CustomTool/UpLoad AssetBundle")]
    static async void UpLoadBundleBuild()
    {
        string assetBunbleDirectoty = Application.persistentDataPath;
        //string assetBunbleDirectoty = Path.Combine(Application.dataPath, "AssetBundle");

        string firebaseStorageURL = "gs://projectss-c99e7.appspot.com";
        var storageReference = FirebaseStorage.DefaultInstance.GetReferenceFromUrl(firebaseStorageURL);

        UpdateAssetBundleVersion();

        List<Task> tasks = new List<Task>();
        DirectoryInfo directoryInfo = new DirectoryInfo(assetBunbleDirectoty);
        var fileList = directoryInfo.GetFiles();
        foreach (FileInfo file in fileList)
        {
            if (file.Name.Contains('.')) continue;  // ���¹��鸸 ���ε� �ǵ��� ���� (meta, mainfest ����)

            StorageReference uploadRef = storageReference.Child("AssetBundle/" + file.Name);
            tasks.Add(uploadRef.PutFileAsync(file.FullName).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log(task.Exception.ToString());
                }
                else
                {
                    StorageMetadata metadata = task.Result;
                    Debug.Log($"{file.Name}, path : {metadata.Path}, uploading Finished");
                }
            }));
        }

        await Task.WhenAll(tasks);

        EditorUtility.DisplayDialog("���� ���� ���ε�", "���� ���� ���ε� �Ϸ�", "�Ϸ�");
    }

    static async void UpdateAssetBundleVersion()
    {
        DocumentReference dataRef = FirebaseFirestore.DefaultInstance.Collection("GameData").Document("Data");
        var t = await dataRef.GetSnapshotAsync();
        if (t.Exists)
        {
            var dic = t.ToDictionary();
            if (dic.ContainsKey(GameDataType.AssetBundleVersion.ToString()))
            {
                int assetBundleVersion = Convert.ToInt32(dic[GameDataType.AssetBundleVersion.ToString()]);
                await dataRef.UpdateAsync(GameDataType.AssetBundleVersion.ToString(), assetBundleVersion + 1);
            }
            else
                await dataRef.UpdateAsync(GameDataType.AssetBundleVersion.ToString(), 1);
        }
        else
        {
            await dataRef.UpdateAsync(GameDataType.AssetBundleVersion.ToString(), 1);
        }
    }
}
