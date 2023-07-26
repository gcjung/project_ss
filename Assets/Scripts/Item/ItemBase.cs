using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData
{
    public ItemBase GetItemData()
    {
        ItemBase item = null;
        //아직 구현 X
        return item;
    }
}


public class ItemBase : MonoBehaviour
{
    //무기, 방어구에만 사용해야 하나?
    //전부 db에서 받아올 데이터
    public ItemType Itemtype { get; set; }
    public ItemGrade ItemGrade { get; set; }
    public int ItemIndex { get; set; }
    public string ItemName { get; set; }
    public float Atk { get; set; }
    public float Def { get; set; }

    public ItemBase(ItemType itemType, ItemGrade itemGrade, string itenName, int itemIndex, float itemAtk, float ItemDef)
    {
        ItemGrade = itemGrade;
        Itemtype = itemType;
        ItemName = itenName;
        ItemIndex = itemIndex;
        Atk = itemAtk;
        Def = ItemDef;
    }
}
