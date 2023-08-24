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

    [Header("Total Damage Text")]
    [SerializeField] private TextMeshProUGUI totalDamageText;

    [Header("Stage Text")]
    private GameObject stageText;
    private MonsterSpawner spawner; //스테이지 세팅용

    [HideInInspector] public int heroId;    //임시 테스트용
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
    private Image[] lobbySkillImage = null;
    private RectTransform category1_UI = null;  // 캐릭터, 장비
    private RectTransform category2_UI = null;  //장비
    private Transform SkillDetail_Popup = null;

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

        SetCurrentStageState(StageState.NormalWave);

        stageId = (int)GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.CurrentStageId, 1);
        SetStage(stageId);  //스테이지 세팅

        heroId = (int)GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.CurrentHeroId, 1);
        SetPlayer(heroId);  //영웅 세팅      

        goldText = mainUiPanel.transform.Find("UpSide_Panel/Goods_Panel/Gold_Image/Gold_Text").GetComponent<TMP_Text>();
        gemText = mainUiPanel.transform.Find("UpSide_Panel/Goods_Panel/Gem_Image/Gem_Text").GetComponent<TMP_Text>();

        var skillPanel = mainUiPanel.transform.Find("UpSide_Panel/Skill_Panel");
        
        lobbySkillImage = new Image[skillPanel.childCount -1];
        for (int i = 0; i < skillPanel.childCount - 1; i++)
            lobbySkillImage[i] = skillPanel.GetChild(i+1).GetComponent<Image>();
        
        popupUI_0 = UIManager.instance.transform.Find("PopupUI_0").gameObject;
        popupUI_1 = UIManager.instance.transform.Find("PopupUI_1").gameObject;

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

        playerCharacter.SetHeroStatus(heroId);  //영웅 기본 스탯 세팅

        IsPlayer = true;

        UpdateStatusLevel();    //최초 1회 스탯 레벨 업데이트
    }
    public void SetStage(int stageId)
    {
        //Debug.Log($"현재 스테이지ID : {stageId}");

        if (stageText != null)
            Destroy(stageText.gameObject);

        stageName = StageTemplate[stageId.ToString()][(int)StageTemplate_.Stage];
        stageText = CommonFunction.GetPrefab("StageName_Text", upSidePanel.transform);
        stageText.GetComponent<TextMeshProUGUI>().text = stageName;

        mapName = StageTemplate[stageId.ToString()][(int)StageTemplate_.MapImage];  //맵이미지 세팅
        monsterId = int.Parse(StageTemplate[stageId.ToString()][(int)StageTemplate_.Monster]); //몬스터 세팅
        bossId = int.Parse(StageTemplate[stageId.ToString()][(int)StageTemplate_.Boss]); //보스몬스터 세팅

        mapSprite = Resources.Load<Sprite>($"Sprite/{mapName}"); //맵 세팅
        map1.sprite = mapSprite;
        map2.sprite = mapSprite;
    }
    public void SetSkill(string[] equippedSkill)
    {
        for (int i = 0; i < equippedSkill.Length; i++)
        {
            if (string.IsNullOrEmpty(equippedSkill[i]))
                continue;

            string icon = SkillTemplate[equippedSkill[i]][(int)SkillTemplate_.Icon];

            string[] iconDatas = icon.Split('/');
            string spriteName = iconDatas[0];
            string atlasName = iconDatas[1];
            lobbySkillImage[i].sprite = CommonFunction.GetSprite_Atlas(spriteName, atlasName);
        }
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

        totalDamageText.text = $"총 공격력 : {Util.BigNumCalculate(playerCharacter.TotalAttack)}";
    }
    private void InitUIfromDB()
    {
        double gold = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.Gold);
        double gem = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.Gem);

        goldText.text = $"{Util.BigNumCalculate(gold)} G";
        gemText.text = Util.BigNumCalculate(gem);

        var equippedSkill = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.EquippedSkill,"@@@@@").Split('@');

        SetSkill(equippedSkill);    // 스킬 세팅
    }

    private void EnterBossRoom()
    {
        if (isWaveClear)
        {
            //Debug.Log("보스방 입장");
            IsWaveClear = false;

            SetCurrentStageState(StageState.BossRoom);

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
        _victoryText.text = "Victory";

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
            case 4:
                OpenUI_Category4(onClickButton);
                break;

            default:
                break;
        }
    }

    void OpenUI_Category1(GameObject clickButton)
    {
        if (category1_UI == null)
        {
            category1_UI = CommonFunction.GetPrefab("Category1", popupUI_0.transform).GetComponent<RectTransform>();
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
            category2_UI = CommonFunction.GetPrefab("UI/Item_Panel", popupUI_0.transform).GetComponent<RectTransform>();
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

    void ShowUI_Character(bool isFirst = false)
    {
        Debug.Log("ShowUI_Character");
        category1_UI.Find("Type1_Character").gameObject.SetActive(true);
        category1_UI.Find("Type2_Skill").gameObject.SetActive(false);
    }
    

    void ShowUI_Skill(bool isFirst = false)
    {
        var equippedSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.EquippedSkill).Split('@');

        if (isFirst)        // 최초 1회만 실행
        {
            Transform grid = category1_UI.Find("Type2_Skill/Scroll View/Viewport/Grid");
            Util.InitGrid(grid);
            foreach (var item in SkillTemplate.OrderBy(x => x.Value[(int)SkillTemplate_.Order]))
            {
                string id = item.Value[(int)SkillTemplate_.SkillId];
                string grade = item.Value[(int)SkillTemplate_.Grade];
                string icon = item.Value[(int)SkillTemplate_.Icon];

                GameObject skill_Icon = CommonFunction.GetPrefab("SkillSlot", grid);
                SkillSlot slot = skill_Icon.AddComponent<SkillSlot>();
                slot.Init(id);

                // 스킬 아이콘 선택 시 (상세보기)
                skill_Icon.GetComponent<Button>().onClick.AddListener(() => {
                    OpenUI_SkillDetail(slot, id);
                });

                skill_Icon.transform.Find("UpperRightImage").GetComponent<Button>().onClick.AddListener(() =>{
                    if (slot.State.HasFlag(SkillSlotState.Equipped))
                    {
                        UnequipSkill(slot, id);
                    }
                    else
                    {
                        EquipSkill(slot, id);
                    }
                });

            }
        }
        
        category1_UI.Find("Type2_Skill").gameObject.SetActive(true);
        category1_UI.Find("Type1_Character").gameObject.SetActive(false);

        Transform equippedSkillGroup = category1_UI.Find("Type2_Skill/EquippedSkillGroup");
        Image[] equippedSkill_Image = new Image[equippedSkillGroup.childCount];
        for (int i = 0; i < equippedSkillGroup.childCount; i++)
            equippedSkill_Image[i] = equippedSkillGroup.GetChild(i).GetComponent<Image>();

        // 장착중인 스킬
        for (int i = 0; i < equippedSkillData.Length; i++)
        {
            if (string.IsNullOrEmpty(equippedSkillData[i]))
                continue;

            string icon = SkillTemplate[equippedSkillData[i]][(int)SkillTemplate_.Icon];

            string[] iconDatas = icon.Split('/');
            string spriteName = iconDatas[0];
            string atlasName = iconDatas[1];
            equippedSkill_Image[i].sprite = CommonFunction.GetSprite_Atlas(spriteName, atlasName);
        }


    }
    void OpenUI_SkillDetail(SkillSlot slot, string skillId)
    {
        if (SkillDetail_Popup == null)
            SkillDetail_Popup = CommonFunction.GetPrefab("SkillDetailPopup", popupUI_1.transform).transform;
        else
            SkillDetail_Popup.gameObject.SetActive(true);
        
        DOTween.Sequence()
       .OnStart(() =>{ SkillDetail_Popup.transform.localScale = Vector3.zero;})
       .Append(SkillDetail_Popup.transform.DOScale(1, 0.3f).SetEase(Ease.OutBack));

        //string id = SkillTemplate[skillId][(int)SkillTemplate_.SkillId];
        string skillName = SkillTemplate[skillId][(int)SkillTemplate_.Name];
        string grade = SkillTemplate[skillId][(int)SkillTemplate_.Grade];
        string icon = SkillTemplate[skillId][(int)SkillTemplate_.Icon];
        string desc = SkillTemplate[skillId][(int)SkillTemplate_.Desc];
        string cooltime = SkillTemplate[skillId][(int)SkillTemplate_.Cooltime];

        string[] iconDatas = icon.Split('/');
        string spriteName = iconDatas[0];
        string atlasName = iconDatas[1];

        SkillDetail_Popup.Find("Icon").GetComponent<Image>().sprite = CommonFunction.GetSprite_Atlas(spriteName, atlasName);
        SkillDetail_Popup.Find("SkillName_Text").GetComponent<TMP_Text>().text = skillName;
        SkillDetail_Popup.Find("Grade_Text").GetComponent<TMP_Text>().color = Util.ConvertGradeToColor(grade);
        SkillDetail_Popup.Find("Grade_Text").GetComponent<TMP_Text>().text = grade;
        SkillDetail_Popup.Find("DescBG/Desc_Text").GetComponent<TMP_Text>().text = desc;
        SkillDetail_Popup.Find("DescBG/CoolTime_Text").GetComponent<TMP_Text>().text = $"{cooltime}초";

        int level = slot.CurrentLevel;
        int currentValue = slot.HoldingCount;
        int requireQuantity = int.Parse(LevelTemplate[level.ToString()][(int)LevelTemplate_.RequiredQuantity]);
        
        SkillDetail_Popup.Find("Level_Text").GetComponent<TMP_Text>().text = $"LV. {level}";
        SkillDetail_Popup.Find("Slider/CurrentValue_Text").GetComponent<TMP_Text>().text = $"{currentValue}";
        SkillDetail_Popup.Find("Slider/TargetValue_Text").GetComponent<TMP_Text>().text = $"/ {requireQuantity}";
        SkillDetail_Popup.Find("Slider").GetComponent<Slider>().value = currentValue / (float)requireQuantity;

        //if (slot.IsLocked)
        if (slot.State == SkillSlotState.Lock)
        {
            SkillDetail_Popup.Find("EquipButton/ButtonImage").GetComponent<Image>().color = Color.gray;// new Color(87f / 255f, 87f / 255f, 87f / 255f, 1);
            SkillDetail_Popup.Find("UpgradeButton/ButtonImage").GetComponent<Image>().color = Color.gray;

            SkillDetail_Popup.Find("EquipButton").GetComponent<Button>().interactable = false;
            SkillDetail_Popup.Find("UpgradeButton").GetComponent<Button>().interactable = false;
        }
        else
        {
            //if (slot.IsUpgradeable)
            if (slot.State.HasFlag(SkillSlotState.Upgradeable))
            {
                SkillDetail_Popup.Find("UpgradeButton").GetComponent<Button>().interactable = true;
                SkillDetail_Popup.Find("UpgradeButton/ButtonImage").GetComponent<Image>().color = Color.white;
            }
            else
            {
                SkillDetail_Popup.Find("UpgradeButton").GetComponent<Button>().interactable = false;
                SkillDetail_Popup.Find("UpgradeButton/ButtonImage").GetComponent<Image>().color = Color.gray;
            }

            SkillDetail_Popup.Find("EquipButton").GetComponent<Button>().interactable = true;
            SkillDetail_Popup.Find("EquipButton/ButtonImage").GetComponent<Image>().color = Color.white;
        }

        //if (slot.IsEqiupped)
        if (slot.State.HasFlag(SkillSlotState.Equipped))
            SkillDetail_Popup.Find("EquipButton/Text").GetComponent<TMP_Text>().text = "해제";
        else
            SkillDetail_Popup.Find("EquipButton/Text").GetComponent<TMP_Text>().text = "장착";

        SkillDetail_Popup.Find("EquipButton").GetComponent<Button>().onClick.RemoveAllListeners();
        SkillDetail_Popup.Find("EquipButton").GetComponent<Button>().onClick.AddListener(() =>
        {
            //if (slot.IsEqiupped)    // 장착중 -> 해제버튼
            if (slot.State.HasFlag(SkillSlotState.Equipped))    // 장착중 -> 해제버튼
                UnequipSkill(slot, skillId);
            else                    // 미장착중 -> 장착버튼
                EquipSkill(slot, skillId);
        });

        SkillDetail_Popup.Find("UpgradeButton").GetComponent<Button>().onClick.RemoveAllListeners();
        SkillDetail_Popup.Find("UpgradeButton").GetComponent<Button>().onClick.AddListener(() =>
        {
            UpgradeSkill(slot,skillId);
        });
    }
    
    void UpgradeSkill(SkillSlot slot, string skillId)
    {
        slot.UpgradeSkill();
        var userSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.SkillData).Split('@');

        userSkillData[int.Parse(skillId) - 1] = $"{slot.CurrentLevel},{slot.HoldingCount}";
        Debug.Log("업그레이드 완료 : " + userSkillData);

        SkillDetail_Popup.gameObject.SetActive(false);
        GlobalManager.Instance.DBManager.UpdateUserData(UserStringDataType.SkillData,string.Join('@', userSkillData));
    }
    void EquipSkill(SkillSlot slot, string skillId)
    {
        Transform equippedSkillGroup = category1_UI.Find("Type2_Skill/EquippedSkillGroup");
        Image[] equippedSkill_Image = new Image[equippedSkillGroup.childCount];
        for (int j = 0; j < equippedSkillGroup.childCount; j++)
            equippedSkill_Image[j] = equippedSkillGroup.GetChild(j).GetComponent<Image>();

        Debug.Log("장착");
        var equippedSkill = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.EquippedSkill).Split('@');
        bool isEmpty = false;

        // 장착할 공간이 있을 때 => 비어있는 칸에 장착
        for (int i = 0; i < equippedSkill.Length; i++)
        {
            if (string.IsNullOrEmpty(equippedSkill[i]))
            {
                isEmpty = true;
                equippedSkill[i] = skillId;     // 비어있는 칸에 스킬 장착

                string icon = SkillTemplate[equippedSkill[i]][(int)SkillTemplate_.Icon];

                string[] iconDatas = icon.Split('/');
                string spriteName = iconDatas[0];
                string atlasName = iconDatas[1];

                slot.State |= SkillSlotState.Equipped;
                slot.SetSlot(slot.State);

                Sprite sp = CommonFunction.GetSprite_Atlas(spriteName, atlasName);
                lobbySkillImage[i].sprite = sp;
                equippedSkill_Image[i].sprite = sp;
                
                break;
            }
        }

        // 장착할 공간이 없을 때 => 6칸 중에 선택해서 교체하도록 해야됨
        if (!isEmpty)
        {

        }
        
        if (SkillDetail_Popup)
            SkillDetail_Popup.gameObject.SetActive(false);

        GlobalManager.Instance.DBManager.UpdateUserData(UserStringDataType.EquippedSkill, string.Join("@", equippedSkill));
    }

    void UnequipSkill(SkillSlot slot, string skillId)
    {
        Debug.Log("해제");
        Transform equippedSkillGroup = category1_UI.Find("Type2_Skill/EquippedSkillGroup");
        Image[] equippedSkill_Image = new Image[equippedSkillGroup.childCount];
        for (int j = 0; j < equippedSkillGroup.childCount; j++)
            equippedSkill_Image[j] = equippedSkillGroup.GetChild(j).GetComponent<Image>();

        var equippedSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.EquippedSkill).Split('@');

        for (int i = 0; i < equippedSkillData.Length; i++)
        {
            if (equippedSkillData[i] == skillId)    // 장착해제할 스킬 찾기
            {
                equippedSkillData[i] = "";

                lobbySkillImage[i].sprite = null;
                equippedSkill_Image[i].sprite = null;
                
                break;
            }
        }
        slot.State &= ~SkillSlotState.Equipped;
        slot.SetSlot(slot.State);
;
        if (SkillDetail_Popup)
            SkillDetail_Popup.gameObject.SetActive(false);

        GlobalManager.Instance.DBManager.UpdateUserData(UserStringDataType.EquippedSkill, string.Join("@", equippedSkillData));
    }
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
