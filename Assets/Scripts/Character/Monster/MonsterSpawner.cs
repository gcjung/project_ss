using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MonsterSpawner : MonoBehaviour
{
    [SerializeField]
    private Slider slider;

    private MainScene mainScene;

    private Monster monster;
    private Monster bossMonster;
    private ObjectPool<Monster> monsterPool;
    private ObjectPool<Monster> bossMonsterPool;

    [Header("Monster Spawn Point")]
    [SerializeField] private Transform spawnPoint1;
    [SerializeField] private Transform spawnPoint2;
    [SerializeField] private Transform spawnPoint3;

    private bool IsSpawning { get; set; } = false;

    public static int WaveCount { get; private set; } = 0;

    private void Start()
    {
        monster = Resources.Load<Monster>("Monster/ch008");     //일반몬스터 세팅        
        var _monster = Instantiate(monster, transform);
        _monster.gameObject.AddComponent<MonsterController>();
        _monster.gameObject.SetActive(false);
        monsterPool = new ObjectPool<Monster>(_monster, 9, this.transform);

        bossMonster = Resources.Load<Monster>("Monster/ch009");     //보스몬스터 세팅
        var _bossMonster = Instantiate(bossMonster, transform);
        _bossMonster.gameObject.AddComponent<MonsterController>();
        _bossMonster.gameObject.SetActive(false);
        bossMonsterPool = new ObjectPool<Monster>(_bossMonster, 1, this.transform);

        mainScene = transform.parent.GetComponent<MainScene>();

        slider = slider.GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnSliderValueChanged);
        slider.interactable = false;
    }
    private IEnumerator SpawnMonster()
    {
        IsSpawning = true;

        int monsterCount = 0;
        float spawnTime = 1.5f;

        while (monsterCount < 6)
        {
            int randomIndex = Random.Range(1, 4);
            Transform spawnPoint = GetSpawnPoint(randomIndex);

            //monsterPool.GetObjectPool().transform.localScale = monster.transform.localScale;
            var _monster = monsterPool.GetObjectPool();
            _monster.transform.localScale = monster.transform.localScale;
            _monster.transform.position = spawnPoint.position;

            yield return new WaitForSeconds(spawnTime);

            monsterCount++;
        }

        IsSpawning = false;
    }
    public void SpawnBossMonster()
    {
        slider.gameObject.SetActive(false);

        var _bossMonster = bossMonsterPool.GetObjectPool();
        _bossMonster.transform.localScale = bossMonster.transform.localScale;
        _bossMonster.transform.position = spawnPoint2.position;
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
        mainScene.IsStageClear = true;
    }

    public void ClearWaveTrigger()
    {
        WaveCount = 0;
    }
}
