using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Manager<SoundManager>
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

    public void PlayBgmSound()
    {
        //
    }

    public void PlayEffectSound()
    {
        //
    }
}
