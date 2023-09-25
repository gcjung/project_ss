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
using System;
using static UnityEditor.Progress;
//using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;


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
    private MonsterSpawner spawner; //�������� ���ÿ�

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

    private RectTransform category1_UI = null;  // ĳ����, ���
    private RectTransform category2_UI = null;  // ���
    private RectTransform category5_UI = null;  // �̱�
    private Transform SkillDetail_Popup = null;

    private GameObject popupUI_0 = null;    // UI�� ����� ĵ���� ���̾�
    private GameObject popupUI_1 = null;    // UI�� ����� ĵ���� ���̾�
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

        Debug.Log("���ξ� ����");

        SetCurrentStageState(StageState.NormalWave);

        stageId = (int)GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.CurrentStageId, 1);
        SetStage(stageId);  //�������� ����

        heroId = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.CurrentHeroId, 1);
        SetPlayer(heroId);  //���� ����      

        goldText = mainUiPanel.transform.Find("UpSide_Panel/Goods_Panel/Gold_Image/Gold_Text").GetComponent<TMP_Text>();
        gemText = mainUiPanel.transform.Find("UpSide_Panel/Goods_Panel/Gem_Image/Gem_Text").GetComponent<TMP_Text>();

        popupUI_0 = UIManager.instance.transform.Find("PopupUI_0").gameObject;
        popupUI_1 = UIManager.instance.transform.Find("PopupUI_1").gameObject;

        skillController = FindObjectOfType<SkillController>();
        skillController.Init(playerCharacter);

        var skillPanel = mainUiPanel.transform.Find("UpSide_Panel/Skill_Panel");
        // �κ� ��ųUI ��ư
        lobbyEquipSkill_Image = new Image[skillPanel.childCount - 1];
        for (int i = 0; i < skillPanel.childCount - 1; i++)
        {
            if (i == 0)
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

        // ���� ī�װ� ��ư
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
        Debug.Log($"���� ����ID : {heroId}");

        if (playerCharacter != null)
            Destroy(playerCharacter.gameObject);

        heroName = HeroTemplate[heroId.ToString()][(int)HeroTemplate_.Name];

        var _playerCharacter = Resources.Load<Player>($"Player/{heroName}");   //�÷��̾� ĳ���� ����
        playerCharacter = Instantiate(_playerCharacter, transform);
        playerCharacter.transform.position = playerSpawnPoint.position;
        PlayerController playerController = playerCharacter.AddComponent<PlayerController>();

        playerCharacter.SetHeroStatus(heroId);  //���� �⺻ ���� ����

        IsPlayer = true;

        UpdateStatusLevel();    //���� 1ȸ ���� ���� ������Ʈ

        var hpBar = CommonFunction.GetPrefabInstance("Slider_HealthBar_Hero", upSidePanel.transform);   //ü�¹� ����
        hpBar.GetComponent<HpSlider>().SetTarget(playerCharacter.gameObject);
    }
    public void SetStage(int stageId)
    {
        //Debug.Log($"���� ��������ID : {stageId}");

        if (stageText != null)
            Destroy(stageText.gameObject);

        stageName = StageTemplate[stageId.ToString()][(int)StageTemplate_.Stage];
        stageText = CommonFunction.GetPrefabInstance("StageName_Text", upSidePanel.transform);
        stageText.GetComponent<TextMeshProUGUI>().text = stageName;

        mapName = StageTemplate[stageId.ToString()][(int)StageTemplate_.MapImage];  //���̹��� ����
        monsterId = int.Parse(StageTemplate[stageId.ToString()][(int)StageTemplate_.Monster]); //���� ����
        bossId = int.Parse(StageTemplate[stageId.ToString()][(int)StageTemplate_.Boss]); //�������� ����

        mapSprite = Resources.Load<Sprite>($"Sprite/{mapName}"); //�� ����
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
        SetLobbyAutoSkill(!isOn);    // ��ư �������ϱ� �ݴ��
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
            default:    //���� 1ȸ �������� ��� ���Ȱ� ����
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

        totalDamageText.text = $"�� ���ݷ� : {Util.BigNumCalculate(playerCharacter.TotalAttack)}";
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
        SetLobbySkill(equippedSkill);    // ��ų ����
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

        _playerPref.transform.DOMove(playerPos, 1.0f).SetEase(Ease.OutQuad);    //�÷��̾� ĳ���� �̵�
        _bossPref.transform.DOMove(bossPos, 1.0f).SetEase(Ease.OutQuad).OnComplete(() =>    //���� ĳ���� �̵�
        {
            vsText = CommonFunction.GetPrefabInstance("VS_Text", upSidePanel.transform);
            DOTween.Sequence()
            .OnStart(() => { vsText.transform.localScale = Vector3.zero; })
            .Append(vsText.transform.DOScale(3, 0.5f).SetEase(Ease.OutBack));
        });

        yield return new WaitForSeconds(delayTime);

        {//�����ҵ� �ı�
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

        Color color = tmp.color;    //���İ� 0���� �ʱ�ȭ
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
              spawner.SetMonster();
              SetPlayer(heroId);
              SetStage(stageId);            
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
                OpenUI_Category5(button);
                break;

            default:
                break;
        }
    }

    private Category1State category1State;
    const int invisiblePosY = -1900;

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
            {
                category1_UI.DOAnchorPosY(0, 0.3f).SetEase(Ease.OutExpo);

                if (category1State == Category1State.Character) // ��Ȱ��ȭ �� Characterâ�̾��ٸ� selectedHeroSlot�� �ʱ�ȭ
                {
                    ResetSelectedHeroSlot();
                }
                else if(category1State == Category1State.Skill)
                {
                    ShowUI_Skill();
                }
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
        //��� UI
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

        category1State = Category1State.Character;

        category1_UI.Find("Type1_Character").gameObject.SetActive(true);
        category1_UI.Find("Type2_Skill").gameObject.SetActive(false);

        if (isFirst)        // ���� 1ȸ�� ����
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

                //���� ������ ������ �̹���,�̸� ������Ʈ
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

                // ���� ������ ���� �� ���õ� ���� ǥ��
                heroSlot.GetComponent<Button>().onClick.AddListener(() =>
                {
                    ChangeSelectedSlot(slot);
                });
            }
        }

        //���õ� ���� �ʱ�ȭ
        if (selectedHeroSlot != null)
        {
            ResetSelectedHeroSlot();
        }        
    }

    private void ChangeCurrentHero()
    {
        if (CurrentStageState != StageState.BossRoom && selectedHeroSlot != null) //���������� NormalWave ������ ���� ���� ����
        {
            if (double.Parse(selectedHeroSlot.heroId) != heroId)    //�̹� �������� ������ �ƴ� ��
            {
                GlobalManager.Instance.DBManager.UpdateUserData("CurrentHeroId", selectedHeroSlot.heroId);
                heroId = double.Parse(selectedHeroSlot.heroId);
                heroName = HeroTemplate[heroId.ToString()][(int)HeroTemplate_.Name];

                StartCoroutine(RestartStage());
            }
            else
            {
                CommonFunction.CreateNotification("�̹� ���� ���� ������", popupUI_1.transform);
            }            
        }
        else
        {
            CommonFunction.CreateNotification("���� ������ ��ü�� �� ����", popupUI_1.transform);
        }
    }

    private void ChangeSelectedSlot(HeroSlot slot)
    {
        if (selectedHeroSlot != null)
        {
            selectedHeroSlot.SetSelected(false);
        }

        // ���� Ŭ���� ������ ���õ� �������� ����
        selectedHeroSlot = slot;

        // ���õ� ������ ���¸� �����Ͽ� üũǥ�ø� Ȱ��ȭ
        selectedHeroSlot.SetSelected(true);
        disabledImage.gameObject.SetActive(false);
    }

    private void ResetSelectedHeroSlot()
    {
        selectedHeroSlot.SetSelected(false);
        selectedHeroSlot = null;
        disabledImage.gameObject.SetActive(true);
    }

    const int maxEquipSkillCount = 6;
    Transform[] equippedSkillSlot = null;
    void ShowUI_Skill(bool isFirst = false)
    {
        category1State = Category1State.Skill;

        var equippedSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.EquippedSkill).Split('@');

        category1_UI.Find("Type2_Skill").gameObject.SetActive(true);
        category1_UI.Find("Type1_Character").gameObject.SetActive(false);

        if (isFirst)        // ���� 1ȸ�� ����
        {
            SkillSlot[] equipSkillSlot = new SkillSlot[maxEquipSkillCount]; // ������ ��ų ���Ե�����

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

                // ��ų ������ ���� �� (�󼼺���)
                skillslot.GetComponent<Button>().onClick.AddListener(() =>
                {
                    OpenUI_SkillDetail(slot, id);
                });

                // ���� ����/���� ��ư
                skillslot.transform.Find("UpperRightImage").GetComponent<Button>().onClick.AddListener(() =>
                {
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

            // ������ ��ų����
            for (int i = 0; i < equippedSkillData.Length; i++)
            {
                int index = i;
                string skillID = equippedSkillData[i];

                if (!string.IsNullOrEmpty(skillID))  // ��ų �������� ���
                {
                    string[] icon = SkillTemplate[skillID][(int)SkillTemplate_.Icon].Split('/');
                    string spriteName = icon[0];
                    string atlasName = icon[1];

                    equippedSkillSlot[i].Find("Image").GetComponent<Image>().sprite = CommonFunction.GetSprite_Atlas(spriteName, atlasName);
                    equippedSkillSlot[i].Find("Image").GetComponent<Image>().gameObject.SetActive(true);

                    string skillLevel = skillAcquireData[int.Parse(skillID) - 1].Split(",")[0];    // syntax : ��ų����,��������@
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
        else
        {
            Debug.Log("��ų������!!!! ������Ʈ�϶�");
            Transform grid = category1_UI.Find("Type2_Skill/Scroll View/Viewport/Grid");
            for (int i = 0; i < grid.childCount; i++)
            {
                //string id = item.Value[(int)SkillTemplate_.SkillId];
                //string grade = item.Value[(int)SkillTemplate_.Grade];
                //string icon = item.Value[(int)SkillTemplate_.Icon];

                //GameObject skillslot = CommonFunction.GetPrefabInstance("SkillSlot", grid);

                SkillSlot slot = grid.GetChild(i).GetComponent<SkillSlot>();
                slot.UpdateSkillSlot();

  
     
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
        SkillDetail_Popup.Find("DescBG/CoolTime_Text").GetComponent<TMP_Text>().text = $"{cooltime}��";

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
            SkillDetail_Popup.Find("EquipButton/Text").GetComponent<TMP_Text>().text = "����";
        else
            SkillDetail_Popup.Find("EquipButton/Text").GetComponent<TMP_Text>().text = "����";

        equipBtn.onClick.RemoveAllListeners();
        equipBtn.onClick.AddListener(() =>
        {
            if (slot.State.HasFlag(SkillSlotState.Equipped))    // ������ -> ������ư
                OnClick_UnequipSkill(slot, skillId);
            else                    // �������� -> ������ư
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

        // ������ ������ ���� �� => ����ִ� ĭ�� ����
        for (int i = 0; i < equippedSkillData.Length; i++)
        {
            if (string.IsNullOrEmpty(equippedSkillData[i])) // ����ִ��� Ȯ��
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

                equippedSkillData[i] = skillID;     // ����ִ� ĭ�� ��ų ����
                GlobalManager.Instance.DBManager.UpdateUserData(UserStringDataType.EquippedSkill, string.Join("@", equippedSkillData));


                //StartSkillCooltime(i, skillID, true);
                //skillController.equippedSkillInfo[i].SetSkillInfo(skillID);
                skillController.SetEquipSkill(i, skillID);
                skillController.StartSkillCooltime(i);

                break;
            }
        }

        // ������ ������ ���� �� => 6ĭ �߿� �����ؼ� ��ü�ϵ��� �ؾߵ�
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
            if (equippedSkillData[i] == skillId)    // ���������� ��ų ã��
            {
                equippedSkillData[i] = "";          // ���� ����

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

    private void OnClickUseSkill(int index)
    {
        var equippedSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.EquippedSkill).Split('@');

        if (string.IsNullOrEmpty(equippedSkillData[index])) return; // ��ų �������� �ƴ� ���

        skillController.UseSkill(index, equippedSkillData[index]);
    }


    #region �׽�Ʈ��
    //private void OnClickUseSkill(int index)
    //{
    //    var equippedSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.EquippedSkill).Split('@');

    //    if (string.IsNullOrEmpty(equippedSkillData[index])) return; // ��ų �������� ĭ�� �ƴ� ���

    //    Debug.Log($"��ų�� : {isSkillCooltime[index]}");

    //    if (!isSkillCooltime[index]) // ��밡���ϸ� 
    //    {
    //        SkillController.instance.StartCoroutine(Co_SkillCooltime[index]);
    //        //StartSkillCooltime(index, equippedSkillData[index]);
    //        UseSkill(equippedSkillData[index]);
    //    }
    //}

    //void StartSkillCooltime(int index, string skillID, bool resetCooltime = false)
    //{
    //    if (!isSkillCooltime[index] || resetCooltime) // resetCooltime: ��ų ������, ������ ��Ÿ�� ������ ����
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
    #endregion �׽�Ʈ��
    void OpenUI_Category5(GameObject clickButton)
    {
        if (category5_UI == null)
        {
            category5_UI = CommonFunction.GetPrefabInstance("Category5", popupUI_0.transform).GetComponent<RectTransform>();
            category5_UI.anchoredPosition = new Vector2(0, invisiblePosY);
            category5_UI.DOAnchorPosY(100, 0.3f).SetEase(Ease.OutExpo);

            //Transform bottomMenu = category5_UI.Find("BottomBar/BottomMenu");
            //for (int i = 1; i < bottomMenu.childCount; i++)
            //{
            //    bottomMenu.GetChild(i).Find("BtnOn").gameObject.SetActive(false);
            //}

            ShowUI_Gacha(0, true);
        }
        else
        {
            bool active = category5_UI.gameObject.activeSelf;

            if (active)
                category5_UI.anchoredPosition = new Vector2(0, invisiblePosY);
            else
                category5_UI.DOAnchorPosY(100, 0.3f).SetEase(Ease.OutExpo);

            category5_UI.gameObject.SetActive(!active);
        }

        clickButton.transform.Find("Text").gameObject.SetActive(!category5_UI.gameObject.activeSelf);
        clickButton.transform.Find("CloseImage").gameObject.SetActive(category5_UI.gameObject.activeSelf);

        Transform parent = category5_UI.Find("BottomBar/BottomMenu");
        for (int i = 0; i < parent.childCount; i++)
        {
            int index = i;
            Button button = parent.GetChild(i).GetComponent<Button>();
            button.onClick.RemoveAllListeners();

            switch (index)
            {
                case 0:
                    button.onClick.AddListener(() => ShowUI_Gacha(index));
                    break;
                case 1:
                    button.onClick.AddListener(() => ShowUI_Test(index));
                    break;
                case 2:
                    button.onClick.AddListener(() => ShowUI_Test(index));
                    break;
                case 3:
                    button.onClick.AddListener(() => ShowUI_Test(index));
                    break;
            }
        }

    }
    void ShowUI_Gacha(int index, bool isFirst = false)
    {
        if (isFirst)
        {
            Transform gachaUI = category5_UI.Find("Type1_Gacha");
            string[] itemData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.Gacha_ItemData, "1@0").Split('@');
            {
                int currentLevel = int.Parse(itemData[0]);
                int holdingCount = int.Parse(itemData[1]);

                //var t = LevelTemplate[itemData[0]][(int)LevelTemplate_.Skill_Item_RequiredQuantity];
                var targetValue = int.Parse(LevelTemplate[itemData[0]][(int)LevelTemplate_.Gacha_RequiredQuantity]);
                //int targetValue = int.Parse(LevelTemplate[itemData[0]][(int)LevelTemplate_.Gacha_RequiredQuantity]);

                Transform itemGacha = gachaUI.Find("Scroll View/Viewport/Content").GetChild(0);
                itemGacha.Find("Level_Text").GetComponent<TMP_Text>().text = $"LV. {currentLevel}";
                itemGacha.Find("Slider/CurrentValue_Text").GetComponent<TMP_Text>().text = $"{holdingCount}";
                itemGacha.Find("Slider/TargetValue_Text").GetComponent<TMP_Text>().text = $"/ {targetValue}";

                if (holdingCount != 0)
                    itemGacha.Find("Slider").GetComponent<Slider>().value = holdingCount / (float)targetValue;
            }

            string[] skillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.Gacha_SkillData).Split('@');
            {
                int currentLevel = int.Parse(skillData[0]);
                int holdingCount = int.Parse(skillData[1]);

                var targetValue = int.Parse(LevelTemplate[skillData[0]][(int)LevelTemplate_.Gacha_RequiredQuantity]);

                Transform skillGacha = gachaUI.Find("Scroll View/Viewport/Content").GetChild(1);
                skillGacha.Find("Level_Text").GetComponent<TMP_Text>().text = $"LV. {currentLevel}";
                skillGacha.Find("Slider/CurrentValue_Text").GetComponent<TMP_Text>().text = $"{holdingCount}";
                skillGacha.Find("Slider/TargetValue_Text").GetComponent<TMP_Text>().text = $"/ {targetValue}";

                if (holdingCount == 0)
                    skillGacha.Find("Slider").GetComponent<Slider>().value = 0;
                else
                    skillGacha.Find("Slider").GetComponent<Slider>().value = holdingCount / (float)targetValue;

                skillGacha.Find("11GachaBtn").GetComponent<Button>().onClick.RemoveAllListeners();
                skillGacha.Find("11GachaBtn").GetComponent<Button>().onClick.AddListener(() =>
                {
                    Debug.Log("11���� ��ư ����");
                    Gacha(GachaType.Skill, 11);
                    //StartCoroutine(Gacha(GachaType.Skill, 11));
                });

                skillGacha.Find("35GachaBtn").GetComponent<Button>().onClick.RemoveAllListeners();
                skillGacha.Find("35GachaBtn").GetComponent<Button>().onClick.AddListener(() =>
                {
                    Debug.Log("35���� ��ư ����");
                    Gacha(GachaType.Skill, 35);
                    //StartCoroutine(Gacha(GachaType.Skill, 35));
                });

            }
        }
        //TargetValue = int.Parse(LevelTemplate[CurrentLevel.ToString()][(int)LevelTemplate_.Skill_Item_RequiredQuantity]);

        //transform.Find("CurrentValue_Text").GetComponent<TMP_Text>().text = $"{HoldingCount}";
        //transform.Find("TargetValue_Text").GetComponent<TMP_Text>().text = $"/ {TargetValue}";
        //transform.Find("Slider").GetComponent<Slider>().value = HoldingCount / (float)TargetValue;

        Debug.Log("ShowUI_Gacha, index : " + index);
        Transform parent = category5_UI.Find("BottomBar/BottomMenu");
        for (int i = 0; i < parent.childCount; i++)
        {
            parent.GetChild(i).Find("BtnOn").gameObject.SetActive(false);
        }
        parent.GetChild(index).Find("BtnOn").gameObject.SetActive(true);
    }

    Transform gachaPanel = null;
    void Gacha(GachaType gachaType, int count)
    {
        if (gachaPanel == null)
            gachaPanel = CommonFunction.GetPrefabInstance("GachaUI", popupUI_1.transform).transform;
        else
            gachaPanel.gameObject.SetActive(true);

        gachaPanel.Find("11GachaBtn").GetComponent<Button>().onClick.RemoveAllListeners();
        gachaPanel.Find("11GachaBtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            Gacha(gachaType, 11);
        });
        gachaPanel.Find("35GachaBtn").GetComponent<Button>().onClick.RemoveAllListeners();
        gachaPanel.Find("35GachaBtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            Gacha(gachaType, 35);
        });

        Transform grid = gachaPanel.Find("Grid");
        for (int i = 0; i < grid.childCount; i++)
        {
            Transform slot = grid.GetChild(i);
            slot.gameObject.SetActive(false);
        }

        string[] items = GetGachaItemID(gachaType, count);
        // ���⿡ ��¥ ���� �ڵ� �ֱ�
        UpdateDBGachaItem(gachaType, items);
        GetGachaExp(gachaType, count);
        StartCoroutine(GachaEffect(items));
    }
    string[] GetGachaItemID(GachaType gachaType, int count)
    {
        Dictionary<string, List<string>> IDByGrade = new Dictionary<string, List<string>>();
        // key - ��ų���, value - ��ųID
        if (gachaType == GachaType.Skill)
        {
            foreach (var item in SkillTemplate)
            {
                string grade = item.Value[(int)SkillTemplate_.Grade];
                string id = item.Value[(int)SkillTemplate_.SkillId];
                if (!IDByGrade.ContainsKey(grade))
                {
                    IDByGrade.Add(grade, new List<string>());
                    IDByGrade[grade].Add(id);
                }
                else
                {
                    IDByGrade[grade].Add(id);
                }
            }
        }
        else if (gachaType == GachaType.Item)
        {

        }

        string[] gachaData = new string[2];
        if (gachaType == GachaType.Skill)
        {
            gachaData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.Gacha_SkillData).Split('@');
        }
        else if (gachaType == GachaType.Item)
        {
            gachaData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.Gacha_ItemData).Split('@');
        }

        string key = $"{gachaType}{gachaData[0]}";

        string[] probability = new string[GachaTemplate[key].Length - 1];
        Array.Copy(GachaTemplate[key], 1, probability, 0, GachaTemplate[key].Length - 1);

        string[] result = new string[count];
        for (int i = 0; i < count; i++)
        {
            Debug.Log("Ȯ�� ������ " + (i + 1) + "ȸ");
            GachaGrade grade = CalcPercent(Array.ConvertAll(probability, int.Parse));

            int randomValue = UnityEngine.Random.Range(0, IDByGrade[grade.ToString()].Count);
            string getSkillID = IDByGrade[grade.ToString()][randomValue];

            Debug.Log($"grade : {grade}, randomValue : {randomValue}, result {getSkillID} @@@@@");
            result[i] = getSkillID;
        }

        return result;
    }

    IEnumerator GachaEffect(string[] items) // �����ۺκе� ���� ó���������..
    {
        Transform grid = gachaPanel.Find("Grid");
        for (int i = 0; i < items.Length; i++)
        {
            Transform slot = grid.GetChild(i);

            string grade = SkillTemplate[items[i]][(int)SkillTemplate_.Grade];
            string[] icon = SkillTemplate[items[i]][(int)SkillTemplate_.Icon].Split('/');
            string spriteName = icon[0];
            string atlasName = icon[1];
            slot.Find("Skill_Image").GetComponent<Image>().sprite = CommonFunction.GetSprite_Atlas(spriteName, atlasName);
            slot.Find("BgGlow").GetComponent<Image>().color = Util.ConvertGradeToColor(grade.ToString());
            slot.Find("FrameGlow").GetComponent<Image>().color = Util.ConvertGradeToColor(grade.ToString());

            Transform glowEffect = slot.Find("GlowEffect");
            for (int j = 0; j < glowEffect.childCount; j++)
                glowEffect.GetChild(j).GetComponent<Image>().color = Util.ConvertGradeToColor(grade.ToString());

            GachaGrade _grade = (GachaGrade)Enum.Parse(typeof(GachaGrade), grade);
            if ((int)GachaGrade.���� <= (int)_grade)
            {
                DOTween.Sequence()
                    .OnStart(() =>
                    {
                        slot.localScale = Vector3.zero;
                        glowEffect.localScale = Vector3.one;
                    })
                    .Join(slot.DOScale(1, 0.3f).SetEase(Ease.OutBack))
                    .Join(glowEffect.DORotate(new Vector3(0, 0, 1800), 1.5f, RotateMode.FastBeyond360))
                    .Join(glowEffect.DOScale(new Vector3(1.7f, 1.7f, 1.7f), 2))
                    .Append(glowEffect.DOScale(Vector3.zero, 1.5f));

                glowEffect.gameObject.SetActive(true);
            }
            else
            {
                DOTween.Sequence()
                 .OnStart(() => { slot.localScale = Vector3.zero; })
                 .Append(slot.DOScale(1, 0.3f).SetEase(Ease.OutBack));

                glowEffect.gameObject.SetActive(false);
            }

            slot.gameObject.SetActive(true);

            yield return CommonIEnumerator.WaitForEndOfFrame;
        }
    }
    #region ��������í
    //IEnumerator Gacha(GachaType gachaType, int count)
    //{
    //    if (gachaPanel == null)
    //        gachaPanel = CommonFunction.GetPrefabInstance("GachaUI", popupUI_1.transform).transform;
    //    else
    //        gachaPanel.gameObject.SetActive(true);

    //    GetGachaExp(gachaType, count);

    //    Transform grid = gachaPanel.Find("Grid");
    //    for (int i = 0; i < 35; i++)
    //    {
    //        Transform slot = grid.GetChild(i);
    //        slot.gameObject.SetActive(false);
    //    }

    //    // key - ��ų���, value - ��ųID
    //    Dictionary<string, List<string>> skillIDByGrade = new Dictionary<string, List<string>>();
    //    foreach (var item in SkillTemplate)
    //    {
    //        string grade = item.Value[(int)SkillTemplate_.Grade];
    //        string id = item.Value[(int)SkillTemplate_.SkillId];
    //        if (!skillIDByGrade.ContainsKey(grade))
    //        {
    //            skillIDByGrade.Add(grade, new List<string>());
    //            skillIDByGrade[grade].Add(id);
    //        }
    //        else
    //        {
    //            skillIDByGrade[grade].Add(id);
    //        }
    //    }



    //    // �� �κе� ��í������ ���� �ٸ� ���� �޵��� �����ؾߵ�.
    //    string[] probability = new string[GachaTemplate["skill1"].Length - 1];
    //    Array.Copy(GachaTemplate["skill1"], 1, probability, 0, GachaTemplate["skill1"].Length - 1);

    //    // �ڷᱸ���� ���ؼ� �� ������ �����س���? => �������� DB�� ������Ʈ?
    //    // �ٵ� �̴¼����� 1. ��ų�� ���(DB������Ʈ) 2. ����� �и��ϱ�,,,  �ֳ� --> �̱����ڸ��� �������� ��������

    //    for (int i = 0; i < count; i++)
    //    {
    //        Transform slot = grid.GetChild(i);

    //        Debug.Log("Ȯ�� ������ " + (i + 1) + "ȸ");
    //        GachaGrade grade = CalcPercent(Array.ConvertAll(probability, int.Parse));

    //        int randomValue = UnityEngine.Random.Range(0, skillIDByGrade[grade.ToString()].Count);
    //        string getSkillID = skillIDByGrade[grade.ToString()][randomValue];
    //        Debug.Log($"grade : {grade}, randomValue : {randomValue}, result {getSkillID} @@@@@");

    //        string[] icon = SkillTemplate[getSkillID][(int)SkillTemplate_.Icon].Split('/');
    //        string spriteName = icon[0];
    //        string atlasName = icon[1];
    //        slot.Find("Skill_Image").GetComponent<Image>().sprite = CommonFunction.GetSprite_Atlas(spriteName, atlasName);
    //        slot.Find("BgGlow").GetComponent<Image>().color = Util.ConvertGradeToColor(grade.ToString());
    //        slot.Find("FrameGlow").GetComponent<Image>().color = Util.ConvertGradeToColor(grade.ToString());

    //        Transform glowEffect = slot.Find("GlowEffect");
    //        int childCount = glowEffect.childCount;
    //        for (int j = 0; j < childCount; j++)
    //            glowEffect.GetChild(j).GetComponent<Image>().color = Util.ConvertGradeToColor(grade.ToString());

    //        if ((int)GachaGrade.��Ŀ�� <= (int)grade)
    //        {
    //            DOTween.Sequence()
    //                .OnStart(() =>
    //                {
    //                    slot.localScale = Vector3.zero;
    //                    glowEffect.localScale = Vector3.one;
    //                })
    //                .Join(slot.DOScale(1, 0.3f).SetEase(Ease.OutBack))
    //                .Join(glowEffect.DORotate(new Vector3(0, 0, 1800), 1.5f, RotateMode.FastBeyond360))
    //                .Join(glowEffect.DOScale(new Vector3(1.7f, 1.7f, 1.7f), 2))
    //                .Append(glowEffect.DOScale(Vector3.zero, 1.5f));

    //            glowEffect.gameObject.SetActive(true);
    //        }
    //        else
    //        {
    //            DOTween.Sequence()
    //             .OnStart(() => { slot.localScale = Vector3.zero; })
    //             .Append(slot.DOScale(1, 0.3f).SetEase(Ease.OutBack));
    //        }
    //        slot.gameObject.SetActive(true);

    //        yield return CommonIEnumerator.WaitForEndOfFrame;
    //    }


    //}
    #endregion ��������í
    private void UpdateDBGachaItem(GachaType gachaType, string[] items)
    {
        if (gachaType == GachaType.Skill)
        {
            var skillAcquireData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.SkillData).Split('@');

            for (int i = 0; i < items.Length; i++)
            {
                int skillID = int.Parse(items[i]) - 1;

                string[] data = skillAcquireData[skillID].Split(',');   // format : ����,��������
                
                int holdingCount = int.Parse(data[1]) + 1;
                data[1] = $"{holdingCount}";
                
                skillAcquireData[skillID] = string.Join(',', data);
            }

            //string[] skillData = new string[SkillTemplate.Count];
            //for (int i = 0; i < skillData.Length; i++)
            //{
            //    int skillLevel = 1;
            //    int holdingCount = 0;
            //    skillData[i] = $"{skillLevel},{holdingCount}";
            //}

            GlobalManager.Instance.DBManager.UpdateUserData(UserStringDataType.SkillData, string.Join('@', skillAcquireData));

        }
        else if (gachaType == GachaType.Item)
        {

        }
    }
    void GetGachaExp(GachaType gachaType, int count)
    {
        string[] skillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.Gacha_SkillData).Split('@');
        int level = int.Parse(skillData[0]);
        int currentValue = int.Parse(skillData[1]) + count;

        int targetValue = int.Parse(LevelTemplate[level.ToString()][(int)LevelTemplate_.Gacha_RequiredQuantity]);
        if (currentValue >= targetValue)
        {
            currentValue -= targetValue;
            level += 1;
        }

        Transform skillGacha = category5_UI.Find("Type1_Gacha/Scroll View/Viewport/Content").GetChild((int)gachaType);
        skillGacha.Find("Level_Text").GetComponent<TMP_Text>().text = $"LV. {level}";
        skillGacha.Find("Slider/CurrentValue_Text").GetComponent<TMP_Text>().text = $"{currentValue}";
        skillGacha.Find("Slider/TargetValue_Text").GetComponent<TMP_Text>().text = $"/ {targetValue}";

        if (currentValue == 0)
            skillGacha.Find("Slider").GetComponent<Slider>().value = 0;
        else
            skillGacha.Find("Slider").GetComponent<Slider>().value = currentValue / (float)targetValue;

        GlobalManager.Instance.DBManager.UpdateUserData(UserStringDataType.Gacha_SkillData, $"{level}@{currentValue}");
    }

    GachaGrade CalcPercent(int[] arr)
    {
        int[] acquisitionProbability = new int[Enum.GetValues(typeof(GachaGrade)).Length - 1];
        int threshold = 0;
        for (int i = 0; i < arr.Length; i++)
        {
            threshold += arr[i];
            acquisitionProbability[i] = threshold;
        }
        Debug.Log($"threshold : {threshold}");
        GachaGrade result = GachaGrade.None;
        int randomValue = UnityEngine.Random.Range(1, 10001);
        for (int i = 0; i < acquisitionProbability.Length; i++)
        {
            if (randomValue <= acquisitionProbability[i])
            {
                result = (GachaGrade)i;
                //Debug.Log($"randomValue : {randomValue}, Ȯ�� : {acquisitionProbability[i]}, result {result}");
                //Debug.Log("Ȯ�� ��곡########");
                return result;
            }
        }

        return result;
    }
    void ShowUI_Test(int index)
    {
        Debug.Log("index : " + index);
        Transform parent = category5_UI.Find("BottomBar/BottomMenu");
        for (int i = 0; i < parent.childCount; i++)
        {
            parent.GetChild(i).Find("BtnOn").gameObject.SetActive(false);
        }
        parent.GetChild(index).Find("BtnOn").gameObject.SetActive(true);
    }


    public void GetGoods(double getGold = 0, double getGem = 0)
    {
        if (getGold > 0)
        {
            //Debug.Log($"{Util.BigNumCalculate(getGold)}G�� ȹ��");

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
            Debug.Log($"{Util.BigNumCalculate(usedGold)}Gold�� ���");
            goldText.text = $"{Util.BigNumCalculate(currentGold - usedGold)}";
            GlobalManager.Instance.DBManager.UpdateUserData(UserDoubleDataType.Gold.ToString(), currentGold - usedGold);

            return true;
        }
        else
        {
            Debug.LogError("��尡 �����մϴ�");

            return false;
        }
    }

    public bool UseGems(double usedGem)
    {
        double currentGem = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.Gem);

        if (currentGem - usedGem >= 0)
        {
            Debug.Log($"{usedGem}Gem�� ���");
            gemText.text = Util.BigNumCalculate(currentGem - usedGem);
            GlobalManager.Instance.DBManager.UpdateUserData(UserDoubleDataType.Gem, currentGem - usedGem);


            return true;
        }
        else
        {
            Debug.LogError("���� �����մϴ�");

            return false;
        }
    }
    public void SetCurrentStageState(StageState state)
    {
        CurrentStageState = state;
    }
}
