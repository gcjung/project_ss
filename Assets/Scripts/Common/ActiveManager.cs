using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveManager : Manager<ActiveManager>
{
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
