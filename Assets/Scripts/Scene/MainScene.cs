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

    private MonsterSpawner spawner; //스테이지 세팅용
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
                    EnterBossStage();
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

        Debug.Log("메인 씬시작");

        SetStage("1-1");    //나중에 유저 테이블에 저장되어 있는 마지막으로 플레이하던 스테이지 받아서 넣어줄 예정

        SetPlayer("ch001"); //이것도 유저 테이블에서 유저가 장착 중인 캐릭터 받아서 넣어줄 예정

        goldText = mainUiPanel.transform.Find("UpSide_Panel/Goods_Panel/Gold_Image/Gold_Text").GetComponent<TMP_Text>();
        gemText = mainUiPanel.transform.Find("UpSide_Panel/Goods_Panel/Gem_Image/Gem_Text").GetComponent<TMP_Text>();
    }
    public void SetPlayer(string charName)
    {
        if (playerCharacter != null)
            Destroy(playerCharacter);

        var _playerCharacter = Resources.Load<GameObject>($"Player/{charName}");   //플레이어 캐릭터 세팅
        playerCharacter = Instantiate(_playerCharacter, transform);
        playerCharacter.transform.position = playerSpawnPoint.position;
        playerCharacter.AddComponent<Player>();
        playerCharacter.AddComponent<PlayerController>();
        IsPlayer = true;
    }
    public void SetStage(string stageName)
    {
        mapName = MonsterSpawnTemplate[stageName][(int)MonsterSpawnTemplate_.MapImage];  //맵세팅
        monsterName = MonsterSpawnTemplate[stageName][(int)MonsterSpawnTemplate_.Monster];   //몬스터 세팅
        bossName = MonsterSpawnTemplate[stageName][(int)MonsterSpawnTemplate_.Boss]; //보스몬스터 세팅

        mapSprite = Resources.Load<Sprite>($"Sprite/{mapName}"); //맵 세팅
        map1.sprite = mapSprite;
        map2.sprite = mapSprite;
    }
    private void InitUIfromDB()
    {
        Debug.Log("InitUIfromDB");
        goldText.text = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.Gold).ToString();
        gemText.text = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.Gem).ToString();
    }

    private void EnterBossStage()
    {
        if (isWaveClear)
        {
            Debug.Log("보스방 입장");
            IsWaveClear = false;

            float fadeInTime = 1.0f;
            float delayTime = 1.0f;

            fadeImage = Instantiate(fadeImage, upSidePanel.transform);
            //fadeImage.transform.SetAsFirstSibling();
            fadeImage.DOFade(1f, fadeInTime).OnComplete(() => Invoke("VersusSetting", delayTime));
        }
    }

    private void VersusSetting()
    {
        int targetLayer = LayerMask.NameToLayer("Over UI");

        vsImage = Instantiate(vsImage, upSidePanel.transform);

        var player = Resources.Load<GameObject>("Player/ch001");
        playerPref = Instantiate(player, playerPosition);
        playerPref.transform.position = playerPosition.position;
        ChangeLayer(playerPref, targetLayer);

        var boss = Resources.Load<GameObject>($"Monster/{bossName}");
        bossPref = Instantiate(boss, bossPosition);
        bossPref.transform.position = bossPosition.position;
        ChangeLayer(bossPref, targetLayer);

        StartCoroutine(BossBattle());
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

    private IEnumerator BossBattle()
    {
        float delayTime = 2.0f;

        yield return new WaitForSeconds(delayTime);

        Destroy(bossPref);
        Destroy(playerPref);
        Destroy(vsImage.gameObject);
        Destroy(fadeImage.gameObject);

        spawner.SpawnBossMonster();
    }

    private void StageClear()
    {
        float durationTime = 1.0f;
        float delayTime = 3.0f;

        victoryText = Instantiate(victoryText, upSidePanel.transform);

        Color color = victoryText.color;    //알파값 초기화
        color.a = 0f;
        victoryText.color = color;

        victoryText.DOFade(1f, durationTime).OnComplete(() =>
        victoryText.DOFade(0f, durationTime).OnComplete(() => StageClear2()));
    }
    private void StageClear2()
    {
        float durationTime = 1.0f;
        float delayTime = 3.0f;
        float movingTime = 3.0f;

        fadeImage = Instantiate(fadeImage, upSidePanel.transform);
        fadeImage.DOFade(1f, durationTime).OnComplete(() => Invoke("VersusSetting", delayTime));

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
