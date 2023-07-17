using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using DG.Tweening;
using Firebase.Auth;
using UnityEngine.SocialPlatforms;
using GooglePlayGames;
using System.Threading.Tasks;

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
    }

  
    private void Init()
    {
        Debug.Log("��ŸƮ�� ����");

        FadeIn();
    }

    public void FadeIn()
    {
        canvasGroup.DOFade(1f, fadeInTime).OnComplete(() => Invoke("FadeOut", delayTime));
    }

    public void FadeOut()
    {
        canvasGroup.DOFade(0f, fadeOutTime).OnComplete((TweenCallback)(() =>
        {
            DestoryPanel();
            ShowLoginPanel();
        }));
    }
    private void ShowLoginPanel()
    {
        FirebaseAuthManager.Instance.ShowLoginPanel();
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
