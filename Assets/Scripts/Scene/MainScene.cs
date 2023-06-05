using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class MainScene : MonoBehaviour
{
    [Header("Create Player")]  
    [SerializeField] private Transform playerSpawnPoint;
    private GameObject playerCharacter;

    [Header("Change Map Sprite")]   
    [SerializeField] private SpriteRenderer mapOne;
    [SerializeField] private SpriteRenderer mapTwo;
    private Sprite mapSprite;

    [Header("Enter Boss Room")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private UpSidePanel upSidePanel;
    [SerializeField] private Image vsImage;
    [SerializeField] private Transform playerPosition;
    [SerializeField] private Transform bossPosition;
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
        Debug.Log("���� ������");

        playerCharacter = Resources.Load<GameObject>("Player/ch001");   //�÷��̾� ĳ���� ����
        var _playerCharacter = Instantiate(playerCharacter, transform);
        _playerCharacter.transform.position = playerSpawnPoint.position;
        _playerCharacter.AddComponent<Player>();
        _playerCharacter.AddComponent<PlayerController>();
        IsPlayer = true;

        mapSprite = Resources.Load<Sprite>("Sprite/Map001"); //�� ����
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
        int targetLayer = LayerMask.NameToLayer("Over UI");

        var _vsImage = Instantiate(vsImage, upSidePanel.transform);

        var player = Resources.Load<GameObject>("Player/ch001");
        var _player = Instantiate(player, playerPosition);
        _player.transform.position = playerPosition.position;
        ChangeLayer(_player, targetLayer);

        var boss = Resources.Load<GameObject>("Monster/ch009");
        var _boss = Instantiate(boss, bossPosition);
        _boss.transform.position = bossPosition.position;
        ChangeLayer(_boss, targetLayer);
    }
    void ChangeLayer(GameObject obj, int layer)
    {
        obj.layer = layer;

        for (int i = 0; i < obj.transform.childCount; i++)
        {
            GameObject childObj = obj.transform.GetChild(i).gameObject;
            ChangeLayer(childObj, layer);
        }
    }
}