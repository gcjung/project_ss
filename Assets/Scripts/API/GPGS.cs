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
        if (Social.localUser.authenticated) // �α��� �Ǿ� �ִٸ�
        {
            PlayGamesPlatform.Instance.SignOut(); // Google �α׾ƿ�
        }
    }

    
    public void LoginGoogleAccount()
    {
        if (Social.localUser.authenticated) // �α��� �Ǿ� �ִٸ�
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

    [Obsolete("���۷α׾ƿ� ������",true)]
    public void TryGoogleLogout()
    {
        if (Social.localUser.authenticated) // �α��� �Ǿ� �ִٸ�
        {
            PlayGamesPlatform.Instance.SignOut(); // Google �α׾ƿ�
        }
    }
}




/// ���� �α׾ƿ�
//public void Logout()
//{
//    PlayGamesPlatform.Instance.SignOut();
//}

/// ���۷α���
//public void Login(Action<bool, UnityEngine.SocialPlatforms.ILocalUser> onLoginSuccess = null)
//{
//    PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptAlways, (success) =>
//    {
//        onLoginSuccess?.Invoke(success == SignInStatus.Success, Social.localUser);
//    });
//}