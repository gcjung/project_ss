using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameDataManager;

public class Monster : MonoBehaviour, IHpProvider
{
    public event Action<double, double> OnHealthChanged;
    public string Name { get; private set; }
    public double MaxHp { get; private set; }
    private double currentHp;
    public double CurrentHp
    {
        get { return currentHp; }
        private set
        {
            if (value >= MaxHp)
            {
                currentHp = MaxHp;
            }
            else if (value <= 0)
            {
                currentHp = 0;
                MonsterDie();
            }
            else
            {
                currentHp = value;
            }

            OnHealthChanged?.Invoke(currentHp, MaxHp);
        }
    }
    public double Attack { get; private set; }
    public double DropGold { get; private set; }

    private MonsterController monsterController;
    private BoxCollider2D boxCollider;
    private SkinnedMeshRenderer skinnedMesh;

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
        skinnedMesh = transform.GetComponentInChildren<SkinnedMeshRenderer>();

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
        CurrentHp -= damageAmount;
    }

    private void MonsterDie()
    {
        boxCollider.enabled = false;
        
        monsterController.SetCurrentMonsterState(MonsterState.Dead);
        MainScene.Instance.GetGoods(DropGold);

        if (this.gameObject.CompareTag("Boss"))
        {
            BossMonsterDied?.Invoke();
        }
    }
}
