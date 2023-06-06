using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using TMPro;
using System;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;

public class FirebaseAuthManager // : SingletonObject<FirebaseAuthManager>
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
    FirebaseAuthManager()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    public bool isCurrentUserLoggedin()
    {
        if (auth.CurrentUser == null)
            return false;
        else
            return true;
    }

    public void SigninFirebaseWithAnonymous(Action action = null)
    {
        if (auth.CurrentUser != null) return;

        auth.SignInAnonymouslyAsync().ContinueWith(task =>
        {
            Debug.Log("SignInAnonymouslyAsync");
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
            AuthResult result = task.Result;
            action?.Invoke();

            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

        });
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
                action?.Invoke();
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);
            });
    }

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