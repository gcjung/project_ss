using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
