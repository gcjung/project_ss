using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    const int maxEquippedSkill = 6;
    SkillInfo[] equippedSkillInfo = null;
    class SkillInfo
    {
        public string id { get; private set; }
        public string name;
    }
    // Start is called before the first frame update
    void Start()
    {
        equippedSkillInfo = new SkillInfo[maxEquippedSkill];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void test(string skillId)
    {
        Debug.Log(skillId + ", 스킬나감");
        //string ts = equippedSkillInfo[1].id;
    }
}
