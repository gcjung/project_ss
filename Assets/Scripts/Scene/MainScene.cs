using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using static GameDataManager;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEditor;
//using static UnityEditor.Progress;
//using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

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
    public UpSidePanel upSidePanel;
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
    [SerializeField] private Transform playerPosition;
    [SerializeField] private Transform bossPosition;
    private GameObject playerPref;
    private GameObject bossPref;

    [Header("Stage Clear")]
    [SerializeField] private Transform movePoint;

    [Header("Total Damage Text")]
    [SerializeField] private TextMeshProUGUI totalDamageText;

    [Header("Stage Text")]
    private GameObject stageText;
    private MonsterSpawner spawner; //스테이지 세팅용

    [HideInInspector] public double heroId;
    [HideInInspector] public string heroName;
    [HideInInspector] public int stageId;
    [HideInInspector] public string stageName;
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
                isWaveClear = value;
                EnterBossRoom();
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
    private bool isPlayerDead = false;
    public bool IsPlayerDead
    {
        get { return isPlayerDead; }
        set
        {
            if (value)
            {
                isPlayerDead = value;
                StartCoroutine(RestartStage());
            }
        }
    }
    public StageState CurrentStageState { get; private set; }
    public bool IsPlayer { get; private set; } = false;

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

    private RectTransform category1_UI = null;  // 캐릭터, 장비
    private RectTransform category2_UI = null;  //장비
    private Transform SkillDetail_Popup = null;

    private GameObject popupUI_0 = null;    // UI를 띄워줄 캔버스 레이어
    private GameObject popupUI_1 = null;    // UI를 띄워줄 캔버스 레이어
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

    SkillController skillController = null;
    private Image[] lobbyEquipSkill_Image = null;
    private void Init()
    {
        if (instance == null)
            instance = this;

        Debug.Log("메인씬 시작");

        SetCurrentStageState(StageState.InfinityWave);

        stageId = (int)GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.CurrentStageId, 1);
        SetStage(stageId);  //스테이지 세팅

        heroId = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.CurrentHeroId, 1);
        SetPlayer(heroId);  //영웅 세팅      

        goldText = mainUiPanel.transform.Find("UpSide_Panel/Goods_Panel/Gold_Image/Gold_Text").GetComponent<TMP_Text>();
        gemText = mainUiPanel.transform.Find("UpSide_Panel/Goods_Panel/Gem_Image/Gem_Text").GetComponent<TMP_Text>();

        popupUI_0 = UIManager.instance.transform.Find("PopupUI_0").gameObject;
        popupUI_1 = UIManager.instance.transform.Find("PopupUI_1").gameObject;

        skillController = FindObjectOfType<SkillController>();
        skillController.Init(playerCharacter);

        var skillPanel = mainUiPanel.transform.Find("UpSide_Panel/Skill_Panel");
        // 로비 스킬UI 버튼
        lobbyEquipSkill_Image = new Image[skillPanel.childCount - 1];
        for (int i = 0; i < skillPanel.childCount - 1; i++)
        {
            if(i == 0)
            {
                skillPanel.GetChild(0).GetComponent<Button>().onClick.AddListener(() => { OnClick_AutoSkillBtn(); });
            }

            int index = i;
            lobbyEquipSkill_Image[i] = skillPanel.GetChild(i + 1).GetComponent<Image>();
            skillController.equippedSkillInfo[i].lobbySkillSlot = skillPanel.GetChild(i + 1).GetComponent<Image>();

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerUp;
            entry.callback.AddListener((eventData) =>
            {
                OnClickUseSkill(index);
            });

            lobbyEquipSkill_Image[i].GetComponent<EventTrigger>().triggers.Add(entry);
        }

        // 바텀 카테고리 버튼
        Transform bottomCategory = mainUiPanel.transform.Find("DownSide_Panel/Category_Image");
        int categoryButtonCount = bottomCategory.childCount;
        for (int i = 0; i < categoryButtonCount; i++)
        {
            int index = i;

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerUp;
            entry.callback.AddListener((eventData) => OnClickCategory(index, eventData.selectedObject));

            EventTrigger button = bottomCategory.GetChild(i).GetComponent<EventTrigger>();
            button.triggers.Add(entry);
        }

    }
    public void SetPlayer(double heroId)
    {
        Debug.Log($"현재 영웅ID : {heroId}");

        if (playerCharacter != null)
            Destroy(playerCharacter.gameObject);

        heroName = HeroTemplate[heroId.ToString()][(int)HeroTemplate_.Name];

        var _playerCharacter = Resources.Load<Player>($"Player/{heroName}");   //플레이어 캐릭터 세팅
        playerCharacter = Instantiate(_playerCharacter, transform);
        playerCharacter.transform.position = playerSpawnPoint.position;
        playerCharacter.AddComponent<PlayerController>();

        playerCharacter.SetHeroStatus(heroId);  //영웅 기본 스탯 세팅

        IsPlayer = true;

        UpdateStatusLevel();    //최초 1회 스탯 레벨 업데이트

        var hpBar = CommonFunction.GetPrefabInstance("Slider_HealthBar_Hero", upSidePanel.transform);   //체력바 세팅
        hpBar.GetComponent<HpSlider>().SetTarget(playerCharacter.gameObject);
    }
    public void SetStage(int stageId)
    {
        //Debug.Log($"현재 스테이지ID : {stageId}");

        if (stageText != null)
            Destroy(stageText.gameObject);

        stageName = StageTemplate[stageId.ToString()][(int)StageTemplate_.Stage];
        stageText = CommonFunction.GetPrefabInstance("StageName_Text", upSidePanel.transform);
        stageText.GetComponent<TextMeshProUGUI>().text = stageName;

        mapName = StageTemplate[stageId.ToString()][(int)StageTemplate_.MapImage];  //맵이미지 세팅
        monsterId = int.Parse(StageTemplate[stageId.ToString()][(int)StageTemplate_.Monster]); //몬스터 세팅
        bossId = int.Parse(StageTemplate[stageId.ToString()][(int)StageTemplate_.Boss]); //보스몬스터 세팅

        mapSprite = Resources.Load<Sprite>($"Sprite/{mapName}"); //맵 세팅
        map1.sprite = mapSprite;
        map2.sprite = mapSprite;
    }
    public void SetLobbySkill(string[] equippedSkill)
    {
        for (int i = 0; i < equippedSkill.Length; i++)
        {
            if (string.IsNullOrEmpty(equippedSkill[i]))
                continue;

            string icon = SkillTemplate[equippedSkill[i]][(int)SkillTemplate_.Icon];

            string[] iconDatas = icon.Split('/');
            string spriteName = iconDatas[0];
            string atlasName = iconDatas[1];
            lobbyEquipSkill_Image[i].sprite = CommonFunction.GetSprite_Atlas(spriteName, atlasName);
        }
    }
    void OnClick_AutoSkillBtn()
    {
        var isOn = GlobalManager.Instance.DBManager.GetUserBoolData(UserBoolDataType.isOnAutoSkill);
        SetLobbyAutoSkill(!isOn);    // 버튼 눌렀으니깐 반대로
        skillController.OnStartAutoSkill(!isOn);

        GlobalManager.Instance.DBManager.UpdateUserData(UserBoolDataType.isOnAutoSkill, !isOn);
    }
    public void SetLobbyAutoSkill(bool isOn)
    {
        var autoSkillBtn = mainUiPanel.transform.Find("UpSide_Panel/Skill_Panel/Auto_Image");
        
        if (isOn)
        {
            autoSkillBtn.Find("Text").GetComponent<TMP_Text>().text = "Auto\nON";
            autoSkillBtn.Find("loop").gameObject.SetActive(true);  
        }
        else
        {
            autoSkillBtn.Find("Text").GetComponent<TMP_Text>().text = "Auto\nOFF";
            autoSkillBtn.Find("loop").gameObject.SetActive(false);
        }
    }

    public void UpdateStatusLevel(string statusName = "", int level = 1)
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

        totalDamageText.text = $"총 공격력 : {Util.BigNumCalculate(playerCharacter.TotalAttack)}";
    }
    private void InitUIfromDB()
    {
        double gold = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.Gold);
        double gem = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.Gem);

        goldText.text = $"{Util.BigNumCalculate(gold)} G";
        gemText.text = Util.BigNumCalculate(gem);

        var isOn = GlobalManager.Instance.DBManager.GetUserBoolData(UserBoolDataType.isOnAutoSkill);
        SetLobbyAutoSkill(isOn);
        skillController.OnStartAutoSkill(isOn);

        var equippedSkill = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.EquippedSkill, "@@@@@").Split('@');
        SetLobbySkill(equippedSkill);    // 스킬 세팅
    }

    private void EnterBossRoom()
    {
        if (isWaveClear)
        {
            IsWaveClear = false;

            SetCurrentStageState(StageState.BossRoom);

            float fadeInTime = 1.0f;

            var _fadeImage = Instantiate(fadeImage, upSidePanel.transform);
            //fadeImage.transform.SetAsFirstSibling();
            _fadeImage.DOFade(1f, fadeInTime).OnComplete(() =>
            {
                Destroy(_fadeImage.gameObject);
                StartCoroutine(BossBattle());
            });
        }
    }

    private IEnumerator BossBattle()
    {       
        Vector3 playerScale = new Vector3(3, 3, 1);
        Vector3 bossScale = new Vector3(3, 3, -1);
        Vector3 playerPos = new Vector3(-3, -1, 1);
        Vector3 bossPos = new Vector3(3, -1, 1);

        float delayTime = 3.0f;
        int targetLayer = LayerMask.NameToLayer("Over UI");
        string bossName = MonsterTemplate[bossId.ToString()][(int)MonsterTemplate_.Name];

        var rawImage = CommonFunction.GetPrefabInstance("VS_RawImage", upSidePanel.transform);
        var _vsSprite = CommonFunction.GetPrefabInstance("VS_Sprite", transform);
        GameObject vsText = null;

        playerPref = Resources.Load<GameObject>($"Player/{heroName}");
        var _playerPref = Instantiate(playerPref, playerPosition);
        _playerPref.transform.position = playerPosition.position;
        _playerPref.transform.localScale = playerScale;
        Util.ChangeLayer(_playerPref, targetLayer);

        bossPref = Resources.Load<GameObject>($"Monster/{bossName}");
        var _bossPref = Instantiate(bossPref, bossPosition);
        _bossPref.transform.position = bossPosition.position;
        _bossPref.transform.localScale = bossScale;
        Util.ChangeLayer(_bossPref, targetLayer);

        _playerPref.transform.DOMove(playerPos, 1.0f).SetEase(Ease.OutQuad);    //플레이어 캐릭터 이동
        _bossPref.transform.DOMove(bossPos, 1.0f).SetEase(Ease.OutQuad).OnComplete(() =>    //보스 캐릭터 이동
        {
            vsText = CommonFunction.GetPrefabInstance("VS_Text", upSidePanel.transform);
            DOTween.Sequence()
            .OnStart(() => { vsText.transform.localScale = Vector3.zero; })
            .Append(vsText.transform.DOScale(3, 0.5f).SetEase(Ease.OutBack));
        });

        yield return new WaitForSeconds(delayTime);

        {//연출요소들 파괴
            Destroy(rawImage.gameObject);
            Destroy(_bossPref);
            Destroy(_playerPref);
            Destroy(_vsSprite.gameObject);
            Destroy(vsText);
        }
        
        spawner.SpawnBossMonster();
    }

    private void StageClear()
    {
        float fadeTime = 1.0f;

        var _victoryText = CommonFunction.GetPrefabInstance("Victory_Text", upSidePanel.transform);
        TextMeshProUGUI tmp = _victoryText.GetComponent<TextMeshProUGUI>();
        tmp.text = "Victory";

        Color color = tmp.color;    //알파값 0으로 초기화
        color.a = 0f;
        tmp.color = color;

        tmp.DOFade(1f, fadeTime).OnComplete(() =>
        tmp.DOFade(0f, fadeTime).OnComplete(() =>
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
        _fadeImage.DOFade(1f, durationTime).OnComplete(() => Destroy(_fadeImage.gameObject));

        playerCharacter.transform.DOMove(movePoint.position, movingTime).OnComplete(() => StageClear3());
    }
    private void StageClear3()
    {
        stageId += 1;

        if (StageTemplate.Count < stageId)
        {
            stageId -= 1;
        }
        GlobalManager.Instance.DBManager.UpdateUserData("CurrentStageId", stageId);

        playerCharacter.transform.position = playerSpawnPoint.position;
        SetStage(stageId);
        spawner.SetMonster();
    }

    public IEnumerator RestartStage()
    {
        isPlayerDead = false;

        switch (CurrentStageState)
        {
            case StageState.NormalWave:
                break;
            case StageState.BossRoom:
                SetCurrentStageState(StageState.InfinityWave);
                break;
            case StageState.InfinityWave:
                break;
        }

        float durationTime = 1.0f;
        bool fadeFinish = false;

        var _fadeImage = Instantiate(fadeImage, upSidePanel.transform);
        _fadeImage.DOFade(1f, durationTime).OnComplete(() =>
        {
            Destroy(_fadeImage);
            fadeFinish = true;
        });

        yield return CommonIEnumerator.IEWaitUntil(
          predicate: () => { return fadeFinish; },
          onFinish: () =>
          {
              SetPlayer(heroId);
              SetStage(stageId);
              spawner.SetMonster();
          }
       );
    }


    private void OnClickCategory(int categoryType, GameObject button)
    {
        switch (categoryType)
        {
            case 0:
                OpenUI_Category1(button);
                break;
            case 1:
                OpenUI_Category2(button);
                break;
            case 4:
                OpenUI_Category4(button);
                break;

            default:
                break;
        }
    }
    const int invisiblePosY = -1700;
    void OpenUI_Category1(GameObject clickButton)
    {
        if (category1_UI == null)
        {
            category1_UI = CommonFunction.GetPrefabInstance("Category1", popupUI_0.transform).GetComponent<RectTransform>();
            category1_UI.DOAnchorPosY(0, 0.3f).SetEase(Ease.OutExpo);

            ShowUI_Character(true);
            ShowUI_Skill(true);
        }
        else
        {
            bool active = category1_UI.gameObject.activeSelf;

            if (active)
                category1_UI.anchoredPosition = new Vector2(0, invisiblePosY);         
            else
                category1_UI.DOAnchorPosY(0, 0.3f).SetEase(Ease.OutExpo);

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
                    button.onClick.AddListener(() => ShowUI_Character());
                    break;
                case 1:
                    button.onClick.AddListener(() => ShowUI_Skill());
                    break;
            }
        }
    }

    private void OpenUI_Category2(GameObject clickButton)
    {
        //장비 UI
        if (category2_UI == null)
        {
            category2_UI = CommonFunction.GetPrefabInstance("UI/Item_Panel", popupUI_0.transform).GetComponent<RectTransform>();
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
    private HeroSlot selectedHeroSlot;
    private Image disabledImage;
    void ShowUI_Character(bool isFirst = false)
    {
        Debug.Log("ShowUI_Character");

        category1_UI.Find("Type1_Character").gameObject.SetActive(true);
        category1_UI.Find("Type2_Skill").gameObject.SetActive(false);

        if (isFirst)        // 최초 1회만 실행
        {
            string spriteName = $"{heroName}_Image";
            string atlasName = "HeroImageAtlas";

            disabledImage = category1_UI.Find("Type1_Character/Equip_Button/Disabled_Image").GetComponent<Image>();

            Image heroImage = category1_UI.Find("Type1_Character/Character/Character_Image").GetComponent<Image>();
            heroImage.sprite = CommonFunction.GetSprite_Atlas(spriteName, atlasName);

            TextMeshProUGUI heroNameText = category1_UI.Find("Type1_Character/Character/CharacterName_Text").GetComponent<TextMeshProUGUI>();
            heroNameText.text = heroName;

            Button equipButton = category1_UI.Find("Type1_Character/Equip_Button").GetComponent<Button>();
            equipButton.onClick.AddListener(() =>
            {
                ChangeCurrentHero();

                //현재 설정된 영웅의 이미지,이름 업데이트
                spriteName = $"{heroName}_Image";
                heroImage.sprite = CommonFunction.GetSprite_Atlas(spriteName, atlasName);
                heroNameText.text = heroName;
            });           

            Transform grid = category1_UI.Find("Type1_Character/CharacterEffect/Scroll View/Viewport/Grid");
            Util.InitGrid(grid);

            foreach (var hero in HeroTemplate.OrderBy(x => x.Value[(int)HeroTemplate_.HeroId]))
            {
                string id = hero.Value[(int)HeroTemplate_.HeroId];

                GameObject heroSlot = CommonFunction.GetPrefabInstance("HeroSlot", grid);
                HeroSlot slot = heroSlot.AddComponent<HeroSlot>();
                slot.Init(id);

                // 영웅 아이콘 선택 시 선택된 슬릇 표시
                heroSlot.GetComponent<Button>().onClick.AddListener(() =>
                {
                    ChangeSelectedSlot(slot);
                });
            }
        }

        //선택된 슬릇 초기화 <<<<<<<<< 수정해야됨
        if (selectedHeroSlot != null)
        {
            selectedHeroSlot.SetSelected(false);
            selectedHeroSlot = null;
            disabledImage.gameObject.SetActive(true);
        }        
    }

    private void ChangeCurrentHero()
    {
        if (CurrentStageState != StageState.BossRoom && selectedHeroSlot != null) //스테이지가 NormalWave 상태일 때만 변경 가능
        {
            if (double.Parse(selectedHeroSlot.heroId) != heroId)    //이미 장착중인 영웅이 아닐 때
            {
                GlobalManager.Instance.DBManager.UpdateUserData("CurrentHeroId", selectedHeroSlot.heroId);
                heroId = double.Parse(selectedHeroSlot.heroId);
                heroName = HeroTemplate[heroId.ToString()][(int)HeroTemplate_.Name];

                StartCoroutine(RestartStage());
            }
            else
            {
                CommonFunction.CreateNotification("이미 장착 중인 영웅임", popupUI_1.transform);
            }            
        }
        else
        {
            CommonFunction.CreateNotification("현재 영웅을 교체할 수 없음", popupUI_1.transform);
        }
    }

    private void ChangeSelectedSlot(HeroSlot slot)
    {
        if (selectedHeroSlot != null)
        {
            selectedHeroSlot.SetSelected(false);
        }

        // 현재 클릭한 슬롯을 선택된 슬롯으로 설정
        selectedHeroSlot = slot;

        // 선택된 슬롯의 상태를 변경하여 체크표시를 활성화
        selectedHeroSlot.SetSelected(true);
        disabledImage.gameObject.SetActive(false);
    }


    const int maxEquipSkillCount = 6;
    Transform[] equippedSkillSlot = null;
    void ShowUI_Skill(bool isFirst = false)
    {
        var equippedSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.EquippedSkill).Split('@');

        category1_UI.Find("Type2_Skill").gameObject.SetActive(true);
        category1_UI.Find("Type1_Character").gameObject.SetActive(false);

        if (isFirst)        // 최초 1회만 실행
        {
            SkillSlot[] equipSkillSlot = new SkillSlot[maxEquipSkillCount]; // 장착한 스킬 슬롯데이터

            Transform grid = category1_UI.Find("Type2_Skill/Scroll View/Viewport/Grid");
            Util.InitGrid(grid);
            foreach (var item in SkillTemplate.OrderBy(x => x.Value[(int)SkillTemplate_.Order]))
            {
                string id = item.Value[(int)SkillTemplate_.SkillId];
                string grade = item.Value[(int)SkillTemplate_.Grade];
                string icon = item.Value[(int)SkillTemplate_.Icon];

                GameObject skillslot = CommonFunction.GetPrefabInstance("SkillSlot", grid);
                SkillSlot slot = skillslot.AddComponent<SkillSlot>();
                slot.Init(id);

                // 스킬 아이콘 선택 시 (상세보기)
                skillslot.GetComponent<Button>().onClick.AddListener(() => {
                    OpenUI_SkillDetail(slot, id);
                });

                // 간편 장착/해제 버튼
                skillslot.transform.Find("UpperRightImage").GetComponent<Button>().onClick.AddListener(() => {
                    if (slot.State.HasFlag(SkillSlotState.Equipped))
                        OnClick_UnequipSkill(slot, id);
                    else
                        OnClick_EquipSkill(slot, id);
                });

                for (int i = 0; i < equippedSkillData.Length; i++)
                {
                    if (equippedSkillData[i] == id)
                        equipSkillSlot[i] = slot;
                }
            }

            
            Transform equippedSkillGroup = category1_UI.Find("Type2_Skill/EquippedSkillGroup");
            equippedSkillSlot = new Transform[maxEquipSkillCount];
            for (int i = 0; i < maxEquipSkillCount; i++)
                equippedSkillSlot[i] = equippedSkillGroup.GetChild(i);

            var skillAcquireData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.SkillData).Split('@');

            // 장착중 스킬슬롯
            for (int i = 0; i < equippedSkillData.Length; i++)
            {
                int index = i;
                string skillID = equippedSkillData[i];

                if (!string.IsNullOrEmpty(skillID))  // 스킬 장착중인 경우
                {
                    string[] icon = SkillTemplate[skillID][(int)SkillTemplate_.Icon].Split('/');
                    string spriteName = icon[0];
                    string atlasName = icon[1];
                    
                    equippedSkillSlot[i].Find("Image").GetComponent<Image>().sprite = CommonFunction.GetSprite_Atlas(spriteName, atlasName);
                    equippedSkillSlot[i].Find("Image").GetComponent<Image>().gameObject.SetActive(true);

                    string skillLevel = skillAcquireData[int.Parse(skillID) - 1].Split(",")[0];    // syntax : 스킬레벨,보유개수@
                    equippedSkillSlot[i].transform.Find("Level_Text").GetComponent<TMP_Text>().text = $"LV{skillLevel}";

                    Button skillDetailBtn = equippedSkillSlot[i].GetComponent<Button>();
                    skillDetailBtn.onClick.RemoveAllListeners();
                    skillDetailBtn.onClick.AddListener(() =>
                    {
                        Debug.Log($"i : {i}, index : {index}");
                        OpenUI_SkillDetail(equipSkillSlot[index], skillID);
                    });

                    Button simpleUnequipBtn = equippedSkillSlot[i].transform.Find("Unequip_Button").GetComponent<Button>();
                    simpleUnequipBtn.gameObject.SetActive(true);
                    simpleUnequipBtn.onClick.RemoveAllListeners();
                    simpleUnequipBtn.onClick.AddListener(() =>
                    {
                        Debug.Log($"i : {i}, index : {index}");
                        OnClick_UnequipSkill(equipSkillSlot[index], skillID);
                    });

                }
            }
        }

    }

    void OpenUI_SkillDetail(SkillSlot slot, string skillId)
    {
        if (SkillDetail_Popup == null)
            SkillDetail_Popup = CommonFunction.GetPrefabInstance("SkillDetailPopup", popupUI_1.transform).transform;
        else
            SkillDetail_Popup.gameObject.SetActive(true);

        DOTween.Sequence()
       .OnStart(() => { SkillDetail_Popup.transform.localScale = Vector3.zero; })
       .Append(SkillDetail_Popup.transform.DOScale(1, 0.3f).SetEase(Ease.OutBack));

        //string id = SkillTemplate[skillId][(int)SkillTemplate_.SkillId];
        string skillName = SkillTemplate[skillId][(int)SkillTemplate_.Name];
        string grade = SkillTemplate[skillId][(int)SkillTemplate_.Grade];
        string[] icon = SkillTemplate[skillId][(int)SkillTemplate_.Icon].Split('/');
        string desc = SkillTemplate[skillId][(int)SkillTemplate_.Desc];
        string cooltime = SkillTemplate[skillId][(int)SkillTemplate_.Cooltime];

        string spriteName = icon[0];
        string atlasName = icon[1];

        SkillDetail_Popup.Find("Icon").GetComponent<Image>().sprite = CommonFunction.GetSprite_Atlas(spriteName, atlasName);
        SkillDetail_Popup.Find("SkillName_Text").GetComponent<TMP_Text>().text = skillName;
        SkillDetail_Popup.Find("Grade_Text").GetComponent<TMP_Text>().color = Util.ConvertGradeToColor(grade);
        SkillDetail_Popup.Find("Grade_Text").GetComponent<TMP_Text>().text = grade;
        SkillDetail_Popup.Find("DescBG/Desc_Text").GetComponent<TMP_Text>().text = desc;
        SkillDetail_Popup.Find("DescBG/CoolTime_Text").GetComponent<TMP_Text>().text = $"{cooltime}초";

        int level = slot.CurrentLevel;
        int currentValue = slot.HoldingCount;
        int requireQuantity = slot.TargetValue;

        SkillDetail_Popup.Find("Level_Text").GetComponent<TMP_Text>().text = $"LV. {level}";
        SkillDetail_Popup.Find("Slider/CurrentValue_Text").GetComponent<TMP_Text>().text = $"{currentValue}";
        SkillDetail_Popup.Find("Slider/TargetValue_Text").GetComponent<TMP_Text>().text = $"/ {requireQuantity}";
        SkillDetail_Popup.Find("Slider").GetComponent<Slider>().value = currentValue / (float)requireQuantity;

        Button equipBtn = SkillDetail_Popup.Find("EquipButton").GetComponent<Button>();
        Button upgradeBtn = SkillDetail_Popup.Find("UpgradeButton").GetComponent<Button>();
        if (slot.State == SkillSlotState.Lock)
        {
            SkillDetail_Popup.Find("EquipButton/ButtonImage").GetComponent<Image>().color = Color.gray;// new Color(87f / 255f, 87f / 255f, 87f / 255f, 1);
            SkillDetail_Popup.Find("UpgradeButton/ButtonImage").GetComponent<Image>().color = Color.gray;

            equipBtn.interactable = false;
            upgradeBtn.interactable = false;
        }
        else
        {
            if (slot.State.HasFlag(SkillSlotState.Upgradeable))
            {
                upgradeBtn.interactable = true;
                SkillDetail_Popup.Find("UpgradeButton/ButtonImage").GetComponent<Image>().color = Color.white;
            }
            else
            {
                upgradeBtn.interactable = false;
                SkillDetail_Popup.Find("UpgradeButton/ButtonImage").GetComponent<Image>().color = Color.gray;
            }

            equipBtn.interactable = true;
            SkillDetail_Popup.Find("EquipButton/ButtonImage").GetComponent<Image>().color = Color.white;
        }

        if (slot.State.HasFlag(SkillSlotState.Equipped))
            SkillDetail_Popup.Find("EquipButton/Text").GetComponent<TMP_Text>().text = "해제";
        else
            SkillDetail_Popup.Find("EquipButton/Text").GetComponent<TMP_Text>().text = "장착";

        equipBtn.onClick.RemoveAllListeners();
        equipBtn.onClick.AddListener(() =>
        {
            if (slot.State.HasFlag(SkillSlotState.Equipped))    // 장착중 -> 해제버튼
                OnClick_UnequipSkill(slot, skillId);
            else                    // 미장착중 -> 장착버튼
                OnClick_EquipSkill(slot, skillId);
        });

        upgradeBtn.onClick.RemoveAllListeners();
        upgradeBtn.onClick.AddListener(() =>
        {
            OnClick_UpgradeSkill(slot, skillId);
        });
    }

    void OnClick_UpgradeSkill(SkillSlot slot, string skillId)
    {
        slot.UpgradeSkill();
        var userSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.SkillData).Split('@');
        userSkillData[int.Parse(skillId) - 1] = $"{slot.CurrentLevel},{slot.HoldingCount}";

        var equippedSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.EquippedSkill).Split('@');
        for (int i = 0; i < equippedSkillData.Length; i++)
        {
            if (equippedSkillData[i] == skillId)
            {
                equippedSkillSlot[i].transform.Find("Level_Text").GetComponent<TMP_Text>().text = $"LV{slot.CurrentLevel}";
                break;
            }
        }

        SkillDetail_Popup.gameObject.SetActive(false);
        GlobalManager.Instance.DBManager.UpdateUserData(UserStringDataType.SkillData, string.Join('@', userSkillData));
    }
    void OnClick_EquipSkill(SkillSlot slot, string skillID)
    {
        var equippedSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.EquippedSkill).Split('@');
        bool isEmpty = false;

        // 장착할 공간이 있을 때 => 비어있는 칸에 장착
        for (int i = 0; i < equippedSkillData.Length; i++)
        {
            if (string.IsNullOrEmpty(equippedSkillData[i])) // 비어있는지 확인
            {
                isEmpty = true;

                string[] icon = SkillTemplate[skillID][(int)SkillTemplate_.Icon].Split('/');
                string spriteName = icon[0];
                string atlasName = icon[1];
                Sprite sp = CommonFunction.GetSprite_Atlas(spriteName, atlasName);
                lobbyEquipSkill_Image[i].sprite = sp;

                equippedSkillSlot[i].Find("Image").GetComponent<Image>().sprite = sp;
                equippedSkillSlot[i].Find("Image").GetComponent<Image>().gameObject.SetActive(true);

                equippedSkillSlot[i].transform.Find("Level_Text").GetComponent<TMP_Text>().text = $"LV{slot.CurrentLevel}";

                slot.State |= SkillSlotState.Equipped;
                slot.SetSlot(slot.State);

                Button simpleUnequipBtn = equippedSkillSlot[i].transform.Find("Unequip_Button").GetComponent<Button>();
                simpleUnequipBtn.gameObject.SetActive(true);
                simpleUnequipBtn.onClick.RemoveAllListeners();
                simpleUnequipBtn.onClick.AddListener(() => OnClick_UnequipSkill(slot, skillID));

                Button skillDetailBtn = equippedSkillSlot[i].GetComponent<Button>();
                skillDetailBtn.onClick.RemoveAllListeners();
                skillDetailBtn.onClick.AddListener(() => OpenUI_SkillDetail(slot, skillID));

                equippedSkillData[i] = skillID;     // 비어있는 칸에 스킬 장착
                GlobalManager.Instance.DBManager.UpdateUserData(UserStringDataType.EquippedSkill, string.Join("@", equippedSkillData));


                //StartSkillCooltime(i, skillID, true);
                //skillController.equippedSkillInfo[i].SetSkillInfo(skillID);
                skillController.SetEquipSkill(i, skillID);
                skillController.StartSkillCooltime(i);
                
                break;
            }
        }

        // 장착할 공간이 없을 때 => 6칸 중에 선택해서 교체하도록 해야됨
        if (!isEmpty)
        {

        }

        if (SkillDetail_Popup)
            SkillDetail_Popup.gameObject.SetActive(false);
    }

    void OnClick_UnequipSkill(SkillSlot slot, string skillId)
    {
        var equippedSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.EquippedSkill).Split('@');

        for (int i = 0; i < equippedSkillData.Length; i++)
        {
            if (equippedSkillData[i] == skillId)    // 장착해제할 스킬 찾기
            {
                equippedSkillData[i] = "";          // 장착 해제

                lobbyEquipSkill_Image[i].sprite = null;
                equippedSkillSlot[i].Find("Image").GetComponent<Image>().sprite = null;
                equippedSkillSlot[i].Find("Image").GetComponent<Image>().gameObject.SetActive(false);

                equippedSkillSlot[i].transform.Find("Level_Text").GetComponent<TMP_Text>().text = $"";
                equippedSkillSlot[i].transform.Find("Unequip_Button").gameObject.SetActive(false);
                equippedSkillSlot[i].GetComponent<Button>().onClick.RemoveAllListeners();

                
                skillController.StopSkillCooltime(i);
                //skillController.equippedSkillInfo[i].SetSkillInfo();
                break;
            }
        }

        slot.State &= ~SkillSlotState.Equipped;
        slot.SetSlot(slot.State);

        if (SkillDetail_Popup)
            SkillDetail_Popup.gameObject.SetActive(false);

        GlobalManager.Instance.DBManager.UpdateUserData(UserStringDataType.EquippedSkill, string.Join("@", equippedSkillData));
    }
    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    Debug.Log("1누름");
        //    StopCoroutine(Co_SkillCooltime[0]);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    Debug.Log("2누름");
        //    StopCoroutine(Co_SkillCooltime[1]);
        //}
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("3누름");
            GameObject obj = CommonFunction.GetPrefabInstance("Lazer_blue",transform);
            
            obj.transform.position = playerCharacter.transform.position + new Vector3();
        }
    }

    private void OnClickUseSkill(int index)
    {
        var equippedSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.EquippedSkill).Split('@');

        if (string.IsNullOrEmpty(equippedSkillData[index])) return; // 스킬 장착중이 아닌 경우

        skillController.UseSkill(index, equippedSkillData[index]);
    }


    #region 테스트용
    //private void OnClickUseSkill(int index)
    //{
    //    var equippedSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.EquippedSkill).Split('@');

    //    if (string.IsNullOrEmpty(equippedSkillData[index])) return; // 스킬 장착중인 칸이 아닌 경우

    //    Debug.Log($"스킬쿨 : {isSkillCooltime[index]}");

    //    if (!isSkillCooltime[index]) // 사용가능하면 
    //    {
    //        SkillController.instance.StartCoroutine(Co_SkillCooltime[index]);
    //        //StartSkillCooltime(index, equippedSkillData[index]);
    //        UseSkill(equippedSkillData[index]);
    //    }
    //}

    //void StartSkillCooltime(int index, string skillID, bool resetCooltime = false)
    //{
    //    if (!isSkillCooltime[index] || resetCooltime) // resetCooltime: 스킬 장착시, 무조건 쿨타임 돌도록 만듬
    //    {
    //        if (resetCooltime && Co_SkillCooltime[index] != null)
    //            StopCoroutine(Co_SkillCooltime[index]);

    //        isSkillCooltime[index] = true;
    //        int cooltime = int.Parse(SkillTemplate[skillID][(int)SkillTemplate_.Cooltime]);

    //        Co_SkillCooltime[index] = Co_SkillCoolTime(cooltime, index);
    //        StartCoroutine(Co_SkillCooltime[index]);
    //    }
    //}
    //void StopSkillCooltime(int index)
    //{
    //    if (Co_SkillCooltime[index] != null)
    //        StopCoroutine(Co_SkillCooltime[index]);

    //    lobbyEquipSkill_Image[index].color = Color.white;
    //    lobbyEquipSkill_Image[index].transform.Find("CoolTime").GetComponent<Image>().fillAmount = 0;
    //}
    //public IEnumerator Co_SkillCoolTime(float cool, int index)
    //{
    //    Image coolTimeIcon = lobbyEquipSkill_Image[index].transform.Find("CoolTime").GetComponent<Image>();
    //    coolTimeIcon.fillAmount = 1f;

    //    lobbyEquipSkill_Image[index].color = Color.gray;

    //    float time = cool;
    //    while (time > 0)
    //    {
    //        time -= Time.deltaTime;
    //        coolTimeIcon.fillAmount = time / cool;

    //        yield return CommonIEnumerator.WaitForEndOfFrame;
    //    }

    //    lobbyEquipSkill_Image[index].color = Color.white;
    //    isSkillCooltime[index] = false;
    //}
    #endregion 테스트용
    void OpenUI_Category4(GameObject clickButton)
    {

    }



    public void GetGoods(double getGold = 0, double getGem = 0)
    {
        if (getGold > 0)
        {
            //Debug.Log($"{Util.BigNumCalculate(getGold)}G를 획득");

            double currentGold = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.Gold);

            //goldText.text = $"{(currentGold + getGold)}";
            goldText.text = $"{Util.BigNumCalculate(currentGold + getGold)}";
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
            goldText.text = $"{Util.BigNumCalculate(currentGold - usedGold)}";
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
            gemText.text = Util.BigNumCalculate(currentGem - usedGem);
            GlobalManager.Instance.DBManager.UpdateUserData(UserDoubleDataType.Gem, currentGem - usedGem);


            return true;
        }
        else
        {
            Debug.LogError("젬이 부족합니다");

            return false;
        }
    }
    public void SetCurrentStageState(StageState state)
    {
        CurrentStageState = state;
    }
}
