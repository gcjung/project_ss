using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class RenderImageToPNG : MonoBehaviour
{
    public RenderTexture rt;
    private int i = 0;
    
    public Transform[] characterSkin;
    private void Awake()
    {
        
        
    }
    void Start()
    {
        //StartCoroutine(nameof(Co_toTexture2D));
        
    }

    IEnumerator Co_toTexture2D()
    {
        for (int i = 0; i < 1; i++)
        {
            yield return new WaitForSeconds(0.5f);
            //if (i == 0) continue;

            //characterSkin[i-1].gameObject.SetActive(false);
            characterSkin[i].gameObject.SetActive(true);
            yield return new WaitForSeconds(0.3f);

            SaveRenderTextureToPng(Application.dataPath + "/Sprites/Characters/" + i.ToString()+ ". " +characterSkin[i].name + ".png", rt);
            
        }
    }

    public static void SaveRenderTextureToPng(string path, RenderTexture rt)
    {
        // ���� �ؽ��ĸ� ����Ʈȭ ��ŵ�ϴ�.
        var bytes = toTexture2D(rt).EncodeToPNG();
        File.WriteAllBytes(path, bytes);

    }

    private static Texture2D toTexture2D(RenderTexture rTex)
    {
        // ���� �ؽ����� ���� X ���� �������, RGBA32 ������ ���� �ؽ���2D�� ����ϴ�.
        var tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);
        RenderTexture.active = rTex;

        // ���� �ؽ��� ����X���� ������� ���� �� �ִ� �ȼ��� ó���մϴ�.
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);

        //����
        tex.Apply();   
        return tex;
    }
}
