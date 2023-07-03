using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadItem : MonoBehaviour
{
    [SerializeField] private ItemData itemdata;

    private void Awake()
    {
        if(itemdata == null)
        {
            Debug.Log($"<color=#7FD6FD>아이템데이터 없음</color>");
        }
    }
}
