using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{ 
    public static PlayerState CurrentPlayerState { get; private set; }
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

            Debug.Log($"현재 감지된 몬스터 수 : {enemyList.Count}");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.TryGetComponent<MonsterController>(out var monster))
        {
            if ((enemyList.Count - 1) == 0 && !MonsterSpawner.IsSpawning)
            {
                StartCoroutine(DelayChangeState(monster));
            }
            else
            {
                enemyList.Remove(monster);
            }

            Debug.Log($"현재 감지된 몬스터 수 : {enemyList.Count}");
        }
    }

    void Start()
    {
        CurrentPlayerState = PlayerState.Idle;

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
                    if (MonsterSpawner.IsSpawning)
                    {
                        CurrentPlayerState = PlayerState.Idle;
                        playerAnimator.SetTrigger("Idle");
                    }
                    else if (!MonsterSpawner.IsSpawning)
                    {
                        CurrentPlayerState = PlayerState.Moving;
                        playerAnimator.SetTrigger("Run");
                    }
                }
                else
                {
                    if (MonsterSpawner.IsSpawning)
                    {
                        CurrentPlayerState = PlayerState.Attacking;
                        playerAnimator.SetTrigger("Attack");
                    }
                    else if (!MonsterSpawner.IsSpawning)
                    {
                        CurrentPlayerState = PlayerState.Idle;
                        playerAnimator.SetTrigger("Idle");
                    }                   
                }
                break;
            case PlayerState.Moving:
                if (CanSeeTarget())
                {
                    CurrentPlayerState = PlayerState.Attacking;
                    playerAnimator.SetTrigger("Attack");
                }
                else
                {
                    CurrentPlayerState = PlayerState.Moving;
                    playerAnimator.SetTrigger("Run");
                }
                break;
            case PlayerState.Attacking:
                if (CanSeeTarget())
                {
                    CurrentPlayerState = PlayerState.Attacking;
                    playerAnimator.SetTrigger("Attack");

                    float animationSpeed = (float)player.TotalAttackSpeed / 1.0f;
                    playerAnimator.SetFloat("AttackSpeedMultiplier", animationSpeed);
                }
                else if (!CanSeeTarget())
                {
                    if (MonsterSpawner.IsSpawning)
                    {
                        CurrentPlayerState = PlayerState.Idle;
                        playerAnimator.SetTrigger("Idle");
                    }
                    else if (!MonsterSpawner.IsSpawning)
                    {
                        CurrentPlayerState = PlayerState.Moving;
                        playerAnimator.SetTrigger("Run");
                    }
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
            target = enemyList[0].transform;

            Vector3 direction = (target.position - transform.position).normalized;

            Bullet bullet = bulletPool.GetObjectPool();
            bullet.transform.localScale = projectilePrefab.transform.localScale;
            bullet.SettingInfo(player.TotalAttack, Critical(player.TotalCritical), direction, projectileSpeed);
            //bullet.Rigid.velocity = direction * projectileSpeed;      
        }
        else
        {
            Debug.LogError("적이 없음");
        }
    }
    public bool Critical(double critical)
    {
        bool isCritical;
        double criticalPercent = UnityEngine.Random.value;

        if (criticalPercent <= critical)
        {
            isCritical = true;
        }
        else
        {
            isCritical = false;
        }

        return isCritical;
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
