using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData
{
    public ItemBase GetItemData()
    {
        ItemBase item = null;
        //���� ���� X
        return item;
    }
}


public class ItemBase : MonoBehaviour
{
    //����, ������ ����ؾ� �ϳ�?
    //���� db���� �޾ƿ� ������
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
