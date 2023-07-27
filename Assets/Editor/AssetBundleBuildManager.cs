using UnityEditor;
using System.IO;
using Firebase.Storage;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using Firebase.Extensions;

public class AssetBundleBuildManager
{
    [MenuItem("CustomTool/AssetBundle Build")]
    static async void AssetBundleBuild()
    {
        string assetBunbleDirectoty = Path.Combine(Application.dataPath,"AssetBundle");
        if (!Directory.Exists(assetBunbleDirectoty))
            Directory.CreateDirectory(assetBunbleDirectoty);

        BuildPipeline.BuildAssetBundles(assetBunbleDirectoty, BuildAssetBundleOptions.None, BuildTarget.Android);
     
        List<Task> tasks = new List<Task>();

        string firebaseStorageURL = "gs://projectss-c99e7.appspot.com";
        var storageReference = FirebaseStorage.DefaultInstance.GetReferenceFromUrl(firebaseStorageURL);
        
        DirectoryInfo directoryInfo = new DirectoryInfo(assetBunbleDirectoty);
        var fileList = directoryInfo.GetFiles();
        foreach (FileInfo file in fileList)
        {
            if (file.Name.Contains('.')) continue;  // 에셋번들만 업로드 되도록 해줌 (meta, mainfest 제외)

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

        EditorUtility.DisplayDialog("에셋 번들 빌드", "에셋 번들 빌드 완료", "완료");
    }
}
