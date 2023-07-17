using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;

public class StartPanel : PanelBase
{
    [Header("Button")]
    [SerializeField] private Button button1;    //스타트 버튼


    public override void InitPanel()
    {
        Debug.Log("스타트 패널 생성");
        button1.onClick.AddListener(() => GlobalManager.Instance.SceneLoadManager.LoadSceneAsync("Main"));
    }

    public override void OpenPanel()
    {

    }

    public override void ClosePanel()
    {

    }
    private void OnDestroy()
    {
        button1.onClick.RemoveAllListeners();
    }
}
