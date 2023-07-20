using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using Firebase.Storage;
using System.Threading.Tasks;
using UnityEditor.SceneManagement;
using Firebase.Extensions;

public class testt : MonoBehaviour
{
    public SpriteAtlas atlas;
    public Image testImg;
    public Sprite bindsp;

    private Image image;

    private void Start()
    {
        StorageReference gsReference = FirebaseStorage.DefaultInstance.GetReferenceFromUrl(@"gs://projectss-c99e7.appspot.com");


        gsReference.GetDownloadUrlAsync().ContinueWithOnMainThread(task => {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                Debug.Log("Download URL: " + task.Result);
                // ... now download the file via WWW or UnityWebRequest.
            }
            var result = task.Result;
        });
        //Invoke(nameof(SpriteTest), 5f);

    }
    void SpriteTest()
    {
        image = UIManager.instance.transform.Find("Image").GetComponent<Image>();
        image.sprite = CommonFuntion.GetSprite_Atlas("UI_Skill_Icon_Arrow_Barrage", "SkillAtlas");
    }

}
