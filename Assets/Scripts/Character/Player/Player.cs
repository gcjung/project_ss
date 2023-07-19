using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public double Attack { get; private set; }
    public double AttackSpeed { get; private set; }
    public double Critical { get; private set; }
    public double MaxHp { get; private set; }

    private double currentHp;
    public double CurrentHp
    {
        get { return currentHp; }
        private set
        {
            if (value > MaxHp)
            {
                currentHp = MaxHp;
            }
            else
            {
                currentHp = value;
            }
        }
    }
    public double TotalAttack { get; private set; }

    private PlayerController playerController;

    private void Start()
    {       
        playerController = GetComponent<PlayerController>();
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

    public void SetHeroStatus(double attack, double attackSpeed, double critical, double hp)
    {
        Attack = attack;
        AttackSpeed = attackSpeed;
        Critical = critical;
        MaxHp = hp;
        CurrentHp = MaxHp;
    }
}
