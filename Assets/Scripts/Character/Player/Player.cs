using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static GameDataManager;
public class Player : MonoBehaviour
{
    public double Attack { get; private set; }
    public double TotalAttack { get; private set; }
    public double AttackSpeed { get; private set; }
    public double TotalAttackSpeed { get; private set; }
    public double Critical { get; private set; }
    public double TotalCritical { get; private set; }
    public double Hp { get; private set; }

    private double currentHp;
    public double CurrentHp
    {
        get { return currentHp; }
        private set
        {
            if (value > TotalHp)
            {
                currentHp = TotalHp;
            }
            else
            {
                currentHp = value;
            }
        }
    }
    public double TotalHp { get; private set; }


    private PlayerController playerController;

    private void Start()
    {       
        if(TryGetComponent<PlayerController>(out var controller))
        {
            playerController = controller;
        }
    }

    public void TakeDamage(double damageAmount)
    {
        CurrentHp -= damageAmount;

        if (CurrentHp <= 0)
        {
            PlayerDie();
        }
    }

    private void PlayerDie()
    {
        playerController.SetCurrentPlayerState(PlayerState.Dead);

        Debug.Log("플레이어 사망");
    }

    public void SetHeroStatus(int heroId)
    {
        Attack = double.Parse(HeroTemplate[heroId.ToString()][(int)HeroTemplate_.Attack]);       
        AttackSpeed = double.Parse(HeroTemplate[heroId.ToString()][(int)HeroTemplate_.AttackSpeed]);
        Critical = double.Parse(HeroTemplate[heroId.ToString()][(int)HeroTemplate_.Critical]);
        Hp = double.Parse(HeroTemplate[heroId.ToString()][(int)HeroTemplate_.Hp]);
    }

    public void UpdateHeroStatus(string statusName = "")
    {
        int attackLevel = MainScene.Instance.attackLevel;
        float attack_ratio = MainScene.Instance.attack_ratio;

        int attackSpeedLevel = MainScene.Instance.attackSpeedLevel;
        float attackSpeed_ratio = MainScene.Instance.attackSpeed_ratio;

        int ciriticalLevel = MainScene.Instance.criticalLevel;
        float critical_ratio = MainScene.Instance.critical_ratio;

        int hpLevel = MainScene.Instance.hpLevel;
        float hp_ratio = MainScene.Instance.hp_ratio;

        switch (statusName)
        {
            case "AttackLevel":                
                TotalAttack = Math.Round(Attack * attackLevel * attack_ratio, 2);
                break;
            case "AttackSpeedLevel":                
                TotalAttackSpeed = Math.Round(AttackSpeed * attackSpeedLevel * attackSpeed_ratio, 2);
                break;
            case "CriticalLevel":               
                TotalCritical = Math.Round(Critical * ciriticalLevel * critical_ratio, 2);
                break;
            case "HpLevel":                
                TotalHp = Math.Round(Hp * hpLevel * hp_ratio, 2);
                break;
            default:    //캐릭터 첫 세팅 or 교체 시에만 실행
                TotalAttack = Math.Round(Attack * attackLevel * attack_ratio, 2);
                TotalAttackSpeed = Math.Round(AttackSpeed * attackSpeedLevel * attackSpeed_ratio, 2);
                TotalCritical = Math.Round(Critical * ciriticalLevel * critical_ratio, 2);
                TotalHp = Math.Round(Hp * hpLevel * hp_ratio, 2);
                CurrentHp = TotalHp;
                break;
        }
    }
}
