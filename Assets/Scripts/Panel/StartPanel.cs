using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;

public class StartPanel : PanelBase
{
    [Header("Button")]
    [SerializeField] private Button button1;    //스타트 버튼

    [Header("Login관련")]
    [SerializeField] private Transform loginPanel;
    [SerializeField] private Button googleLoginButton;
    [SerializeField] private Button guestLoginButton;
    //private new void Awake()
    //{
    //    base.Awake();
    //    auth = FirebaseAuth.DefaultInstance;
    //    loginPanel = transform.Find("LoginPanel");
    //    button1.onClick.AddListener(() => GlobalManager.Instance.SceneLoadManager.LoadSceneAsync("Main"));
    //}

    public override void InitPanel()
    {
        Debug.Log("스타트 패널 생성");
        button1.onClick.AddListener(() => GlobalManager.Instance.SceneLoadManager.LoadSceneAsync("Main"));

        if (!FirebaseAuthManager.Instance.isCurrentUserLoggedin())
            ShowLoginPanel();
    }

    void ShowLoginPanel()
    {
        loginPanel.gameObject.SetActive(true);
        googleLoginButton.onClick.AddListener(() => FirebaseAuthManager.Instance.SignInFirebaseWithGoogleAccount(CloseLoginPanel));
        guestLoginButton.onClick.AddListener(() => FirebaseAuthManager.Instance.SigninFirebaseWithAnonymous(CloseLoginPanel));
    }
    public void CloseLoginPanel()
    {
        loginPanel.gameObject.SetActive(false);
    }
    
    public override void OpenPanel()
    {

    }

    public override void ClosePanel()
    {

    }
    private void OnDestroy()
    {
        googleLoginButton.onClick.RemoveAllListeners();
        guestLoginButton.onClick.RemoveAllListeners();
        button1.onClick.RemoveAllListeners();
    }
}
