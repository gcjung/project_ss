using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.Events;

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

    [Header("Create Player")]
    private GameObject playerCharacter;

    [SerializeField] private Transform playerSpawnPoint;

    [Header("Change Map Sprite")]   
    [SerializeField] private SpriteRenderer map1;
    [SerializeField] private SpriteRenderer map2;
    private Sprite mapSprite;

    [Header("Enter Boss Room")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private UpSidePanel upSidePanel;
    [SerializeField] private Image vsImage;
    [SerializeField] private Transform playerPosition;
    [SerializeField] private Transform bossPosition;
    private GameObject playerPref;
    private GameObject bossPref;
    private float fadeInTime = 1f;
    private float delayTime = 1f;

    private MonsterSpawner spawner;

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


    [SerializeField] private GameObject mainUiPanel;
    private TMP_Text goldText;
    private TMP_Text gemText;
    private RectTransform category1Panel;
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

        spawner = Utill.FindChildComponent<MonsterSpawner>(transform);
    }

    private void Init()
    {
        if (instance == null)
            instance = this;

        Debug.Log("메인 씬시작");
        
        playerCharacter = Resources.Load<GameObject>("Player/ch001");   //플레이어 캐릭터 세팅
        var _playerCharacter = Instantiate(playerCharacter, transform);
        _playerCharacter.transform.position = playerSpawnPoint.position;
        _playerCharacter.AddComponent<Player>();
        _playerCharacter.AddComponent<PlayerController>();
        IsPlayer = true;

        mapSprite = Resources.Load<Sprite>("Sprite/Map001"); //맵 세팅

        goldText = mainUiPanel.transform.Find("UpSide_Panel/Goods_Panel/Gold_Image/Gold_Text").GetComponent<TMP_Text>();
        gemText = mainUiPanel.transform.Find("UpSide_Panel/Goods_Panel/Gem_Image/Gem_Text").GetComponent<TMP_Text>();

        category1Panel = mainUiPanel.transform.Find("DownSide_Panel/Category1_Panel").GetComponent<RectTransform>();

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

    private void InitUIfromDB()
    {
        Debug.Log("InitUIfromDB");
        goldText.text = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.Gold).ToString();
        gemText.text = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.Gem).ToString();
    }

    private void EnterBossStage()
    {
        if (isStageClear)
        {
            Debug.Log("보스방 입장");
            IsStageClear = false;

            fadeImage = Instantiate(fadeImage, upSidePanel.transform);
            //fadeImage.transform.SetAsFirstSibling();
            FadeIn();
        }
    }
    int invisiblePosY = -1211;
    int visiblePosY = 487;
    private void OnClickCategory(int categoryType, GameObject onClickButton)
    {
        switch (categoryType)
        {
            case 0:
                ShowCategory1UI(category1Panel.gameObject.activeSelf, onClickButton);
                break;
        }
    }

    void ShowCategory1UI(bool active, GameObject onClickButton)
    {
        category1Panel.gameObject.SetActive(!active);
        onClickButton.transform.Find("Text").gameObject.SetActive(active);
        onClickButton.transform.Find("CloseImage").gameObject.SetActive(!active);

        if (active)
        {
            category1Panel.anchoredPosition = new Vector2(0, invisiblePosY);
        }
        else
        {
            category1Panel.DOAnchorPosY(visiblePosY, 0.3f).SetEase(Ease.OutExpo);
        }


        Transform parent = category1Panel.Find("BottomMenu");
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
        category1Panel.Find("Type1_Character").gameObject.SetActive(true);
        category1Panel.Find("Type2_Skill").gameObject.SetActive(false);
    }

    void ShowUI_Skill()
    {
        Debug.Log("ShowUI_Skill");
        category1Panel.Find("Type2_Skill").gameObject.SetActive(true);
        category1Panel.Find("Type1_Character").gameObject.SetActive(false);
    }


    private void FadeIn()
    {
        fadeImage.DOFade(1f, fadeInTime).OnComplete(() => Invoke("VersusSetting", delayTime));
    }

    private void VersusSetting()
    {
        int targetLayer = LayerMask.NameToLayer("Over UI");

        vsImage = Instantiate(vsImage, upSidePanel.transform);

        var player = Resources.Load<GameObject>("Player/ch001");
        playerPref = Instantiate(player, playerPosition);
        playerPref.transform.position = playerPosition.position;
        ChangeLayer(playerPref, targetLayer);

        var boss = Resources.Load<GameObject>("Monster/ch009");
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
