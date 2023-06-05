using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class MainScene : MonoBehaviour
{
    [Header("Create Player")]
    private GameObject playerCharacter;
    [SerializeField] private Transform playerSpawnPoint;

    [Header("Change Map Sprite")]
    private Sprite mapSprite;
    [SerializeField] private SpriteRenderer mapOne;
    [SerializeField] private SpriteRenderer mapTwo;

    [Header("Enter Boss Room")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private UpSidePanel upSidePanel;
    private float fadeInTime = 1f;
    private float fadeOutTime = 1f;
    private float delayTime = 1f;

    private bool isStageClear = false;
    public bool IsStageClear
    {
        get { return isStageClear; }
        set
        {
            if (value)
            {
                if (MonsterSpawner.WaveCount == 5)
                {
                    isStageClear = value;
                    EnterBossStage();
                }
            }
        }
    }
    public static bool IsPlayer { get; private set; } = false;
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
        Debug.Log("메인 씬시작");

        playerCharacter = Resources.Load<GameObject>("Player/ch001");   //플레이어 캐릭터 세팅
        var _playerCharacter = Instantiate(playerCharacter, transform);
        _playerCharacter.transform.position = playerSpawnPoint.position;
        _playerCharacter.AddComponent<Player>();
        _playerCharacter.AddComponent<PlayerController>();
        IsPlayer = true;

        mapSprite = Resources.Load<Sprite>("Sprite/Map001"); //맵 세팅
        mapOne.sprite = mapSprite;
        mapTwo.sprite = mapSprite;
    }

    public void EnterBossStage()
    {
        if (isStageClear)
        {
            IsStageClear = false;

            fadeImage = Instantiate(fadeImage, upSidePanel.transform);
            //fadeImage.transform.SetAsFirstSibling();
            FadeIn();
        }
    }

    public void FadeIn()
    {
        fadeImage.DOFade(1f, fadeInTime).OnComplete(() => Invoke("VersusSetting", delayTime));
    }

    public void VersusSetting()
    {

    }
}
