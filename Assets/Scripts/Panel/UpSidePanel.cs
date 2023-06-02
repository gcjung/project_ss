using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class UpSidePanel : MonoBehaviour
{
    [Header("Stage Fade Image")]
    [SerializeField] private GameObject fadeImage;
    private CanvasGroup canvasGroup;

    private bool isStageClear = false;
    public bool IsStageClear
    {
        get { return isStageClear; }
        set
        {
            if(value)
            {
                if (StageSlider.WaveCount == 5 && (PlayerController.CurrentPlayerState == PlayerState.Moving))
                {
                    isStageClear = value;
                    EnterBossStage();
                }
            }            
        }
    }  

    private float fadeInTime = 1f;
    private float fadeOutTime = 1f;
    private float delayTime = 1f;
    private void Start()
    {
        canvasGroup = fadeImage.GetComponent<CanvasGroup>();
    }

    public void EnterBossStage()
    {
        Debug.Log("이미지 테스트");
        if(isStageClear)
        {
            IsStageClear = false;

            fadeImage = Instantiate(fadeImage, transform);
            fadeImage.transform.SetAsFirstSibling();
            FadeIn();
        }
    }

    public void FadeIn()
    {
        canvasGroup.DOFade(1f, fadeInTime).OnComplete(() => Invoke("FadeOut", delayTime));
    }

    public void FadeOut()
    {
        canvasGroup.DOFade(0f, fadeOutTime).OnComplete(() => Destroy(fadeImage));
    }
}
