using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponeListPanel : MonoBehaviour
{
    private Weapone[] weapones;

    public void GetWeaponeData()
    {
        weapones = new Weapone[transform.childCount];
        
        for(int i = 0; i < weapones.Length; ++i)
        {
            weapones[i] = transform.GetChild(i).GetComponent<Weapone>();
            weapones[i].LoadWeaponeData(i);
        }
    }
}
