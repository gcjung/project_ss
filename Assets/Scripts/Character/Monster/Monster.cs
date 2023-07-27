using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameDataManager;

public class Monster : MonoBehaviour
{
    public string Name { get; private set; }
    public double MaxHp { get; private set; }
    public double CurrentHp { get; private set; }
    public double Attack { get; private set; }
    public double DropGold { get; private set; }

    private MonsterController monsterController;
    private BoxCollider2D boxCollider;

    public Action BossMonsterDied;
    private void OnEnable()
    {
        CurrentHp = MaxHp;

        if (boxCollider != null)
        {
            boxCollider.enabled = true;
        }

        if (monsterController != null)
        {
            monsterController.SetCurrentMonsterState(MonsterState.Idle);
        }
    }
    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        if (TryGetComponent<MonsterController>(out var controller))
        {
            monsterController = controller;
        }
    }
    public void SetMonsterStat(int monsterId)
    {
        Name = MonsterTemplate[monsterId.ToString()][(int)MonsterTemplate_.Name];
        MaxHp = double.Parse(MonsterTemplate[monsterId.ToString()][(int)MonsterTemplate_.Hp]);
        Attack = double.Parse(MonsterTemplate[monsterId.ToString()][(int)MonsterTemplate_.Attack]);
        DropGold = double.Parse(MonsterTemplate[monsterId.ToString()][(int)MonsterTemplate_.Gold]);
        CurrentHp = MaxHp;
    }


    public void TakeDamage(double damageAmount)
    {
        //Debug.Log($"{Name}가 {damageAmount}만큼 피해입음");

        CurrentHp -= damageAmount;
        
        if (CurrentHp <= 0)
        {
            MonsterDie();
        }
    }

    private void MonsterDie()
    {
        //Debug.Log($"{Name} 죽음");

        boxCollider.enabled = false;
        
        monsterController.SetCurrentMonsterState(MonsterState.Dead);
        MainScene.Instance.GetGoods(DropGold);

        if (this.gameObject.CompareTag("Boss"))
        {
            BossMonsterDied?.Invoke();
        }
    }
}
