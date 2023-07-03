using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Item/ItemkData")]
public class ItemData : ScriptableObject
{
    [System.Serializable]
    public class ItemDataBase
    {
        public ItemBase Item; //아이템 오브젝트
    }

    public List<ItemDataBase> listWeapones = new List<ItemDataBase>();
    public List<ItemDataBase> listarmors= new List<ItemDataBase>();
    //public List<ItemDataBase> listWeapones = new List<ItemDataBase>();
    //public List<ItemDataBase> listWeapones = new List<ItemDataBase>();
}