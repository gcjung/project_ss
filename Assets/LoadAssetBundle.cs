using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class LoadAssetBundle : MonoBehaviour
{
    //일단 총알 링크임
    private readonly string googleDrive = "https://drive.google.com/uc?export=download&id=1iKvwHuOcqKlZ8xtyPJf-c8sB7VUrorZr";
    private string AtlasURL = "https://docs.google.com/uc?export=download&id=1jn5AJZsMzv1hyoIv4nOjGoa-kBvrq4GT&confirm=t";
    //에셋 번들로 불러올 오브젝트 이름
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
        Debug.Log("번들가져오기 " + Time.time);
        //에셋 번들 가져오기
        UnityWebRequest unityWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(url);

        //가져올 때 까지 대기
        yield return unityWebRequest.SendWebRequest();
        
        //에러가 없다면
        if(unityWebRequest.error == null)
        {
            AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(unityWebRequest); 
            GameObject go = assetBundle.LoadAsset<GameObject>(AssetName);
            Instantiate(go, Vector2.zero, Quaternion.Euler(0,0,0));
            Debug.Log(AssetName);
            
        }
        //에러 발생
        else
        {
            Debug.Log($"에셋 번들 로드 실패 : {unityWebRequest.error}");
        }
        Debug.Log("번들가져오기 끝 " + Time.time);
    }

}
