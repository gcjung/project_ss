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
    [MenuItem("CustomTool/AssetBundle Build")]
    static async void AssetBundleBuild()
    {
        string assetBunbleDirectoty = Path.Combine(Application.dataPath,"AssetBundle");
        if (!Directory.Exists(assetBunbleDirectoty))
            Directory.CreateDirectory(assetBunbleDirectoty);

        BuildPipeline.BuildAssetBundles(assetBunbleDirectoty, BuildAssetBundleOptions.None, BuildTarget.Android);

        DocumentReference dataRef = FirebaseFirestore.DefaultInstance.Collection("GameData").Document("Data");


        int assetBundleVersion;
        var t = await dataRef.GetSnapshotAsync();
        if (t.Exists)
        {
            var dic = t.ToDictionary();
            if(dic.ContainsKey(GameDataType.AssetBundleVersion.ToString()))
            {
                assetBundleVersion = Convert.ToInt32(dic[GameDataType.AssetBundleVersion.ToString()]);
                //assetBundleVersion = int.Parse(dic[GameDataType.AssetBundleVersion.ToString()]);
            }
        }
        else
        {

        }





        List<Task> tasks = new List<Task>();

        string firebaseStorageURL = "gs://projectss-c99e7.appspot.com";
        var storageReference = FirebaseStorage.DefaultInstance.GetReferenceFromUrl(firebaseStorageURL);
        
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

        EditorUtility.DisplayDialog("���� ���� ����", "���� ���� ���� �Ϸ�", "�Ϸ�");
    }
}
