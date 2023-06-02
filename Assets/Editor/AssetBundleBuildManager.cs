using UnityEditor;
using System.IO;

public class AssetBundleBuildManager
{
    [MenuItem("CustomTool/AssetBundle Build")]
    static void AssetBundleBuild()
    {
        //에셋 번들로 생성된 오브젝트는 사용 후 언로드(메모리에세 해제) 하는 것이 필요함
        string assetBunbleDirectoty = "Assets/AssetBundle";

        if(!Directory.Exists(assetBunbleDirectoty))
        {
            Directory.CreateDirectory(assetBunbleDirectoty);
        }

        BuildPipeline.BuildAssetBundles(assetBunbleDirectoty, BuildAssetBundleOptions.None, BuildTarget.Android);
    }
}
