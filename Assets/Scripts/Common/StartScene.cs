using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using DG.Tweening;
using Firebase.Auth;
using UnityEngine.SocialPlatforms;
using GooglePlayGames;
using TMPro;

public class StartScene : MonoBehaviour
{
    [Header("Start Loading Image")]
    [SerializeField] private GameObject logoPanel;

    [Header("Game Start Image")]
    [SerializeField] private GameObject startPanel;

    private GameObject loadingCanvas;
    private CanvasGroup canvasGroup;

    private float fadeInTime = 2f;
    private float fadeOutTime = 2f;
    private float delayTime = 2f;


    private void Awake()
    {
        loadingCanvas = GameObject.Find("Canvas");

        logoPanel = Instantiate(logoPanel, loadingCanvas.transform);

        canvasGroup = logoPanel.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

    }

    private IEnumerator Start()
    {
        GlobalManager.Instance.Init();

        yield return CommonIEnumerator.IEWaitUntil(
           predicate: () => { return GlobalManager.Instance.Initialized; },
           onFinish: () => { 
               Init();
           }
        );

        StartCoroutine(InitializeApplicationSetting());
    }
    IEnumerator InitializeApplicationSetting()  // �α���, ���ҽ� ��ġ
    {
        yield return CommonIEnumerator.IEWaitUntil(() => { return loadingComplete; }, () => OpenUI_LoginPanel());
        
        yield return CommonIEnumerator.IEWaitUntil(() => { return FirebaseAuthManager.Instance.isCurrentLogin(); }, () => CheckAssetBundleVersion());
        
        yield return CommonIEnumerator.IEWaitUntil(() => { return ResourceLoader.Instance.downloadComplete; }, () => 
            {
                var startBtn = startPanel.transform.Find("GameStart_Image");
                startBtn.gameObject.SetActive(true);

                var seq = DOTween.Sequence();
                seq.Append(startBtn.DOScale(1.2f, 0.5f));
                seq.Append(startBtn.DOScale(1.0f, 0.5f));
                seq.Play();
            });
    }


    private void Init()
    {
        Debug.Log("��ŸƮ�� ����");

        FadeIn();
    }

    public void FadeIn()
    {
        canvasGroup.DOFade(1f, fadeInTime).OnComplete(() => Invoke(nameof(FadeOut), delayTime));
    }

    bool loadingComplete = false;
    public void FadeOut()
    {
        canvasGroup.DOFade(0f, fadeOutTime).OnComplete((TweenCallback)(() =>
        {
            DestoryPanel();
            loadingComplete = true;
        }));
    }

    private GameObject loginPanel = null;
    public void OpenUI_LoginPanel()
    {
        //FirebaseAuthManager.Instance.SingOutFirebase();
        if (!FirebaseAuthManager.Instance.isCurrentLogin())  // ���̾�̽� ���� X
        {
            if (loginPanel == null)
            {
                var obj = Resources.Load<GameObject>("UI/Login_Panel");
                loginPanel = Instantiate(obj, UIManager.instance.transform);
            }

            loginPanel.transform.Find("LoginSelect_Panel/GoogleLogin_Button").GetComponent<Button>().onClick.AddListener(
                () => FirebaseAuthManager.Instance.TrySignIn(LoginType.Google));

            loginPanel.transform.Find("LoginSelect_Panel/GuestLogin_Button").GetComponent<Button>().onClick.AddListener(
                () => FirebaseAuthManager.Instance.TrySignIn(LoginType.Guest));
        }
        else                    // ���̾�̽� ��������
        {
            if (!FirebaseAuthManager.Instance.isCurrentUserAnonymous())
                GPGS.Instance.LoginGoogleAccount();
        }
    }

    public void CloseLoginPanel()
    {
        loginPanel.gameObject.SetActive(false);
        loginPanel = null;
    }

    Slider resourceDownSlider = null;
    async void OpenUI_ResourceDownPopup(string desc ="")
    {
        var obj = Resources.Load<GameObject>("UI/ResourceDownPopup");
        long size = await ResourceLoader.Instance.GetTotalResourceSize();

        GameObject popup = Instantiate(obj, UIManager.instance.transform);

        if (!string.IsNullOrEmpty(desc))
            popup.transform.Find("Desc_Text").GetComponent<TMP_Text>().text = desc;

        popup.transform.Find("Size_Image/Text").GetComponent<TMP_Text>().text = Util.ConvertBytes(size);
        popup.transform.Find("Yes_Button").GetComponent<Button>().onClick.AddListener(() =>
        { 
            popup.SetActive(false);

            var obj = Resources.Load<GameObject>("UI/ResourceDonwSlider");
            if(resourceDownSlider == null)
            {
                resourceDownSlider = Instantiate(obj, UIManager.instance.transform).GetComponent<Slider>();
                resourceDownSlider.transform.Find("TargetValue_Text").GetComponent<TMP_Text>().text = $"/ {Util.ConvertBytes(size)}";
                
                ResourceLoader.Instance.LoadAllAssetBundle(true);
            }
   
        }); 
    }
    public void SetResourceDownloadSlider(long current, long target)
    {
        var percentText = resourceDownSlider.transform.Find("PercentValue_Text").GetComponent<TMP_Text>();
        var currentValueText = resourceDownSlider.transform.Find("CurrentValue_Text").GetComponent<TMP_Text>();

        float downloadPercent = ((float)current / (float)target * 100);
        percentText.text = $"{(int)downloadPercent} %";
        currentValueText.text = Util.ConvertBytes((long)(downloadPercent * target * 0.01f));
        resourceDownSlider.value = (float)downloadPercent;
    }


    private void CheckAssetBundleVersion()
    {
        PlayerPrefs.DeleteKey("AssetBundleVersion");

        if (!PlayerPrefs.HasKey("AssetBundleVersion"))  // ���� ����
        {
            OpenUI_ResourceDownPopup();
        }
        else        // ���� ���� ����
        {
            int version = int.Parse(GlobalManager.Instance.DBManager.GetGameData(GameDataType.AssetBundleVersion));
            if (PlayerPrefs.GetInt("AssetBundleVersion") == version)    // ���¹��� �ֽŹ���  
            {
                ResourceLoader.Instance.LoadAllAssetBundle(loadFromServer : false);
            }
            else
            {
                OpenUI_ResourceDownPopup("�߰� ���ҽ��� �ٿ�ε��մϴ�.\n�߰� �ٿ�ε带 �Ͻðڽ��ϱ�?");
            }
        }
    }
    public void DestoryPanel()
    {
        Destroy(logoPanel);
        
        startPanel = Instantiate(startPanel, loadingCanvas.transform);
    }


    #region �׽�Ʈ��
    // �׽�Ʈ
    static string log1;
    static string log2;
    static string log3;
    public static void logtest(string str1 = "", string str2 = "", string str3 = "")
    {
        if (!string.IsNullOrEmpty(str1))
            log1 = str1;
        if (!string.IsNullOrEmpty(str2))
            log2 = str2;
        if (!string.IsNullOrEmpty(str3))
            log3 = str3;
    }
    void OnGUI()
    {
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * 2);

        if (GUILayout.Button("ClearLog"))
        {
            log1 = "";
            log2 = "";
            startPanel.GetComponent<CanvasGroup>().alpha = 1f;
        }


        if (GUILayout.Button("�α��� Check"))
        {
            startPanel.GetComponent<CanvasGroup>().alpha = 0.3f;


            if (FirebaseAuthManager.Instance.isCurrentLogin())
            {
                log1 = $"���̾�̽� ��� OK, uid : {FirebaseAuth.DefaultInstance.CurrentUser.UserId}";
            }
            else
            {
                log1 = "���̾�̽� ��� NO";
            }

            ILocalUser storeUser = Social.localUser;

            //if (storeUser.authenticated == true)
            //{
            //    log2 = $"�÷��̽���� name : {storeUser.userName}, �α��λ��� : {storeUser.authenticated}";
            //}
            //else
            //{
            //    log2 = "�÷��̽���� �α��� �ȵ�";
            //}
            ILocalUser googleUser = PlayGamesPlatform.Instance.localUser;
            if (googleUser.authenticated == true)
            {
                log3 = $"���� name : {googleUser.userName}, �α��λ��� : {googleUser.authenticated}";
            }
            else
            {
                log3 = "���� �α��� �ȵ�";
            }
        }

        GUILayout.Label(log1);
        GUILayout.Label(log2);
        GUILayout.Label(log3);
    }
    #endregion �׽�Ʈ��
}

#region ���X
//public void ProgressResourceDownSlider(float current, float target)
//{
//    StartCoroutine(CoProgressResourceDownSlider(current / target * 100, target));
//}
//IEnumerator CoProgressResourceDownSlider(float target, float total)
//{
//    float duration = 2f; // ī���ÿ� �ɸ��� �ð� ����. 
//    float currentValue = resourceDownSlider.value;
//    float offset = (target - currentValue) / duration;

//    var percentText = resourceDownSlider.transform.Find("PercentValue_Text").GetComponent<TMP_Text>();
//    var currentValueText = resourceDownSlider.transform.Find("CurrentValue_Text").GetComponent<TMP_Text>();
//    while (currentValue < target)
//    {
//        currentValue += offset * Time.deltaTime;

//        resourceDownSlider.value = currentValue;
//        percentText.text = $"{(int)currentValue} %";
//        currentValueText.text = Util.ConvertBytes((long)(currentValue * total * 0.01f));

//        yield return null;
//    }

//    resourceDownSlider.value = target;
//}

#endregion ��� X