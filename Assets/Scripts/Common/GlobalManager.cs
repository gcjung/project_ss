using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GlobalManager : SingletonObject<GlobalManager>
{
    public bool Initialized { get; set; } = false;

    //사운드매니저 보면 Manager 상속받아서 만들어주고, 여기에서 받아서 초기화함.
    //이후 사운드 매니저 사용하고 싶으면 GlobalManager.instance.SoundManager.PlayBgmSound();
    public SoundManager SoundManager { get; set; } = null;
    public SceneLoadManager SceneLoadManager { get; set; } = null;

    public override void Awake()
    {
        base.Awake();
    }
    public void Init()
    {
        StartCoroutine(nameof(InitManager));
    }
    private IEnumerator InitManager()
    {
        if(Initialized)
        {
            Debug.Log($"글로벌 매니저 이미 존재함.");
            yield break;
        }
        InitGameSetting();
        {
            //클래스 초기화
            SoundManager = SoundManager.CreateManager(transform);
            SceneLoadManager = SceneLoadManager.CreateManager(transform);
        }
        
        yield return new WaitUntil(() =>
        {
            return true
            && SoundManager.Ininialized
            && SceneLoadManager.Ininialized;
        });

        SoundManager.InitializedFininsh();
        SceneLoadManager.InitializedFininsh();

        Initialized = true;
    }

    private void InitGameSetting()
    {
        Application.targetFrameRate = 60;
        Screen.SetResolution(1080, 1920, true);
    }
}
