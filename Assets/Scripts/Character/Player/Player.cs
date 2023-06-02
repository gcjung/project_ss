using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int MaxHp { get; private set; } = 300;
    public int CurrentHp { get; private set; }
    public int Attack { get; private set; } = 15;

    private PlayerController playerController;

    private void Start()
    {
        CurrentHp = MaxHp;

        playerController = GetComponent<PlayerController>();
    }

    public void TakeDamage(int damageAmount)
    {
        CurrentHp -= damageAmount;

        if (CurrentHp <= 0)
        {
            playerController.SetCurrentPlayerState(PlayerState.Dead);

            Debug.Log("플레이어 사망");
        }
    }
}
