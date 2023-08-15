using UnityEngine;
using static GameDataManager;

public class Weapone : ItemBase
{
    public void LoadWeaponeData(int itemId)
    {
        ItemType = ItemType.Weapon;
        ItemGrade = ItemTemplate[itemId.ToString()][(int)ItemTemplate_.ItemGrade];
        ItemName = ItemTemplate[itemId.ToString()][(int)ItemTemplate_.ItemName];
        ItemLevel = int.Parse(ItemTemplate[itemId.ToString()][(int)ItemTemplate_.ItemLevel]);
        Atk = int.Parse(ItemTemplate[itemId.ToString()][(int)ItemTemplate_.ItemAtk]);
        ItemIcon = ItemTemplate[itemId.ToString()][(int)ItemTemplate_.ItemIcon];

        //������ ������ ���� cost ���� �߰� �ʿ�
    }
}
