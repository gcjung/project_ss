using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MonsterController : MonoBehaviour
{
    public MonsterState CurrentMonsterState { get; private set; } = MonsterState.Idle;

    private Animator monsterAnimator;

    private GameObject player;
    private Player target;

    private Monster monster;

    private float moveSpeed = 1f;
    private float attackRange = 1f;

    private void Awake()
    {
        monster = GetComponent<Monster>();
        monsterAnimator = GetComponent<Animator>();
    }
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        if (player.gameObject.TryGetComponent<Player>(out var value))
        {
            target = value;
        }
    }

    void Update()
    {
        switch (CurrentMonsterState)
        {
            case MonsterState.Idle:
                if (CanSeeTarget())
                {
                    CurrentMonsterState = MonsterState.Moving;
                }
                break;
            case MonsterState.Moving:
                if (CanSeeTarget())
                {
                    ResetAllTriggers();
                    monsterAnimator.SetTrigger("Walk");

                    transform.position = Vector3.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.deltaTime);
                    if (Mathf.Abs(Vector3.Distance(transform.position, player.transform.position)) <= attackRange)
                    {
                        CurrentMonsterState = MonsterState.Attacking;
                    }
                }
                else
                {
                    CurrentMonsterState = MonsterState.Idle;
                }
                break;
            case MonsterState.Attacking:
                if (CanSeeTarget())
                {
                    ResetAllTriggers();
                    monsterAnimator.SetTrigger("Attack");

                    if (Vector3.Distance(transform.position, player.transform.position) > attackRange)
                    {
                        CurrentMonsterState = MonsterState.Moving;
                    }
                }
                else
                {
                    CurrentMonsterState = MonsterState.Idle;
                }
                break;
            case MonsterState.Dead:
                ResetAllTriggers();
                monsterAnimator.SetTrigger("Dead");
                break;
        }
    }

    private bool CanSeeTarget()
    {
        if (PlayerController.CurrentPlayerState != PlayerState.Dead || CurrentMonsterState != MonsterState.Dead)
            return true;
        else
            return false;
    }

    public void GiveDamage()    //애니메이션 이벤트로 사용중!!!
    {
        if (PlayerController.CurrentPlayerState != PlayerState.Dead)
        {
            target.TakeDamage(monster.Attack);
        }
        else
        {
            SetCurrentMonsterState(MonsterState.Idle);
        }
        
    }

    public void MonsterSetACtivateFalse()   //애니메이션 이벤트로 사용중!!!
    {
        this.gameObject.SetActive(false);   
    }

    public void SetCurrentMonsterState(MonsterState state)
    {
        CurrentMonsterState = state;
    }

    private void ResetAllTriggers()
    {
        foreach (var param in monsterAnimator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                monsterAnimator.ResetTrigger(param.name);
            }
        }
    }
}
