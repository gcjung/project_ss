using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{ 
    public static PlayerState CurrentPlayerState { get; private set; } = PlayerState.Idle;
    private Animator playerAnimator;
    private Player player;

    private List<MonsterController> enemyList = new List<MonsterController>();
  
    private Bullet projectilePrefab;    //투사체 세팅
    private ObjectPool<Bullet> bulletPool;
    private float projectileSpeed = 5.0f;
    private Transform target;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<MonsterController>(out var monster))
        {
            enemyList.Add(monster);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.TryGetComponent<MonsterController>(out var monster))
        {
            if ((enemyList.Count - 1) == 0 && MonsterSpawner.MonsterCount == 0)
            {
                StartCoroutine(DelayChangeState(monster));
            }
            else
            {
                enemyList.Remove(monster);
            }         
        }
    }

    void Start()
    {
        playerAnimator = transform.GetComponent<Animator>();

        projectilePrefab = Resources.Load<Bullet>("ETC/(TEST)projectile");
        if (projectilePrefab != null)
        {
            bulletPool = new ObjectPool<Bullet>(projectilePrefab, 30, this.transform);
        }
            
        if (TryGetComponent<Player>(out var value))
        {
            player = value;
        }           
    }

    void Update()
    {
        switch (CurrentPlayerState)
        {
            case PlayerState.Idle:
                if (!CanSeeTarget())
                {
                    CurrentPlayerState = PlayerState.Moving;                   
                }
                else
                {
                    CurrentPlayerState = PlayerState.Idle;

                    ResetAllTriggers();
                    playerAnimator.SetTrigger("Idle");
                }
                break;
            case PlayerState.Moving:
                if (CanSeeTarget())
                {
                    CurrentPlayerState = PlayerState.Attacking;
                }
                else
                {
                    playerAnimator.SetTrigger("Run");
                }
                break;
            case PlayerState.Attacking:
                if (CanSeeTarget())
                {
                    playerAnimator.SetTrigger("Attack");

                    float animationSpeed = (float)player.TotalAttackSpeed / 1.0f;
                    playerAnimator.SetFloat("AttackSpeedMultiplier", animationSpeed);
                }
                else if(!CanSeeTarget() && !MonsterSpawner.IsSpawning)
                {
                    CurrentPlayerState = PlayerState.Moving;
                }
                break;
            case PlayerState.Dead:
                playerAnimator.SetTrigger("Dead");
                break;
        }
    }

    private bool CanSeeTarget()
    {
        if (enemyList.Count == 0)
            return false;
        else
            return true;
    }

    
    public void ShootProjectile()
    {
        if (enemyList.Count > 0)
        {
            double damage;
            bool isCritical;
            double critical = UnityEngine.Random.value;            

            if(critical <= player.TotalCritical)
            {
                damage = player.TotalAttack * 1.5f;
                isCritical = true;
            }
            else
            {
                damage = player.TotalAttack;
                isCritical = false;
            }

            target = enemyList[0].transform;

            Vector3 direction = (target.position - transform.position).normalized;

            Bullet bullet = bulletPool.GetObjectPool();
            bullet.transform.localScale = projectilePrefab.transform.localScale;
            bullet.SettingInfo(damage, isCritical, direction, projectileSpeed);
            //Debug.Log($"{bullet.Damage}");
            //bullet.Rigid.velocity = direction * projectileSpeed;      
        }
        else
        {
            Debug.LogError("적이 없음");
        }
    }

    public void SetCurrentPlayerState(PlayerState state)
    {
        CurrentPlayerState = state;
    }

    private IEnumerator DelayChangeState(MonsterController enemy)
    {
        float delayTime = 2.0f;

        yield return CommonIEnumerator.IETimeout(
            onStart: () => { SetCurrentPlayerState(PlayerState.Idle); },
            onFinish: () => { enemyList.Remove(enemy); }, delayTime);
    }

    private void ResetAllTriggers()
    {
        foreach (var param in playerAnimator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                playerAnimator.ResetTrigger(param.name);
            }
        }
    }
}
