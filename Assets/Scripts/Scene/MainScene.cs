using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using static GameDataManager;
public class MainScene : MonoBehaviour
{
    private static MainScene instance;
    public static MainScene Instance
    {
        get
        {
            if (instance == null)
                return null;

            return instance;
        }
    }
    [Header("UI Canvas")]
    [SerializeField] private UpSidePanel upSidePanel;
    //[SerializeField] private DownSidePanel downSidePanel;

    [Header("Create Player")]
    [SerializeField] private Transform playerSpawnPoint;
    private GameObject playerCharacter;   

    [Header("Change Map Sprite")]   
    [SerializeField] private SpriteRenderer map1;
    [SerializeField] private SpriteRenderer map2;
    private Sprite mapSprite;

    [Header("Enter Boss Room")]
    [SerializeField] private Image fadeImage;   
    [SerializeField] private Image vsImage;
    [SerializeField] private Transform playerPosition;
    [SerializeField] private Transform bossPosition;
    private GameObject playerPref;
    private GameObject bossPref;
    

    [Header("Stage Clear")]
    [SerializeField] private Transform movePoint;
    [SerializeField] private TextMeshProUGUI victoryText;

    private MonsterSpawner spawner; //�������� ���ÿ�
    [HideInInspector] public string mapName;
    [HideInInspector] public string monsterName;
    [HideInInspector] public string bossName;

    private bool isWaveClear = false;
    public bool IsWaveClear
    {
        get { return isWaveClear; }
        set
        {
            if (value)
            {
                if (MonsterSpawner.WaveCount == 5)
                {
                    isWaveClear = value;
                    EnterBossRoom();
                }
            }
        }
    }

    private bool isStageClear = false;
    public bool IsStageClear
    {
        get { return isStageClear; }
        set
        {
            if (value)
            {
                isStageClear = value;
                StageClear();
            }
        }
    }
    public static bool IsPlayer { get; private set; } = false;


    [SerializeField] private GameObject mainUiPanel;
    private TMP_Text goldText;
    private TMP_Text gemText;
    private IEnumerator Start()
    {
        GlobalManager.Instance.Init();

        yield return CommonIEnumerator.IEWaitUntil(
           predicate: () => { return GlobalManager.Instance.Initialized; },
           onFinish: () => 
           { 
               Init();
               InitUIfromDB();
           }
        );

        spawner = FindChildComponent<MonsterSpawner>(transform);
    }

    private void Init()
    {
        if (instance == null)
            instance = this;

        Debug.Log("���� ������");

        SetStage("1-1");    //���߿� ���� ���̺� ����Ǿ� �ִ� ���������� �÷����ϴ� �������� �޾Ƽ� �־��� ����

        SetPlayer("ch001"); //�̰͵� ���� ���̺��� ������ ���� ���� ĳ���� �޾Ƽ� �־��� ����

        goldText = mainUiPanel.transform.Find("UpSide_Panel/Goods_Panel/Gold_Image/Gold_Text").GetComponent<TMP_Text>();
        gemText = mainUiPanel.transform.Find("UpSide_Panel/Goods_Panel/Gem_Image/Gem_Text").GetComponent<TMP_Text>();
    }
    public void SetPlayer(string charName)
    {
        if (playerCharacter != null)
            Destroy(playerCharacter);

        var _playerCharacter = Resources.Load<GameObject>($"Player/{charName}");   //�÷��̾� ĳ���� ����
        playerCharacter = Instantiate(_playerCharacter, transform);
        playerCharacter.transform.position = playerSpawnPoint.position;
        playerCharacter.AddComponent<Player>();
        playerCharacter.AddComponent<PlayerController>();
        IsPlayer = true;
    }
    public void SetStage(string stageName)
    {
        mapName = MonsterSpawnTemplate[stageName][(int)MonsterSpawnTemplate_.MapImage];  //�ʼ���
        monsterName = MonsterSpawnTemplate[stageName][(int)MonsterSpawnTemplate_.Monster];   //���� ����
        bossName = MonsterSpawnTemplate[stageName][(int)MonsterSpawnTemplate_.Boss]; //�������� ����

        mapSprite = Resources.Load<Sprite>($"Sprite/{mapName}"); //�� ����
        map1.sprite = mapSprite;
        map2.sprite = mapSprite;
    }
    private void InitUIfromDB()
    {
        Debug.Log("InitUIfromDB");
        goldText.text = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.Gold).ToString();
        gemText.text = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.Gem).ToString();
    }

    private void EnterBossRoom()
    {
        if (isWaveClear)
        {
            Debug.Log("������ ����");
            IsWaveClear = false;

            float fadeInTime = 1.0f;
            float delayTime = 2.0f;

            var _fadeImage = Instantiate(fadeImage, upSidePanel.transform);
            //fadeImage.transform.SetAsFirstSibling();
            _fadeImage.DOFade(1f, fadeInTime).OnComplete(() => Destroy(_fadeImage));

            Invoke("StartBossBattle", delayTime);
        }
    }
    private void StartBossBattle()
    {
        StartCoroutine(BossBattle());
    }
    private IEnumerator BossBattle()
    {
        float delayTime = 2.0f;

        int targetLayer = LayerMask.NameToLayer("Over UI");

        var _vsImage = Instantiate(vsImage, upSidePanel.transform);

        playerPref = Resources.Load<GameObject>("Player/ch001");
        var _playerPref = Instantiate(playerPref, playerPosition);
        _playerPref.transform.position = playerPosition.position;
        ChangeLayer(_playerPref, targetLayer);

        bossPref = Resources.Load<GameObject>($"Monster/{bossName}");
        var _bossPref = Instantiate(bossPref, bossPosition);
        _bossPref.transform.position = bossPosition.position;
        ChangeLayer(_bossPref, targetLayer);

        yield return new WaitForSeconds(delayTime);

        Destroy(_bossPref);
        Destroy(_playerPref);
        Destroy(_vsImage.gameObject);

        spawner.SpawnBossMonster();
    }
    private void ChangeLayer(GameObject obj, int layer)
    {
        obj.layer = layer;

        for (int i = 0; i < obj.transform.childCount; i++)
        {
            GameObject childObj = obj.transform.GetChild(i).gameObject;
            ChangeLayer(childObj, layer);
        }
    }

    private void StageClear()
    {
        float fadeTime = 1.0f;

        var _victoryText = Instantiate(victoryText, upSidePanel.transform);

        Color color = _victoryText.color;    //���İ� 0���� �ʱ�ȭ
        color.a = 0f;
        _victoryText.color = color;

        _victoryText.DOFade(1f, fadeTime).OnComplete(() =>
        _victoryText.DOFade(0f, fadeTime).OnComplete(() => StageClear2()));
    }
    private void StageClear2()
    {
        float durationTime = 1.0f;
        float delayTime = 3.0f;
        float movingTime = 3.0f;

        var _fadeImage = Instantiate(fadeImage, upSidePanel.transform);
        _fadeImage.DOFade(1f, durationTime);

        playerCharacter.transform.DOMove(movePoint.position, movingTime);
    }
    private T FindChildComponent<T>(Transform parent) where T : Component
    {
        T foundComponent = null;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            T component = child.GetComponent<T>();
            if (component != null)
            {
                foundComponent = component;
                break;
            }

            foundComponent = FindChildComponent<T>(child);
            if (foundComponent != null)
                break;
        }

        return foundComponent;
    }

    public void GetGoods(double getGold = 0, double getGem = 0)
    {
        if (getGold > 0)
        {
            double currentGold = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.Gold);

            goldText.text = $"{(currentGold + getGold)}";
            GlobalManager.Instance.DBManager.UpdateUserData(UserDoubleDataType.Gold.ToString(), currentGold + getGold);
        }

        if (getGem > 0)
        {
            double currentGem = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.Gem);
            
            gemText.text = $"{(currentGem + getGem)}";
            GlobalManager.Instance.DBManager.UpdateUserData(UserDoubleDataType.Gem.ToString(), currentGem + getGem);
        }
    }
}
