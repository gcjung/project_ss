using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using TMPro;
using System;
using GooglePlayGames;
using UnityEngine.UI;
using System.Threading.Tasks;


public enum LoginType
{
    Guest,
    Google,
}
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
        StartScene.logtest(str3: "생성자");
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
            var sc = GameObject.FindObjectOfType<StartScene>();
            sc.CloseLoginPanel();
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

    // 익명계정을 플레이게임즈계정으로 연동시키기
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

        //if (GUILayout.Button("구글Login"))
        //    GPGS.Instance.Login((success, localUser) =>
        //    log = $"{success}, {localUser.userName}, {localUser.id}, {localUser.state}, {localUser.underage}");

        //if (GUILayout.Button("구글Logout"))
        //{
        //    GPGS.Instance.Logout();
        //}
        //if (GUILayout.Button("구글 파이어베이스 등록"))
        //{
        //    TryFirebaseGoogleLogin(success =>
        //    log = $"{success}, 파이어베이스 등록"
        //    );
        //}
        //if(GUILayout.Button("구글 파이어베이스 로그아웃"))
        //{
        //    auth.SignOut();
        //}
        if (GUILayout.Button("플레이스토어 로그인"))
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
        if (GUILayout.Button("플레이스토어 파이어베이스등록"))
        {
            Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
            Firebase.Auth.Credential credential =
                Firebase.Auth.PlayGamesAuthProvider.GetCredential(authCode);
            auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
                    log = $"실패!! 플레이스토어 파이어베이스 실패";
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);
                    log = $"실패!! 플레이스토어 파이어베이스 실패";
                    return;
                }
                log = $"성공!! 플레이스토어 파이어베이스 등록성공";
                Firebase.Auth.AuthResult result = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    result.User.DisplayName, result.User.UserId);
            });
        }
        if (GUILayout.Button("익명 로그인"))
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
        if (GUILayout.Button("파이어베이스 Check"))
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
                log = "현재 파이어베이스 인증 안되어있음";
            }
        }
        if (GUILayout.Button("플레이스토어 Check"))
        {
            ILocalUser user = Social.localUser;
            
            if (user != null)
            {
                log = $"name : {user.userName}, email : {user.id}, state : {user.state}, 로그인상태 : {user.authenticated}";
            }
            else
            {
                log = "현재 로그인 안되어있음";
            }
        }
        GUILayout.Label(log);

    }
 */
#endregion

#region 지금은 안씀
//public void TryFirebaseGoogleLogin(Action<bool> onLoginSuccess = null)
//{
//    string idToken = ((PlayGamesLocalUser)Social.localUser).GetIdToken();
//    //idtoken.text = idToken != "" ? idToken : "비어있음";

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
//    Debug.Log("로그아웃");
//}


/// 파이어베이스 이메일 계정생성
//public void Create(string email, string password)
//{
//    auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(t =>
//    {
//        if (t.IsCanceled)
//        {
//            Debug.LogError("회원가입 취소");
//            return;
//        }
//        if (t.IsFaulted)
//        {
//            Debug.LogError("회원가입 실패");
//            return;
//        }
//        //FirebaseUser newUser = t.Result;
//        Debug.LogError("회원가입 완료");
//    });
//}

// 파이어베이스 이메일 로그인
//public void Login(string email, string password)
//{
//    auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(t =>
//    {
//        if (t.IsCanceled)
//        {
//            Debug.LogError("로그인 취소");
//            return;
//        }
//        if (t.IsFaulted)
//        {
//            Debug.LogError("로그인 실패");
//            return;
//        }
//        //FirebaseUser newUser = t.Result;
//        Debug.LogError("로그인 완료");
//    });
//}

//private GameObject loginPanel = null;
//public void OpenUI_LoginPanel()
//{
//    auth.SignOut();
//    if (!isCurrentLogin())  // 파이어베이스 연동 X
//    {
//        if (loginPanel == null)
//        {
//            var obj = Resources.Load<GameObject>("UIPrefabs/Login_Panel");

//            //loginPanel = CommonFuntion.GetPrefab("Login_Panel", UIManager.instance.transform);
//        }

//        loginPanel.transform.Find("LoginSelect_Panel/GoogleLogin_Button").GetComponent<Button>().onClick.AddListener(
//            () => TrySignIn(LoginType.Google));

//        loginPanel.transform.Find("LoginSelect_Panel/GuestLogin_Button").GetComponent<Button>().onClick.AddListener(
//            () => TrySignIn(LoginType.Guest));
//    }
//    else                    // 파이어베이스 연동상태
//    {
//        if (!isCurrentUserAnonymous())
//            GPGS.Instance.LoginGoogleAccount();
//    }
//}

//public void CloseLoginPanel()
//{
//    loginPanel.gameObject.SetActive(false);
//    loginPanel = null;
//}

#endregion 지금은 안씀