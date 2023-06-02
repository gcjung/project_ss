using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IManager
{ 
    GlobalManager GlobalManager { get; }
    bool Ininialized { get; }
    void SetGlobalManager(Transform transform);
}

public abstract class Manager<T> : MonoBehaviour, IManager where T : Component, IManager
{
    public GlobalManager GlobalManager { get; protected set; } = null;
    public bool Ininialized { get; protected set; } = false;

    public static T CreateManager(Transform transform)
    {
        T findT = default(T);

        T[] finds = FindObjectsOfType(typeof(T)) as T[];

        if (finds.Length > 0)
        {
            if (finds.Length == 1)
            {
                findT = finds[0];
                findT.gameObject.SetParent(transform);
            }
        }
        else
        {
            string name = typeof(T).ToString();
            GameObject gameObject = new GameObject(name);
            gameObject.SetParent(transform);
            findT = gameObject.AddComponent<T>();
        }
        findT.SetGlobalManager(transform);

        return findT;
    }

    public void SetGlobalManager(Transform transform)
    {
        GlobalManager = transform.GetComponent<GlobalManager>();
        Init();
    }

    public abstract void Init();
    public abstract void InitializedFininsh();
}

