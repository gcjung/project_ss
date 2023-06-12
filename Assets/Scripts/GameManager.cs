using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : SingletonObject<GameManager>
{
    [SerializeField]
    private GameObject mainUiPanel;

    private TMP_Text goldText;
    void Start()
    {
        goldText = mainUiPanel.transform.Find("UpSide_Panel/Goods_Panel/Gold_Image/Gold_Text").GetComponent<TMP_Text>();

        //StartCoroutine(teeest());
        //InvokeRepeating(nameof(GetGold), 3f,3f);
    }

    
    IEnumerator teeest()
    {
        while (true)
        {
            GetGold();
            yield return new WaitForSecondsRealtime(3);
        }
    }

    void GetGold()
    {
        Debug.Log($"{DBManager.Instance.userObject.Count}");
        int gold = 111 + (int)Convert.ChangeType(DBManager.Instance.userObject["Gem"], typeof(int));
        

        DBManager.Instance.UpdateUserData<int>("Gem", gold);
    }
}
