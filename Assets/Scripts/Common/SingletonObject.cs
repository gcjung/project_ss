using UnityEngine;

public class SingletonObject<T> : MonoBehaviour where T : Component
{
    //ΩÃ±€≈Ê ø¿∫Í¡ß∆Æ º≥¡§
    protected SingletonObject() { }
    ~SingletonObject() { }

    private static T instance = null;
    private static Object obj = new Object();
    private static bool isQuit = false;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                if(isQuit)
                {
                    return null;
                }

                T[] finds = FindObjectsOfType(typeof(T)) as T[];

                if (finds.Length > 0)
                {
                    if (finds.Length == 1)
                    {
                        instance = finds[0];
                    }
                    else
                    {
                        Debug.Log("12");
                    }
                }
                else
                {
                    lock (obj)
                    {
                        string objName = "[" + typeof(T).ToString() + "]";
                        GameObject go = new GameObject(objName, typeof(T));
                        instance = go.GetComponent<T>();
                    }
                }
            }
           
            return instance;
        }
    }

    public virtual void Awake()
    {
        Debug.Log($"ΩÃ±€≈Ê ø¿∫Í¡ß∆Æ: { gameObject.name} Ω∫≈©∏≥∆Æ : { typeof(T)}");

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(this);
        }
    }

    public virtual void OnDestroy()
    {
        isQuit = true;
        instance = null;
    }

    public virtual void OnApplicationQuit()
    {
        isQuit = true;
        instance = null;
    }
}
