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

    [Header("Monster Spawn Point")]
    [SerializeField] private Transform spawnPoint1;
    [SerializeField] private Transform spawnPoint2;
    [SerializeField] private Transform spawnPoint3;

    private bool IsSpawning { get; set; } = false;

    public static int WaveCount { get; private set; } = 0;

    private void Start()
    {
        monster = Resources.Load<Monster>("Monster/ch008");

        var _monster = Instantiate(monster, transform);
        _monster.gameObject.AddComponent<MonsterController>();
        _monster.gameObject.SetActive(false);

        monsterPool = new ObjectPool<Monster>(_monster, 9, this.transform);

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
    public void OnSliderValueChanged(float value)
    {
        float errorRange = 0.001f;

        if (Mathf.Abs(value - 0.2f) <= errorRange && WaveCount == 0)
        {
            WaveCount++;    // 1
            StartCoroutine(SpawnMonster());
        }
        else if (Mathf.Abs(value - 0.4f) <= errorRange && WaveCount == 1)
        {
            WaveCount++;    // 2
            StartCoroutine(SpawnMonster());
        }
        else if (Mathf.Abs(value - 0.6f) <= errorRange && WaveCount == 2)
        {
            WaveCount++;    // 3
            StartCoroutine(SpawnMonster());
        }
        else if (Mathf.Abs(value - 0.8f) <= errorRange && WaveCount == 3)
        {
            WaveCount++;    // 4
            StartCoroutine(SpawnMonster());
        }
        else if (Mathf.Abs(value - 1.0f) <= errorRange && WaveCount == 4)
        {
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
