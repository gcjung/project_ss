using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScene : MonoBehaviour
{
    [Header("Loading Panel")]
    [SerializeField] private GameObject loadingPanel;

    private GameObject loadingCanvas;
    private void Awake()
    {
        loadingCanvas = GameObject.Find("Canvas");

        loadingPanel = Instantiate(loadingPanel, loadingCanvas.transform);
    }
    private IEnumerator Start()
    {
        GlobalManager.Instance.Init();

        yield return CommonIEnumerator.IEWaitUntil(
           predicate: () => { return GlobalManager.Instance.Initialized; },
           onFinish: () => { Init(); }
        );
    }

    private void Init()
    {
        Debug.Log("로딩씬 시작");

        GC.Collect();
    }
}
