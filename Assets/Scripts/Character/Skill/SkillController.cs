using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static GameDataManager;

public class SkillController : MonoBehaviour
{
    const int maxEquippedSkill = 6;

    public class EquippedSkillInfo
    {
        public string id { get; private set; }
        public string name { get; private set; }
        public int cooltime { get; private set; }
        public string[] icon { get; private set; }
        public bool isSkillCooltime { get; set; } = false;

        public Image lobbySkillSlot { get; set; }
        public IEnumerator Co_SkillCooltime;
        public EquippedSkillInfo(string skillId) => SetSkillInfo(skillId);
        public void SetSkillInfo(string skillId = "")   
        {
            if (string.IsNullOrEmpty(skillId))  // 장착된 스킬이 없는 경우
            {
                id = default;
                name = default;
                icon = default;
                cooltime = default;
            }
            else
            {
                id = skillId;
                name = SkillTemplate[skillId][(int)SkillTemplate_.Name];
                icon = SkillTemplate[skillId][(int)SkillTemplate_.Icon].Split('/');
                cooltime = int.Parse(SkillTemplate[skillId][(int)SkillTemplate_.Cooltime]);
            }
        }
    }
    public EquippedSkillInfo[] equippedSkillInfo = null;
    Player player = null;
    public void Init(Player player)
    {
        this.player = player;

        var equippedSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.EquippedSkill).Split('@');

        equippedSkillInfo = new EquippedSkillInfo[maxEquippedSkill];
        for (int i = 0; i < maxEquippedSkill; i++)
            equippedSkillInfo[i] = new EquippedSkillInfo(equippedSkillData[i]);
    }
    public void SetEquipSkill(int index, string skillId)
    {
        equippedSkillInfo[index].SetSkillInfo(skillId);
    }
    public void StartSkillCooltime(int index)
    {
        var skillInfo = equippedSkillInfo[index];

        if (skillInfo.Co_SkillCooltime != null)
            StopCoroutine(skillInfo.Co_SkillCooltime);

        skillInfo.isSkillCooltime = true;

        skillInfo.Co_SkillCooltime = Co_SkillCoolTime(skillInfo.cooltime, index);
        StartCoroutine(skillInfo.Co_SkillCooltime);

    }
    public void StopSkillCooltime(int index)
    {
        var skillInfo = equippedSkillInfo[index];

        if (skillInfo.Co_SkillCooltime != null)
            StopCoroutine(skillInfo.Co_SkillCooltime);

        skillInfo.lobbySkillSlot.color = Color.white;
        skillInfo.lobbySkillSlot.transform.Find("CoolTime").GetComponent<Image>().fillAmount = 0;
    }
    public IEnumerator Co_SkillCoolTime(float cool, int index)
    {
        //Image coolTimeIcon = lobbySkillSlot.transform.Find("CoolTime").GetComponent<Image>();
        //coolTimeIcon.fillAmount = 1f;
        var skillInfo = equippedSkillInfo[index];

        Image coolTimeIcon = skillInfo.lobbySkillSlot.transform.Find("CoolTime").GetComponent<Image>();
        coolTimeIcon.fillAmount = 1f;

        skillInfo.lobbySkillSlot.color = Color.gray;

        float time = cool;
        while (time > 0)
        {
            time -= Time.deltaTime;
            coolTimeIcon.fillAmount = time / cool;

            yield return CommonIEnumerator.WaitForEndOfFrame;
        }

        skillInfo.lobbySkillSlot.color = Color.white;
        skillInfo.isSkillCooltime = false;
    }

    //ObjectPool<Thunderbolt> thunderBoltPool;
    
    void Start()
    {
        //Thunderbolt thunderbolt = CommonFunction.GetPrefab("ThunderBolt").GetComponent<Thunderbolt>();
        //thunderBoltPool = new ObjectPool<Thunderbolt>(thunderbolt, 5, transform);
        //GameObject creationFireBallEffect = CommonFunction.GetPrefab("CreationFireBallEffect");
        //creationFireBallEffectPool = new ObjectPool<GameObject>(creationFireBallEffect, 5, transform);
        enemyList = new LinkedList<Monster>();

        TmpObjectPool.Instance.CreatePool("ThunderBolt", 3);
        TmpObjectPool.Instance.CreatePool("CreationFireBallEffect", 3);
        TmpObjectPool.Instance.CreatePool("FireBall", 3);
    }
    LinkedList<Monster> enemyList;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Monster>(out var monster))
        {
            enemyList.AddLast(monster);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Monster>(out var monster))
        {
            if(enemyList.Find(monster) != null)
            {
                enemyList.Remove(monster);
            }
        }
    }
    public void UseSkill(int index, string skillId)
    {
        var skillInfo = equippedSkillInfo[index];

        if (skillInfo.isSkillCooltime) return;
        
        StartSkillCooltime(index);
        int damage = int.Parse(SkillTemplate[skillId][(int)SkillTemplate_.Damage]);
        switch (skillId)
        {
            case "1":

                break;
            case "2":
                StartCoroutine(UseSkill_FireBall(damage));
                break;

            case "3":
                StartCoroutine(UseSkill_ThunderBolt(damage));
                break;

            case "4":
                break;
            case "5":
                break;

            case "6":
                break;
            default:
                Debug.Log(skillId + "는 없는 스킬 ID 입니다");
                break;
        }
    }

    Transform FindTarget()
    {
        Debug.Log("현재 타겟수 : " + enemyList.Count);
        if (enemyList.Count <= 0) return null;

        Transform target = enemyList.First.Value.transform;
        foreach(var enemy in enemyList)
        {
            if (enemy.transform.position.x < target.position.x)
                target = enemy.transform;
        }

        return target;
    }

    IEnumerator UseSkill_FireBall(int damage)
    {
        for (int i = 0; i < 2; i++)
        {
            GameObject obj =  TmpObjectPool.Instance.GetPoolObject("CreationFireBallEffect");
            Vector3 tempPostion = player.transform.position + new Vector3(Random.Range(-0.8f, 0.2f), Random.Range(-1f, 2f), 0f);
            obj.transform.position = tempPostion;
            yield return CommonIEnumerator.WaitForSecond(0.1f);

            FireBall fireBall = TmpObjectPool.Instance.GetPoolObject("FireBall").GetComponent<FireBall>();
            fireBall.transform.position = tempPostion + new Vector3(0.3f,0,0);
            fireBall.SettingInfo(player.TotalAttack * damage, player.TotalCritical, FindTarget());

            yield return CommonIEnumerator.WaitForSecond(0.3f);
        }
    }

    IEnumerator UseSkill_ThunderBolt(int damage)
    {
        for (int i = 0; i < 4; i++)
        {
            Thunderbolt obj = TmpObjectPool.Instance.GetPoolObject("ThunderBolt").GetComponent<Thunderbolt>();
            obj.transform.position = player.transform.position + new Vector3(Random.Range(1f, 4f), 3f, 0f) + Random.insideUnitSphere * 1;
            //obj.transform.localScale = new Vector3(0.5f, 1f, 1f);
            obj.SettingInfo(player.TotalAttack * damage, player.TotalCritical, /*임시*/transform);

            yield return CommonIEnumerator.WaitForSecond(0.3f);
        }
    }
}
