using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using static GameDataManager;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using System.Linq;

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
    private Player playerCharacter;   

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
    [HideInInspector] public int heroId;    //임시 테스트용
    [HideInInspector] public string heroName;
    [HideInInspector] public int stageId;
    [HideInInspector] public string mapName;
    [HideInInspector] public int monsterId;
    [HideInInspector] public int bossId;

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
    public bool IsPlayer { get; private set; } = false;

    //스탯 적용 테스트 중
    [HideInInspector] public int attackLevel;
    [HideInInspector] public int attackSpeedLevel;
    [HideInInspector] public int criticalLevel;
    [HideInInspector] public int hpLevel;
    [HideInInspector] public float attack_ratio;
    [HideInInspector] public float attackSpeed_ratio;
    [HideInInspector] public float critical_ratio;
    [HideInInspector] public float hp_ratio;


    [SerializeField] private GameObject mainUiPanel;
    private TMP_Text goldText;
    private TMP_Text gemText;
    private RectTransform category1_UI;
    private RectTransform category2_UI; //장비
    private GameObject popupUI_0 = null;
    private GameObject popupUI_1 = null;
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

        spawner = Util.FindChildComponent<MonsterSpawner>(transform);
    }

    private void Init()
    {
        if (instance == null)
            instance = this;

        Debug.Log("메인씬 시작");

        stageId = (int)GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.CurrentStageId, 1);
        SetStage(stageId);  //스테이지 세팅

        heroId = (int)GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.CurrentHeroId, 1);
        SetPlayer(heroId);  //영웅 세팅      

        goldText = mainUiPanel.transform.Find("UpSide_Panel/Goods_Panel/Gold_Image/Gold_Text").GetComponent<TMP_Text>();
        gemText = mainUiPanel.transform.Find("UpSide_Panel/Goods_Panel/Gem_Image/Gem_Text").GetComponent<TMP_Text>();

        popupUI_0 = UIManager.instance.transform.Find("PopupUI_0").gameObject;
        popupUI_1 = UIManager.instance.transform.Find("PopupUI_1").gameObject;

        // 바텀 카테고리 버튼
        Transform bottomCategortButton = mainUiPanel.transform.Find("DownSide_Panel/Category_Image");
        int categoryButtonCount = bottomCategortButton.childCount;
        for (int i = 0; i < categoryButtonCount; i++)
        {
            int index = i;

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerUp;
            entry.callback.AddListener((eventData) => OnClickCategory(index, eventData.selectedObject));

            EventTrigger button = bottomCategortButton.GetChild(i).GetComponent<EventTrigger>();
            button.triggers.Add(entry);
        }

    }
    public void SetPlayer(int heroId)
    {
        Debug.Log($"현재 영웅ID : {heroId}");

        if (playerCharacter != null)
            Destroy(playerCharacter.gameObject);

        heroName = HeroTemplate[heroId.ToString()][(int)HeroTemplate_.Name];

        var _playerCharacter = Resources.Load<Player>($"Player/{heroName}");   //플레이어 캐릭터 세팅
        playerCharacter = Instantiate(_playerCharacter, transform);
        playerCharacter.transform.position = playerSpawnPoint.position;
        playerCharacter.AddComponent<PlayerController>();
        playerCharacter.SetHeroStatus(heroId);

        IsPlayer = true;

        UpdateStatusLevel();    //영웅 스탯 세팅
    }
    public void SetStage(int stageId)
    {
        Debug.Log($"현재 스테이지ID : {stageId}");

        mapName = StageTemplate[stageId.ToString()][(int)StageTemplate_.MapImage];  //맵이미지 세팅
        monsterId = int.Parse(StageTemplate[stageId.ToString()][(int)StageTemplate_.Monster]); //몬스터 세팅
        bossId = int.Parse(StageTemplate[stageId.ToString()][(int)StageTemplate_.Boss]); //보스몬스터 세팅

        mapSprite = Resources.Load<Sprite>($"Sprite/{mapName}"); //맵 세팅
        map1.sprite = mapSprite;
        map2.sprite = mapSprite;
    }

    public void UpdateStatusLevel(string statusName = "", int level = 1)    //임시 메서드(존나게 맘에 안듬)
    {
        switch (statusName)
        {
            case "AttackLevel":
                attackLevel = level;
                Debug.Log($"AttackLevel : {attackLevel}");
                break;
            case "AttackSpeedLevel":
                attackSpeedLevel = level;
                Debug.Log($"AttackSpeedLevel : {attackSpeedLevel}");
                break;
            case "CriticalLevel":
                criticalLevel = level;
                Debug.Log($"CriticalLevel : {criticalLevel}");
                break;
            case "HpLevel":
                hpLevel = level;
                Debug.Log($"HpLevel : {hpLevel}");
                break;
            default:    //최초 1회 실행으로 모든 스탯값 세팅
                attackLevel = (int)GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.AttackLevel, 1);
                attackSpeedLevel = (int)GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.AttackSpeedLevel, 1);
                criticalLevel = (int)GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.CriticalLevel, 1);
                hpLevel = (int)GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.HpLevel, 1);

                attack_ratio = float.Parse(StatusTemplate["Attack"][(int)StatusTemplate_.Value_Calc]);
                attackSpeed_ratio = float.Parse(StatusTemplate["AttackSpeed"][(int)StatusTemplate_.Value_Calc]);
                critical_ratio = float.Parse(StatusTemplate["Critical"][(int)StatusTemplate_.Value_Calc]);
                hp_ratio = float.Parse(StatusTemplate["Hp"][(int)StatusTemplate_.Value_Calc]);

                Debug.Log($"All Status Updated");
                break;
        }

        if (IsPlayer)
            playerCharacter.UpdateHeroStatus(statusName);
    }
    private void InitUIfromDB()
    {
        Debug.Log("InitUIfromDB");

        double gold = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.Gold);
        double gem = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.Gem);

        goldText.text = $"{Util.BigNumCalculate(gold)} G";
        gemText.text = Util.BigNumCalculate(gem);
    }

    private void EnterBossRoom()
    {
        if (isWaveClear)
        {
            Debug.Log("보스방 입장");
            IsWaveClear = false;

            float fadeInTime = 1.0f;

            var _fadeImage = Instantiate(fadeImage, upSidePanel.transform);
            //fadeImage.transform.SetAsFirstSibling();
            _fadeImage.DOFade(1f, fadeInTime).OnComplete(() =>
            {
                Destroy(_fadeImage);
                StartCoroutine(BossBattle());
            });
        }
    }

    private IEnumerator BossBattle()
    {
        float delayTime = 2.0f;
        int targetLayer = LayerMask.NameToLayer("Over UI");
        var _vsImage = Instantiate(vsImage, upSidePanel.transform);

        playerPref = Resources.Load<GameObject>($"Player/{heroName}");
        var _playerPref = Instantiate(playerPref, playerPosition);
        _playerPref.transform.position = playerPosition.position;
        Util.ChangeLayer(_playerPref, targetLayer);

        string bossName = MonsterTemplate[bossId.ToString()][(int)MonsterTemplate_.Name];
        bossPref = Resources.Load<GameObject>($"Monster/{bossName}");
        var _bossPref = Instantiate(bossPref, bossPosition);
        _bossPref.transform.position = bossPosition.position;
        Util.ChangeLayer(_bossPref, targetLayer);

        yield return new WaitForSeconds(delayTime);

        Destroy(_bossPref);
        Destroy(_playerPref);
        Destroy(_vsImage.gameObject);

        spawner.SpawnBossMonster();
    }


    private void StageClear()
    {
        float fadeTime = 1.0f;

        var _victoryText = Instantiate(victoryText, upSidePanel.transform);

        Color color = _victoryText.color;    //알파값 0으로 초기화
        color.a = 0f;
        _victoryText.color = color;

        _victoryText.DOFade(1f, fadeTime).OnComplete(() =>
        _victoryText.DOFade(0f, fadeTime).OnComplete(() =>
        {
            StageClear2();
            Destroy(_victoryText);
        }));
    }
    private void StageClear2()
    {
        float durationTime = 3.0f;
        float movingTime = 3.0f;

        var _fadeImage = Instantiate(fadeImage, upSidePanel.transform);
        _fadeImage.DOFade(1f, durationTime).OnComplete(() => Destroy(_fadeImage));

        playerCharacter.transform.DOMove(movePoint.position, movingTime).OnComplete(() => StageClear3());
    }
    private void StageClear3()
    {
        stageId += 1;
        
        if(StageTemplate.Count < stageId)
        {
            stageId -= 1;
        }
        GlobalManager.Instance.DBManager.UpdateUserData("CurrentStageId", stageId);

        playerCharacter.transform.position = playerSpawnPoint.position;
        SetStage(stageId);
        spawner.SetMonster();
    }

    int invisiblePosY = -1700;
    private void OnClickCategory(int categoryType, GameObject onClickButton)
    {
        switch (categoryType)
        {
            case 0:
                OpenUI_Category1(onClickButton);
                break;
            case 1:
                OpenUI_Category2(onClickButton);
                break;

            default:
                break;
        }
    }

    void OpenUI_Category1(GameObject clickButton)
    {
        if (category1_UI == null)
        {
            category1_UI = CommonFuntion.GetPrefab("Category1", popupUI_0.transform).GetComponent<RectTransform>();
            //category1_UI.Find("Type1_Character/Character/Level_Text").GetComponent<TMP_Text>().font = "LV 1";
            category1_UI.DOAnchorPosY(0, 0.3f).SetEase(Ease.OutExpo);
        }
        else
        {
            bool active = category1_UI.gameObject.activeSelf;

            if (active)
            {
                category1_UI.anchoredPosition = new Vector2(0, invisiblePosY);
            }
            else
            {
                category1_UI.DOAnchorPosY(0, 0.3f).SetEase(Ease.OutExpo);
            }

            category1_UI.gameObject.SetActive(!active);
        }

        clickButton.transform.Find("Text").gameObject.SetActive(!category1_UI.gameObject.activeSelf);
        clickButton.transform.Find("CloseImage").gameObject.SetActive(category1_UI.gameObject.activeSelf);

        Transform parent = category1_UI.Find("BottomMenu");
        for (int i = 0; i < parent.childCount; i++)
        {
            int index = i;
            Button button = parent.GetChild(i).GetComponent<Button>();
            button.onClick.RemoveAllListeners();

            switch (index)
            {
                case 0:
                    button.onClick.AddListener(ShowUI_Character);
                    break;
                case 1:
                    button.onClick.AddListener(ShowUI_Skill);
                    break;
            }
        }
    }

    private void OpenUI_Category2(GameObject clickButton)
    {
        //장비 UI
        if (category2_UI == null)
        {
            category2_UI = CommonFuntion.GetPrefab("UI/Item_Panel", popupUI_0.transform).GetComponent<RectTransform>();
            category2_UI.DOAnchorPosY(0, 0.3f).SetEase(Ease.OutExpo);
        }
        else
        {
            bool active = category2_UI.gameObject.activeSelf;

            if (active)
            {
                category2_UI.anchoredPosition = new Vector2(0, invisiblePosY);
            }
            else
            {
                category2_UI.DOAnchorPosY(0, 0.3f).SetEase(Ease.OutExpo);
            }

            category2_UI.gameObject.SetActive(!active);
        }
    }

    void ShowUI_Character()
    {
        Debug.Log("ShowUI_Character");
        category1_UI.Find("Type1_Character").gameObject.SetActive(true);
        category1_UI.Find("Type2_Skill").gameObject.SetActive(false);
    }

    void ShowUI_Skill()
    {
        Debug.Log("ShowUI_Skill");
        category1_UI.Find("Type2_Skill").gameObject.SetActive(true);
        category1_UI.Find("Type1_Character").gameObject.SetActive(false);
    }

    public void GetGoods(double getGold = 0, double getGem = 0)
    {
        if (getGold > 0)
        {
            Debug.Log($"{Util.BigNumCalculate(getGold)}G를 획득");

            double currentGold = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.Gold);

            //goldText.text = $"{(currentGold + getGold)}";
            goldText.text = $"{Util.BigNumCalculate(currentGold + getGold)} G";
            GlobalManager.Instance.DBManager.UpdateUserData(UserDoubleDataType.Gold.ToString(), currentGold + getGold);
        }

        if (getGem > 0)
        {
            double currentGem = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.Gem);

            //gemText.text = $"{(currentGem + getGem)}";
            gemText.text = Util.BigNumCalculate(currentGem + getGem);
            GlobalManager.Instance.DBManager.UpdateUserData(UserDoubleDataType.Gem.ToString(), currentGem + getGem);
        }
    }

    public bool UseGolds(double usedGold)
    {
        double currentGold = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.Gold);

        if (currentGold - usedGold >= 0)
        {
            Debug.Log($"{Util.BigNumCalculate(usedGold)}Gold를 사용");
            goldText.text = $"{Util.BigNumCalculate(currentGold - usedGold)} G";
            GlobalManager.Instance.DBManager.UpdateUserData(UserDoubleDataType.Gold.ToString(), currentGold - usedGold);

            return true;
        }
        else
        {
            Debug.LogError("골드가 부족합니다");

            return false;
        }
    }

    public bool UseGems(double usedGem)
    {
        double currentGem = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.Gem);

        if (currentGem - usedGem >= 0)
        {
            Debug.Log($"{usedGem}Gem을 사용");
            gemText.text = $"{Util.BigNumCalculate(currentGem - usedGem)}";
            GlobalManager.Instance.DBManager.UpdateUserData(UserDoubleDataType.Gem.ToString(), currentGem - usedGem);

            return true;
        }
        else
        {
            Debug.LogError("젬이 부족합니다");

            return false;
        }
    }
}
