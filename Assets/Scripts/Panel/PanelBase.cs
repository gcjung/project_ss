using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PanelBase : MonoBehaviour
{
    public void Awake()
    {
        InitPanel();
    }

    public abstract void InitPanel();
    public abstract void OpenPanel();
    public abstract void ClosePanel();
}
