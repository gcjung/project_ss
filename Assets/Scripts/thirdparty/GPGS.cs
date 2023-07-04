using GooglePlayGames.BasicApi;
using GooglePlayGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

enum LoginState { }
public class GPGS
{
    private static GPGS instance;
    public static GPGS Instance
    { 
        get 
        {
            if (instance == null)
                instance = new GPGS();
            
            return instance; 
        } 
    }

    GPGS() => Init();

    public void Init()
    {
        var config = new PlayGamesClientConfiguration.Builder().
            RequestServerAuthCode(false).
            RequestIdToken().
            Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
    }

    public void LoginPlayGames(Action<bool> func = null)
    {
        if (Social.localUser.authenticated) return;

        Social.localUser.Authenticate(success =>
        {
            func?.Invoke(success);
        });
    }

    public void LogoutPlayGames()
    {
        if (Social.localUser.authenticated) // 로그인 되어 있다면
        {
            PlayGamesPlatform.Instance.SignOut(); // Google 로그아웃
        }
    }

    
    public void LoginGoogleAccount()
    {
        if (Social.localUser.authenticated) // 로그인 되어 있다면
            return;

        PlayGamesPlatform.Instance.Authenticate(success =>
        {
            if (success)
            {

            }
            else
            {
                
            }
        });
    }

    [Obsolete("구글로그아웃 사용안함",true)]
    public void TryGoogleLogout()
    {
        if (Social.localUser.authenticated) // 로그인 되어 있다면
        {
            PlayGamesPlatform.Instance.SignOut(); // Google 로그아웃
        }
    }
}




/// 구글 로그아웃
//public void Logout()
//{
//    PlayGamesPlatform.Instance.SignOut();
//}

/// 구글로그인
//public void Login(Action<bool, UnityEngine.SocialPlatforms.ILocalUser> onLoginSuccess = null)
//{
//    PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptAlways, (success) =>
//    {
//        onLoginSuccess?.Invoke(success == SignInStatus.Success, Social.localUser);
//    });
//}