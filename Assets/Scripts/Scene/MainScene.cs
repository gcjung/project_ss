using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScene : MonoBehaviour
{
    [Header("Create Player")]
    private GameObject playerCharacter;
    [SerializeField] private Transform playerSpawnPoint;

    [Header("Change Map Sprite")]
    private Sprite mapSprite;
    [SerializeField] private SpriteRenderer mapOne;
    [SerializeField] private SpriteRenderer mapTwo;
    
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
        Debug.Log("메인 씬시작");

        playerCharacter = Resources.Load<GameObject>("Player/ch001");   //플레이어 캐릭터 세팅
        var _playerCharacter = Instantiate(playerCharacter, transform);
        _playerCharacter.transform.position = playerSpawnPoint.position;
        _playerCharacter.AddComponent<Player>();
        //_playerCharacter.AddComponent<PlayerController>();  //AddComponent 순서 중요
        IsPlayer = true;


        mapSprite = Resources.Load<Sprite>("Sprite/Map001"); //맵 세팅
        mapOne.sprite = mapSprite;
        mapTwo.sprite = mapSprite;
    }

    /*
         [SerializeField]
    private GameObject mainUiPanel;

    private TMP_Text goldText;
    private TMP_Text gemText;
    
    async void Start()
    {
        //goldText = mainUiPanel.transform.Find("UpSide_Panel/Goods_Panel/Gold_Image/Gold_Text").GetComponent<TMP_Text>();
        //gemText = mainUiPanel.transform.Find("UpSide_Panel/Goods_Panel/Gem_Image/Gem_Text").GetComponent<TMP_Text>();

        //StartCoroutine(teeest());
        //InvokeRepeating(nameof(GetGold), 3f,3f);


        //bool gem = await DBManager.Instance.GetFirebaseUserData<bool>("Gem");
        //Debug.Log("gem : " + gem);

        //int gem2 = await DBManager.Instance.GetFirebaseUserData<int>("Gem22");
        //Debug.Log("gem22 : " + gem2);
        //DBManager.Instance.UpdateFirebaseUserData("Test", 120);
    }

    public void GetGoods(int getGold = 0, int getGem = 0)
    {
        
        if (getGold > 0)
        {
            double currentGold = GlobalManager.Instance.DBManager.GetUserDoubleData("Gold");
            //DBManager.Instance.GetUserIntData("Gold");

            goldText.text = $"{(currentGold + getGold)}";
            GlobalManager.Instance.DBManager.UpdateUserData("Gold", currentGold + getGold);
        }

        if (getGem > 0) 
        {
            double currentGem = GlobalManager.Instance.DBManager.GetUserDoubleData("Gem");
            gemText.text = $"{(currentGem + getGem)}";
            GlobalManager.Instance.DBManager.UpdateUserData("Gem", currentGem + getGem);
        }

    }
     */


}
