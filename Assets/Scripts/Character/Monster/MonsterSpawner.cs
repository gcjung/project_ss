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
    private ObjectPool<Monster> monsterPool;

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
    public void StartSpawn()
    {
        if (!IsSpawning)
            StartCoroutine(SpawnMonster());
    }
    public IEnumerator SpawnMonster()
    {
        IsSpawning = true;

        int monsterCount = 0;
        float spawnTime = 1.5f;

        while (monsterCount < 9)
        {
            monsterPool.GetObjectPool().transform.localScale = monster.transform.localScale;

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
            WaveCount++;
            StartSpawn();
        }
        else if (Mathf.Abs(value - 0.4f) <= errorRange && WaveCount == 1)
        {
            WaveCount++;
            StartSpawn();
        }
        else if (Mathf.Abs(value - 0.6f) <= errorRange && WaveCount == 2)
        {
            WaveCount++;
            StartSpawn();
        }
        else if (Mathf.Abs(value - 0.8f) <= errorRange && WaveCount == 3)
        {
            WaveCount++;
            StartSpawn();
        }
        else if (Mathf.Abs(value - 1.0f) <= errorRange && WaveCount == 4)
        {
            WaveCount++;
            FinishWave();
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
