using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class LoadAssetBundle : MonoBehaviour
{
    //�ϴ� �Ѿ� ��ũ��
    private readonly string googleDrive = "https://drive.google.com/uc?export=download&id=1iKvwHuOcqKlZ8xtyPJf-c8sB7VUrorZr";
    private string AtlasURL = "https://docs.google.com/uc?export=download&id=1jn5AJZsMzv1hyoIv4nOjGoa-kBvrq4GT&confirm=t";
    //���� ����� �ҷ��� ������Ʈ �̸�
    public string AssetName { get; set; } = "(TEST)projectile";

    private void Start()
    {
        GetAssetBumdle(googleDrive);
        //GetAssetBumdle(AtlasURL);
    }

    public void GetAssetBumdle(string url)
    {
        StartCoroutine(GetAssetBundleFromGoogleDrive(url));
    }
    private IEnumerator GetAssetBundleFromGoogleDrive(string url)
    {
        Debug.Log("���鰡������ " + Time.time);
        //���� ���� ��������
        UnityWebRequest unityWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(url);

        //������ �� ���� ���
        yield return unityWebRequest.SendWebRequest();
        
        //������ ���ٸ�
        if(unityWebRequest.error == null)
        {
            AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(unityWebRequest); 
            GameObject go = assetBundle.LoadAsset<GameObject>(AssetName);
            Instantiate(go, Vector2.zero, Quaternion.Euler(0,0,0));
            Debug.Log(AssetName);
            
        }
        //���� �߻�
        else
        {
            Debug.Log($"���� ���� �ε� ���� : {unityWebRequest.error}");
        }
        Debug.Log("���鰡������ �� " + Time.time);
    }

}
