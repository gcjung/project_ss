using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using TMPro;
using System;
using GooglePlayGames;
using UnityEngine.UI;
using System.Threading.Tasks;

public class FirebaseAuthManager
{
    private static FirebaseAuthManager instance;
    public static FirebaseAuthManager Instance
    {
        get
        {
            if (instance == null)
                instance = new FirebaseAuthManager();

            return instance;
        }
    }

    private FirebaseAuth auth;
    public string UserId => auth.CurrentUser.UserId;
    FirebaseAuthManager()
    {
        auth = FirebaseAuth.DefaultInstance;
        StartScene.logtest(str3: "������");
    }
    
    public enum LoginType
    {
        Guest,
        Google,
    }

    private GameObject loginPanel;
    public void ShowLoginPanel()
    {
        if (!isCurrentLogin())  // ���̾�̽� ���� X
        {
            if (loginPanel == null)
            {
                loginPanel = CommonFuntion.GetPrefab("UI/Login_Panel", UIManager.instance.transform);
            }

            loginPanel.transform.Find("LoginSelect_Panel/GoogleLogin_Button").GetComponent<Button>().onClick.AddListener(
                () => TrySignIn(LoginType.Google));

            loginPanel.transform.Find("LoginSelect_Panel/GuestLogin_Button").GetComponent<Button>().onClick.AddListener(
                () => TrySignIn(LoginType.Guest));
        }
        else        // ���̾�̽� ��������
        {
            if (!isCurrentUserAnonymous())
                GPGS.Instance.LoginGoogleAccount();
        }
    }

    public void CloseLoginPanel()
    {
        loginPanel.gameObject.SetActive(false);
        loginPanel = null;
    }
    public void SingOutFirebase()
    {
        auth.SignOut();
    }

    public bool isCurrentLogin()
    {
        if (auth.CurrentUser == null)
            return false;
        else
            return true;
    }

    public bool isCurrentUserAnonymous() 
    {
        if (auth.CurrentUser.IsAnonymous)
            return true;
        else
            return false;
    }
    public async void TrySignIn(LoginType type)
    {
        bool result = false;
        switch (type)
        {
            case LoginType.Guest:
                result = await SigninFirebaseWithAnonymous();
                break;

            case LoginType.Google:
                GPGS.Instance.LoginGoogleAccount();
                result = await SignInFirebaseWithGoogleAccount();
                break;
        }

        if(result)
        {
            CloseLoginPanel();
        }
        else
        {

        }
        
    }
    public async Task<bool> SigninFirebaseWithAnonymous(Action action = null)
    {
        bool success = true;
        if (auth.CurrentUser != null) return success;

        await auth.SignInAnonymouslyAsync().ContinueWith((task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                success = false;
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                success = false;
                return;
            }
            AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

            action?.Invoke();
        }));
        
        return success;
    }

    public void SignInFirebaseWithPlayGames(Action action = null)
    {
        Credential credential = PlayGamesAuthProvider.GetCredential(PlayGamesPlatform.Instance.GetServerAuthCode());
        auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);
                    return;
                }
                AuthResult result = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

                action?.Invoke();
            });
    }

    public async Task<bool> SignInFirebaseWithGoogleAccount(Action action = null)
    {
        bool result = true;
        if (auth.CurrentUser != null) return result;

        string idToken = ((PlayGamesLocalUser)Social.localUser).GetIdToken();

        await Task.Delay(1000);
        idToken = ((PlayGamesLocalUser)Social.localUser).GetIdToken();

        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
        await auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithCredentialAsync was canceled.");
                result = false;
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                result = false;
                return;
            }

            action?.Invoke();
        });

        return result;
    }
    /*
        public void SignInFirebaseWithGoogleAccount(Action action = null)
    {
        string idToken = ((PlayGamesLocalUser)PlayGamesPlatform.Instance.localUser).GetIdToken();

        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
        auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithCredentialAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                return;
            }
            
            FirebaseUser user = task.Result.User;
            action?.Invoke();
            Debug.LogFormat("User signed in successfully: {0} ({1})", user.DisplayName, user.UserId);
        });
    }
     */

    // �͸������ �÷��̰������������ ������Ű��
    public void LinkAnonymous2PlayGamesAccount()
    {
        Credential credential = PlayGamesAuthProvider.GetCredential(PlayGamesPlatform.Instance.GetServerAuthCode());

        auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWith(t =>
        {
            if (t.IsCanceled)
            {
                Debug.Log("LinkWithCredentialAsync was canceled");
                return;
            }
            if (t.IsFaulted)
            {
                Debug.Log("LinkWithCredentialAsync encountered an error");
                return;
            }
            AuthResult result = t.Result;
        });
    }


}

#region GUI
/*
 void OnGUI()
    {
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * 3);


        if (GUILayout.Button("ClearLog"))
            log = "";

        //if (GUILayout.Button("����Login"))
        //    GPGS.Instance.Login((success, localUser) =>
        //    log = $"{success}, {localUser.userName}, {localUser.id}, {localUser.state}, {localUser.underage}");

        //if (GUILayout.Button("����Logout"))
        //{
        //    GPGS.Instance.Logout();
        //}
        //if (GUILayout.Button("���� ���̾�̽� ���"))
        //{
        //    TryFirebaseGoogleLogin(success =>
        //    log = $"{success}, ���̾�̽� ���"
        //    );
        //}
        //if(GUILayout.Button("���� ���̾�̽� �α׾ƿ�"))
        //{
        //    auth.SignOut();
        //}
        if (GUILayout.Button("�÷��̽���� �α���"))
        {
            Social.localUser.Authenticate((bool success) => {
                // handle success or failure
                if (success)
                {
                    authCode = PlayGamesPlatform.Instance.GetServerAuthCode();
                    log = $"{success}, authCode : {authCode}";
                }
                else { }
                
            });
        }
        if (GUILayout.Button("�÷��̽���� ���̾�̽����"))
        {
            Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
            Firebase.Auth.Credential credential =
                Firebase.Auth.PlayGamesAuthProvider.GetCredential(authCode);
            auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
                    log = $"����!! �÷��̽���� ���̾�̽� ����";
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);
                    log = $"����!! �÷��̽���� ���̾�̽� ����";
                    return;
                }
                log = $"����!! �÷��̽���� ���̾�̽� ��ϼ���";
                Firebase.Auth.AuthResult result = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    result.User.DisplayName, result.User.UserId);
            });
        }
        if (GUILayout.Button("�͸� �α���"))
        {
            auth.SignInAnonymouslyAsync().ContinueWith(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInAnonymouslyAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                    return;
                }

                Firebase.Auth.AuthResult result = task.Result;
                log = string.Format("User signed in successfully: {0}, {1}", result.User.DisplayName, result.User.UserId);
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    result.User.DisplayName, result.User.UserId);
            });


        }
        if (GUILayout.Button("���̾�̽� Check"))
        {
            Firebase.Auth.FirebaseUser user = auth.CurrentUser;
            if (user != null)
            {
                string name = user.DisplayName;
                string email = user.Email;
                System.Uri photo_url = user.PhotoUrl;
                // The user's Id, unique to the Firebase project.
                // Do NOT use this value to authenticate with your backend server, if you
                // have one; use User.TokenAsync() instead.
                string uid = user.UserId;
                log = $"name : {name}, email : {email}, uid : {uid}";
            }
            else
            {
                log = "���� ���̾�̽� ���� �ȵǾ�����";
            }
        }
        if (GUILayout.Button("�÷��̽���� Check"))
        {
            ILocalUser user = Social.localUser;
            
            if (user != null)
            {
                log = $"name : {user.userName}, email : {user.id}, state : {user.state}, �α��λ��� : {user.authenticated}";
            }
            else
            {
                log = "���� �α��� �ȵǾ�����";
            }
        }
        GUILayout.Label(log);

    }
 */
#endregion

//public void TryFirebaseGoogleLogin(Action<bool> onLoginSuccess = null)
//{
//    string idToken = ((PlayGamesLocalUser)Social.localUser).GetIdToken();
//    //idtoken.text = idToken != "" ? idToken : "�������";

//    Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
//    //auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
//    auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task =>
//    {
//        if (task.IsCanceled)
//        {
//            Debug.LogError("SignInWithCredentialAsync was canceled.");
//            onLoginSuccess?.Invoke(false);
//            return;
//        }
//        if (task.IsFaulted)
//        {
//            Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
//            onLoginSuccess?.Invoke(false);
//            return;
//        }

//        //FirebaseUser newUser = task.Result;
//        //Debug.LogFormat("User signed in successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);
//        AuthResult result = task.Result;
//        onLoginSuccess?.Invoke(true);
//        Debug.LogFormat("User signed in successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);
//    });
//}

//public void TryFirebaseGoogleLogOut()
//{
//    auth.SignOut();
//    Debug.Log("�α׾ƿ�");
//}


/// ���̾�̽� �̸��� ��������
//public void Create(string email, string password)
//{
//    auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(t =>
//    {
//        if (t.IsCanceled)
//        {
//            Debug.LogError("ȸ������ ���");
//            return;
//        }
//        if (t.IsFaulted)
//        {
//            Debug.LogError("ȸ������ ����");
//            return;
//        }
//        //FirebaseUser newUser = t.Result;
//        Debug.LogError("ȸ������ �Ϸ�");
//    });
//}

// ���̾�̽� �̸��� �α���
//public void Login(string email, string password)
//{
//    auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(t =>
//    {
//        if (t.IsCanceled)
//        {
//            Debug.LogError("�α��� ���");
//            return;
//        }
//        if (t.IsFaulted)
//        {
//            Debug.LogError("�α��� ����");
//            return;
//        }
//        //FirebaseUser newUser = t.Result;
//        Debug.LogError("�α��� �Ϸ�");
//    });
//}