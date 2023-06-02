using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawn : MonoBehaviour
{
    private Monster monster;

    private ObjectPool<Monster> monsterPool;

    private bool IsRunning { get; set; } = false;

    private void Start()
    {
        monster = Resources.Load<Monster>("Monster/ch008");

        var _monster = Instantiate(monster, transform);
        _monster.gameObject.AddComponent<MonsterController>();
        _monster.gameObject.SetActive(false);

        monsterPool = new ObjectPool<Monster>(_monster, 9, this.transform);
    }
    public void StartSpawn()
    {
        if (!IsRunning)
            StartCoroutine(SpawnMonster());
    }
    public IEnumerator SpawnMonster()
    {
        IsRunning = true;

        int monsterCount = 0;
        float spawnTime = 1.5f;

        while (monsterCount < 9)
        {
            monsterPool.GetObjectPool().transform.localScale = monster.transform.localScale;

            yield return new WaitForSeconds(spawnTime);

            monsterCount++;
        }

        IsRunning = false;
    }
}
