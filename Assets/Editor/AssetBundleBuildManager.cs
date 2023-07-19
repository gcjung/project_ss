using UnityEditor;
using System.IO;

public class AssetBundleBuildManager
{
    [MenuItem("CustomTool/AssetBundle Build")]
    static void AssetBundleBuild()
    {
        //에셋 번들로 생성된 오브젝트는 사용 후 언로드(메모리 해제) 하는 것이 필요함
        string assetBunbleDirectoty = "Assets/AssetBundle";

        if(!Directory.Exists(assetBunbleDirectoty))
        {
            Directory.CreateDirectory(assetBunbleDirectoty);
        }

        BuildPipeline.BuildAssetBundles(assetBunbleDirectoty, BuildAssetBundleOptions.None, BuildTarget.Android);

        EditorUtility.DisplayDialog("에셋 번들 빌드", "에셋 번들 빌드 완료", "완료");
    }
}
