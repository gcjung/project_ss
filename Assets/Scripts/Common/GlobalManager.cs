using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GlobalManager : SingletonObject<GlobalManager>
{
    public bool Initialized { get; set; } = false;

    //����Ŵ��� ���� Manager ��ӹ޾Ƽ� ������ְ�, ���⿡�� �޾Ƽ� �ʱ�ȭ��.
    //���� ���� �Ŵ��� ����ϰ� ������ GlobalManager.instance.SoundManager.PlayBgmSound();
    public SoundManager SoundManager { get; set; } = null;
    public SceneLoadManager SceneLoadManager { get; set; } = null;
    public DBManager DBManager { get; set; } = null;
    public GameDataManager GameDataManager { get; set; } = null;

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
        if (Initialized)
        {
            Debug.Log($"�۷ι� �Ŵ��� �̹� ������.");
            yield break;
        }
        InitGameSetting();
        {
            //Ŭ���� �ʱ�ȭ
            SoundManager = SoundManager.CreateManager(transform);
            SceneLoadManager = SceneLoadManager.CreateManager(transform);
            DBManager = DBManager.CreateManager(transform);
            GameDataManager = GameDataManager.CreateManager(transform);
        }
        
        yield return new WaitUntil(() =>
        {
            return true
            && SoundManager.Ininialized
            && SceneLoadManager.Ininialized
            && DBManager.Ininialized
            && GameDataManager.Ininialized;
        });

        SoundManager.InitializedFininsh();
        SceneLoadManager.InitializedFininsh();
        DBManager.InitializedFininsh();
        GameDataManager.InitializedFininsh();

        Initialized = true;
    }

    private void InitGameSetting()
    {
        Application.targetFrameRate = 60;
        Screen.SetResolution(1080, 1920, true);
    }
}
