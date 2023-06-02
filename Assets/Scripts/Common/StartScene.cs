using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using DG.Tweening;
public class StartScene : MonoBehaviour
{
    [Header("Start Loading Image")]
    [SerializeField] private GameObject logoPanel;

    [Header("Game Start Image")]
    [SerializeField] private GameObject startPanel;

    private GameObject loadingCanvas;
    private CanvasGroup canvasGroup;

    private float fadeInTime = 2f;
    private float fadeOutTime = 2f;
    private float delayTime = 2f;
    private void Awake()
    {
        loadingCanvas = GameObject.Find("Canvas");

        logoPanel = Instantiate(logoPanel, loadingCanvas.transform);

        canvasGroup = logoPanel.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
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
        Debug.Log("스타트씬 시작");

        FadeIn();
    }

    public void FadeIn()
    {
        canvasGroup.DOFade(1f, fadeInTime).OnComplete(() => Invoke("FadeOut", delayTime));
    }

    public void FadeOut()
    {
        canvasGroup.DOFade(0f, fadeOutTime).OnComplete(() => DestoryPanel());
    }

    public void DestoryPanel()
    {
        Destroy(logoPanel);

        Instantiate(startPanel, loadingCanvas.transform);
    }
}
