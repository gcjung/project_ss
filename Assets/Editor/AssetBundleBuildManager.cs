using UnityEditor;
using System.IO;

public class AssetBundleBuildManager
{
    [MenuItem("CustomTool/AssetBundle Build")]
    static void AssetBundleBuild()
    {
        //���� ����� ������ ������Ʈ�� ��� �� ��ε�(�޸� ����) �ϴ� ���� �ʿ���
        string assetBunbleDirectoty = "Assets/AssetBundle";

        if(!Directory.Exists(assetBunbleDirectoty))
        {
            Directory.CreateDirectory(assetBunbleDirectoty);
        }

        BuildPipeline.BuildAssetBundles(assetBunbleDirectoty, BuildAssetBundleOptions.None, BuildTarget.Android);

        EditorUtility.DisplayDialog("���� ���� ����", "���� ���� ���� �Ϸ�", "�Ϸ�");
    }
}
