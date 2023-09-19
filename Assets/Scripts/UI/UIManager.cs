using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance = null;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        TmpObjectPool.Instance.CreatePool("Notification_Text", 10);
        TmpObjectPool.Instance.CreatePool("Damage_Text", 10);
        TmpObjectPool.Instance.CreatePool("Damage_Text(Critical)", 10);
    }
}
