using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameDataManager;
public class MonsterSpawner : MonoBehaviour
{
    [Header("Stage Slider")]
    [SerializeField] private Slider slider;

    [Header("Monster Spawn Point")]
    [SerializeField] private Transform spawnPoint1;
    [SerializeField] private Transform spawnPoint2;
    [SerializeField] private Transform spawnPoint3;

    private MainScene mainScene;

    private Monster monster;
    private Monster bossMonster;
    private ObjectPool<Monster> monsterPool;
    private ObjectPool<Monster> bossMonsterPool;

    public static bool IsSpawning { get; set; } = false;
    public static int MonsterCount { get; private set; } = 0;
    public static int WaveCount { get; private set; } = 0;

    private void Awake()
    {
        mainScene = transform.parent.GetComponent<MainScene>();

        slider = slider.GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnSliderValueChanged);
        slider.interactable = false;
    }
    private IEnumerator Start()
    {
        yield return CommonIEnumerator.IEWaitUntil(
           predicate: () => { return mainScene.IsPlayer; }, //GlobalManager.Instance.Initialized
           onFinish: () =>
           {
               Init();
           });
    }
    void Init()
    {
        SetMonster();
    }

    public void SetMonster()
    {
        ClearWaveTrigger();

        if (!slider.IsActive())
            slider.gameObject.SetActive(true);

        string monsterName = MonsterTemplate[mainScene.monsterId.ToString()][(int)MonsterTemplate_.Name];
        string bossName = MonsterTemplate[mainScene.bossId.ToString()][(int)MonsterTemplate_.Name];

        {//일반몬스터 세팅   
            monster = Resources.Load<Monster>($"Monster/{monsterName}");          
            var _monster = Instantiate(monster, transform);
            _monster.gameObject.AddComponent<MonsterController>();

            if (monsterPool != null)
                Destroy(monsterPool.TrPool.gameObject);

            monsterPool = new ObjectPool<Monster>(_monster, 9, this.transform);           
            Destroy(_monster.gameObject);
        }

        {//보스몬스터 세팅
            bossMonster = Resources.Load<Monster>($"Monster/{bossName}");     
            var _bossMonster = Instantiate(bossMonster, transform);
            _bossMonster.gameObject.AddComponent<MonsterController>();           

            if (bossMonsterPool != null)
                Destroy(bossMonsterPool.TrPool.gameObject);

            bossMonsterPool = new ObjectPool<Monster>(_bossMonster, 1, this.transform);
            Destroy(_bossMonster.gameObject);
        }        
    }
    private IEnumerator SpawnMonster()
    {
        IsSpawning = true;
        MonsterCount = 6;

        float spawnTime = 1.5f;

        while (MonsterCount != 0)
        {
            int randomIndex = Random.Range(1, 4);
            Transform spawnPoint = GetSpawnPoint(randomIndex);

            //monsterPool.GetObjectPool().transform.localScale = monster.transform.localScale;
            var _monster = monsterPool.GetObjectPool();
            _monster.SetMonsterStat(mainScene.monsterId);
            _monster.transform.localScale = monster.transform.localScale;
            _monster.transform.position = spawnPoint.position;

            MonsterCount--;

            yield return new WaitForSeconds(spawnTime);
           
        }

        IsSpawning = false;
    }
    public void SpawnBossMonster()
    {
        slider.gameObject.SetActive(false);

        var _bossMonster = bossMonsterPool.GetObjectPool();
        _bossMonster.SetMonsterStat(mainScene.bossId);
        _bossMonster.transform.localScale = bossMonster.transform.localScale;
        _bossMonster.transform.position = spawnPoint2.position;
        _bossMonster.GetComponent<Monster>().BossMonsterDied += FinishStage;
    }
    public void OnSliderValueChanged(float value)
    {
        if (Mathf.Approximately(value, 0.2f) && WaveCount == 0)
        {
            Debug.Log("웨이브1 실행");
            WaveCount++;    // 1
            StartCoroutine(SpawnMonster());
        }
        else if (Mathf.Approximately(value, 0.4f) && WaveCount == 1)
        {
            Debug.Log("웨이브2 실행");
            WaveCount++;    // 2
            StartCoroutine(SpawnMonster());
        }
        else if (Mathf.Approximately(value, 0.6f) && WaveCount == 2)
        {
            Debug.Log("웨이브3 실행");
            WaveCount++;    // 3
            StartCoroutine(SpawnMonster());
        }
        else if (Mathf.Approximately(value, 0.8f) && WaveCount == 3)
        {
            Debug.Log("웨이브4 실행");
            WaveCount++;    // 4
            StartCoroutine(SpawnMonster());
        }
        else if (Mathf.Approximately(value, 1.0f) && WaveCount == 4)
        {
            Debug.Log("웨이브5 실행");
            WaveCount++;    // 5
            FinishWave();
        }
    }
    private Transform GetSpawnPoint(int index)
    {
        switch (index)
        {
            case 1:
                return spawnPoint1;
            case 2:
                return spawnPoint2;
            case 3:
                return spawnPoint3;
            default:
                return null;
        }
    }
    public void FinishWave()
    {
        mainScene.IsWaveClear = true;
    }
    public void FinishStage()
    {
        //Debug.Log("스테이지 클리어");
        mainScene.IsStageClear = true;
    }
    public void ClearWaveTrigger()
    {
        WaveCount = 0;
    }
}
