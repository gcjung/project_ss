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
    [HideInInspector] public string playerName;    //임시 테스트용
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
    public static bool IsPlayer { get; private set; } = false;


    [SerializeField] private GameObject mainUiPanel;
    private TMP_Text goldText;
    private TMP_Text gemText;
    private RectTransform category1_UI;
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

        Debug.Log("메인 씬시작");

        stageId = 1;
        SetStage(stageId);    //나중에 유저 테이블에 저장되어 있는 마지막으로 플레이하던 스테이지 받아서 넣어줄 예정

        playerName = "ch001";
        SetPlayer(playerName); //이것도 유저 테이블에서 유저가 장착 중인 캐릭터 받아서 넣어줄 예정

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
    public void SetPlayer(string charName)
    {
        if (playerCharacter != null)
            Destroy(playerCharacter.gameObject);

        var _playerCharacter = Resources.Load<GameObject>($"Player/{charName}");   //플레이어 캐릭터 세팅
        playerCharacter = Instantiate(_playerCharacter, transform);
        playerCharacter.transform.position = playerSpawnPoint.position;
        playerCharacter.AddComponent<Player>();
        playerCharacter.AddComponent<PlayerController>();
        IsPlayer = true;
    }
    public void SetStage(int stageId)
    {
        mapName = StageTemplate[stageId.ToString()][(int)StageTemplate_.MapImage];  //맵세팅
        monsterId = int.Parse(StageTemplate[stageId.ToString()][(int)StageTemplate_.Monster]); //몬스터 세팅
        bossId = int.Parse(StageTemplate[stageId.ToString()][(int)StageTemplate_.Boss]); //보스몬스터 세팅

        mapSprite = Resources.Load<Sprite>($"Sprite/{mapName}"); //맵 세팅
        map1.sprite = mapSprite;
        map2.sprite = mapSprite;
    }
    private void InitUIfromDB()
    {
        Debug.Log("InitUIfromDB");

        double gold = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.Gold);
        double gem = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.Gem);

        goldText.text = Util.BigNumCalculate(gold);
        gemText.text = Util.BigNumCalculate(gem);
    }

    private void EnterBossRoom()
    {
        if (isWaveClear)
        {
            Debug.Log("보스방 입장");
            IsWaveClear = false;

            float fadeInTime = 1.0f;
            float delayTime = 1.0f;

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

        string bossName = MonsterTemplate[bossId.ToString()][(int)MonsterTemplate_.Name];
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

        Color color = _victoryText.color;    //알파값 0으로 초기화
        color.a = 0f;
        _victoryText.color = color;

        _victoryText.DOFade(1f, fadeTime).OnComplete(() =>
        _victoryText.DOFade(0f, fadeTime).OnComplete(() => StageClear2()));
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

        SetPlayer(playerName);
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
        }
    }

    void OpenUI_Category1(GameObject clickButton)
    {
        if (category1_UI == null)
        {
            category1_UI = CommonFuntion.GetPrefab("UI/Category1", popupUI_0.transform).GetComponent<RectTransform>();
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
            double currentGold = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.Gold);

            //goldText.text = $"{(currentGold + getGold)}";
            goldText.text = Util.BigNumCalculate(currentGold + getGold);
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
}
