using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;

public class StartPanel : PanelBase
{
    [Header("Button")]
    [SerializeField] private Button button1;    //��ŸƮ ��ư


    public override void InitPanel()
    {
        Debug.Log("��ŸƮ �г� ����");
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
