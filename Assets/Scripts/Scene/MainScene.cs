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
using UnityEditor;

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
    private MonsterSpawner spawner; //�������� ���ÿ�

    [HideInInspector] public int heroId;    //�ӽ� �׽�Ʈ��
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

    //���� ���� �׽�Ʈ ��
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
    private Image[] skillSprite = null;
    private RectTransform category1_UI = null;  // ĳ����, ���
    private RectTransform category2_UI = null;  //���
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

        Debug.Log("���ξ� ����");

        SetCurrentStageState(StageState.NormalWave);

        stageId = (int)GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.CurrentStageId, 1);
        SetStage(stageId);  //�������� ����

        heroId = (int)GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.CurrentHeroId, 1);
        SetPlayer(heroId);  //���� ����      

        goldText = mainUiPanel.transform.Find("UpSide_Panel/Goods_Panel/Gold_Image/Gold_Text").GetComponent<TMP_Text>();
        gemText = mainUiPanel.transform.Find("UpSide_Panel/Goods_Panel/Gem_Image/Gem_Text").GetComponent<TMP_Text>();

        var skillPanel = mainUiPanel.transform.Find("UpSide_Panel/Skill_Panel");
        
        skillSprite = new Image[skillPanel.childCount -1];
        for (int i = 0; i < skillPanel.childCount - 1; i++)
            skillSprite[i] = skillPanel.GetChild(i+1).GetComponent<Image>();
        
        popupUI_0 = UIManager.instance.transform.Find("PopupUI_0").gameObject;
        popupUI_1 = UIManager.instance.transform.Find("PopupUI_1").gameObject;

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
    public void SetPlayer(int heroId)
    {
        Debug.Log($"���� ����ID : {heroId}");

        if (playerCharacter != null)
            Destroy(playerCharacter.gameObject);

        heroName = HeroTemplate[heroId.ToString()][(int)HeroTemplate_.Name];

        var _playerCharacter = Resources.Load<Player>($"Player/{heroName}");   //�÷��̾� ĳ���� ����
        playerCharacter = Instantiate(_playerCharacter, transform);
        playerCharacter.transform.position = playerSpawnPoint.position;
        playerCharacter.AddComponent<PlayerController>();
        playerCharacter.AddComponent<SkillController>();
        playerCharacter.SetHeroStatus(heroId);  //���� �⺻ ���� ����
        
        IsPlayer = true;

        UpdateStatusLevel();    //���� 1ȸ ���� ���� ������Ʈ

        var hpBar = CommonFunction.GetPrefab("Slider_HealthBar_Hero", upSidePanel.transform);   //ü�¹� ����
        hpBar.GetComponent<HpSlider>().SetTarget(playerCharacter.gameObject);
        Vector3 hpBarScale = new Vector3(55f, 55f, 1f);
        hpBar.transform.localScale = hpBarScale;
    }
    public void SetStage(int stageId)
    {
        Debug.Log($"���� ��������ID : {stageId}");

        if (stageText != null)
            Destroy(stageText.gameObject);

        stageName = StageTemplate[stageId.ToString()][(int)StageTemplate_.Stage];
        stageText = CommonFunction.GetPrefab("StageName_Text", upSidePanel.transform);
        stageText.GetComponent<TextMeshProUGUI>().text = stageName;

        mapName = StageTemplate[stageId.ToString()][(int)StageTemplate_.MapImage];  //���̹��� ����
        monsterId = int.Parse(StageTemplate[stageId.ToString()][(int)StageTemplate_.Monster]); //���� ����
        bossId = int.Parse(StageTemplate[stageId.ToString()][(int)StageTemplate_.Boss]); //�������� ����

        mapSprite = Resources.Load<Sprite>($"Sprite/{mapName}"); //�� ����
        map1.sprite = mapSprite;
        map2.sprite = mapSprite;
    }
    public void SetSkill(string[] equipedSkill)
    {
        for (int i = 0; i < equipedSkill.Length; i++)
        {
            if (string.IsNullOrEmpty(equipedSkill[i]))
                continue;

            string icon = SkillTemplate[equipedSkill[i]][(int)SkillTemplate_.Icon];

            string[] iconDatas = icon.Split('/');
            string spriteName = iconDatas[0];
            string atlasName = iconDatas[1];
            skillSprite[i].sprite = CommonFunction.GetSprite_Atlas(spriteName, atlasName);
        }
    }

    public void UpdateStatusLevel(string statusName = "", int level = 1)    //�ӽ� �޼���(������ ���� �ȵ�)
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

        var equippedSkill = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.EquippedSkill).Split('@');
        SetSkill(equippedSkill);    // ��ų ����
    }

    private void EnterBossRoom()
    {
        if (isWaveClear)
        {
            Debug.Log("������ ����");
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

        Color color = _victoryText.color;    //���İ� 0���� �ʱ�ȭ
        color.a = 0f;
        _victoryText.color = color;

        _victoryText.DOFade(1f, fadeTime).OnComplete(() =>
        _victoryText.DOFade(0f, fadeTime).OnComplete(() =>
        {
            StageClear2();
            Destroy(_victoryText.gameObject);
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
        //��� UI
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
        if (isFirst)        // ���� 1ȸ�� ����
        {
            // ��ų ���
            Transform grid = category1_UI.Find("Type2_Skill/Scroll View/Viewport/Grid");
            Util.InitGrid(grid);
            foreach (var item in SkillTemplate.OrderBy(x => x.Value[(int)SkillTemplate_.Order]))
            {
                var skill_Icon = CommonFunction.GetPrefab("Skill_Icon", grid);

                string grade = item.Value[(int)SkillTemplate_.Grade];
                string icon = item.Value[(int)SkillTemplate_.Icon];

                skill_Icon.GetComponent<Image>().color = ConvertGradeToColor(grade);
                string[] iconDatas = icon.Split('/');
                string spriteName = iconDatas[0];
                string atlasName = iconDatas[1];
                skill_Icon.transform.Find("Image").GetComponent<Image>().sprite = CommonFunction.GetSprite_Atlas(spriteName, atlasName);

                skill_Icon.GetComponent<Button>().onClick.AddListener(() =>
                {
                    var skill_Icon = CommonFunction.GetPrefab("Skill_Detail", popupUI_1.transform);

                });


            }
        }
        else
        {
            category1_UI.Find("Type2_Skill").gameObject.SetActive(true);
            category1_UI.Find("Type1_Character").gameObject.SetActive(false);

            Transform equippedSkillGroup = category1_UI.Find("Type2_Skill/EquippedSkillGroup");
            Image[] equippedSkill_Image = new Image[equippedSkillGroup.childCount];
            for (int i = 0; i < equippedSkillGroup.childCount; i++)
                equippedSkill_Image[i] = equippedSkillGroup.GetChild(i).GetComponent<Image>();

            var userEquippedSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.EquippedSkill).Split('@');
            for (int i = 0; i < userEquippedSkillData.Length; i++)
            {
                if (string.IsNullOrEmpty(userEquippedSkillData[i]))
                    continue;

                string icon = SkillTemplate[userEquippedSkillData[i]][(int)SkillTemplate_.Icon];

                string[] iconDatas = icon.Split('/');
                string spriteName = iconDatas[0];
                string atlasName = iconDatas[1];
                equippedSkill_Image[i].sprite = CommonFunction.GetSprite_Atlas(spriteName, atlasName);
            }
        }

    }

    Color ConvertGradeToColor(string grade)
    {
        if(grade == "common")
        {
            return Color.white;
        }
        else if(grade == "uncommon")
        {
            return Color.green;
        }
        else if (grade == "rare")
        {
            return Color.cyan;
        }
        else
        {
            return Color.red;
        }
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
