using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartPanel : PanelBase
{
    [Header("Button")]
    [SerializeField] private Button button1;    //��ŸƮ ��ư
    private new void Awake()
    {
        base.Awake();

        button1.onClick.AddListener(() => GlobalManager.Instance.SceneLoadManager.LoadSceneAsync("Main"));
    }

    public override void InitPanel()
    {
        Debug.Log("��ŸƮ �г� ����");    
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
