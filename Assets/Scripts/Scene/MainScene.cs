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
        Debug.Log("���� ������");

        playerCharacter = Resources.Load<GameObject>("Player/ch001");   //�÷��̾� ĳ���� ����
        var _playerCharacter = Instantiate(playerCharacter, transform);
        _playerCharacter.transform.position = playerSpawnPoint.position;
        _playerCharacter.AddComponent<Player>();
        //_playerCharacter.AddComponent<PlayerController>();  //AddComponent ���� �߿�
        IsPlayer = true;


        mapSprite = Resources.Load<Sprite>("Sprite/Map001"); //�� ����
        mapOne.sprite = mapSprite;
        mapTwo.sprite = mapSprite;
    }
}
