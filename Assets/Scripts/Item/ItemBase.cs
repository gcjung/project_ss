using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public int ItemId { get; set; }
    public ItemType ItemType { get; set; }
    public string ItemGrade { get; set; }
    public string ItemName { get; set; }
    public int ItemLevel { get; set; }
    public float Atk { get; set; }
    public float Def { get; set; }
    public string ItemIcon { get; set; }

    [SerializeField] protected Button buttonLevelUp;
    [SerializeField] protected TextMeshProUGUI textCost;
}
