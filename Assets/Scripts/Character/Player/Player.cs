using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public double MaxHp { get; private set; } = 300;
    public double CurrentHp { get; private set; }
    public double Attack { get; private set; } = 50;

    private PlayerController playerController;

    private void Start()
    {
        CurrentHp = MaxHp;

        playerController = GetComponent<PlayerController>();
    }

    public void TakeDamage(double damageAmount)
    {
        CurrentHp -= damageAmount;

        if (CurrentHp <= 0)
        {
            playerController.SetCurrentPlayerState(PlayerState.Dead);

            Debug.Log("플레이어 사망");
        }
    }
}
