using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveManager : Manager<ActiveManager>
{
    public Dictionary<ItemBase, bool> ItemLockStatus = new Dictionary<ItemBase, bool>();
    public override void Init()
    {
        if (Ininialized)
        {
            return;
        }

        Ininialized = true;
    }
    public override void InitializedFininsh()
    {
        //
    }
}