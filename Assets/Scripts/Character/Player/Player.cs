using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static GameDataManager;
public class Player : MonoBehaviour, IHpProvider
{
    public event Action<double, double> OnHealthChanged;
    public double Attack { get; private set; }
    public double TotalAttack { get; private set; }
    public double AttackSpeed { get; private set; }
    public double TotalAttackSpeed { get; private set; }
    public double Critical { get; private set; }
    public double TotalCritical { get; private set; }
    public double Hp { get; private set; }  // ĳ���� �⺻ ü��

    private double currentHp;   //ĳ���� ���� ü��
    public double CurrentHp
    {
        get { return currentHp; }
        private set
        {
            if (value >= TotalHp)
            {
                currentHp = TotalHp;
            }
            else if (value <= 0)
            {
                currentHp = 0;
                PlayerDie();
            }
            else
            {
                currentHp = value;
            }

            OnHealthChanged?.Invoke(currentHp, TotalHp);
        }
    }
    public double TotalHp { get; private set; } //ĳ���� ���� ü��


    private PlayerController playerController;
    private SkinnedMeshRenderer skinnedMesh;
    private Color originalColor;

    private void Start()
    {
        skinnedMesh = transform.GetComponentInChildren<SkinnedMeshRenderer>();
        originalColor = skinnedMesh.material.color;

        if (TryGetComponent<PlayerController>(out var controller))
        {
            playerController = controller;
        }       
    }

    public void TakeDamage(double damageAmount)
    {
        Debug.Log($"�÷��̾ {damageAmount}��ŭ ���ظ� ����.");
        Debug.Log($"���� ü�� : {CurrentHp}");

        CurrentHp -= damageAmount;

        StartCoroutine(DamageEffect());
    }
    private IEnumerator DamageEffect()
    {
        float delayTime = 0.1f;
        Color damageColor = Color.red;

        skinnedMesh.material.color = damageColor;

        yield return new WaitForSeconds(delayTime);

        skinnedMesh.material.color = originalColor;
    }
    private void PlayerDie()
    {
        playerController.SetCurrentPlayerState(PlayerState.Dead);

        Debug.Log("�÷��̾� ���");
    }

    public void SetPlayerDead()     //�ִϸ��̼� �̺�Ʈ�� ��� ��
    {
        MainScene.Instance.IsPlayerDead = true;
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

                Debug.Log($"���ݷ� : {TotalAttack}");
                break;
            case "AttackSpeedLevel":
                TotalAttackSpeed = Math.Round(AttackSpeed + (attackSpeedLevel * attackSpeed_ratio), 2); //���ݼӵ��� �տ���

                Debug.Log($"���ݼӵ� : {TotalAttackSpeed}");
                break;
            case "CriticalLevel":               
                TotalCritical = Math.Round(Critical * ciriticalLevel * critical_ratio, 2);

                Debug.Log($"ũ��Ƽ�� : {TotalCritical}");
                break;
            case "HpLevel":                
                TotalHp = Math.Round(Hp * hpLevel * hp_ratio, 2);

                Debug.Log($"�ִ�ü�� : {TotalHp}");
                break;
            default:    //ĳ���� ù ���� or ��ü �ÿ��� ����
                TotalAttack = Math.Round(Attack * attackLevel * attack_ratio, 2);
                TotalAttackSpeed = Math.Round(AttackSpeed + (attackSpeedLevel * attackSpeed_ratio), 2);
                TotalCritical = Math.Round(Critical * ciriticalLevel * critical_ratio, 2);
                TotalHp = Math.Round(Hp * hpLevel * hp_ratio, 2);
                CurrentHp = TotalHp;
                break;
        }       
    }
}
