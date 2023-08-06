using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameDataManager;

public class SkillController : MonoBehaviour
{
    string equippedSkill;
    IEnumerator Start()
    {
        yield return CommonIEnumerator.IEWaitUntil(
            predicate: () => { return GlobalManager.Instance.Initialized; },
            onFinish: () =>{
                equippedSkill = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.EquippedSkill);
            });


        //foreach (var data in SkillTemplate)
        //{
        //    //Debug.Log($"data.Key : {data.Key}");
        //    for (int i = 0; i < data.Value.Length; i++)
        //    {
        //        Debug.Log($"data.Key : {data.Key}, value : {data.Value[i]}");
        //    }
        //}
    }

  
}
