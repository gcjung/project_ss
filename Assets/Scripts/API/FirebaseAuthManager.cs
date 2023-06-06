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