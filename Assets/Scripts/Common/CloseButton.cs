using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(CloseButton))]
public class CloseButton_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        // public 변수 다 보임
        //DrawDefaultInspector();
        //if (GUILayout.Button("Test Button"))
        //{
        //    Debug.Log("버튼버튼 누름");
        //}


        CloseButton x = (CloseButton)target;
        x.isDontDestroy = EditorGUILayout.Toggle("isDontDestoy", x.isDontDestroy);
        x.DestroyTarget = (GameObject)EditorGUILayout.ObjectField("DestroyTarget", x.DestroyTarget, typeof(GameObject), true);
    }
}
#endif
public class CloseButton : Button
{
    public bool isDontDestroy = false;
    public GameObject DestroyTarget;

    override protected void Start()
    {
        base.Start();

        onClick.AddListener(CloseObject);
    }

    public void CloseObject()
    {

        if (!isDontDestroy)
        {
            if (DestroyTarget != null)
            {
                Destroy(DestroyTarget);
            }
            else
            {
                if (transform.parent.gameObject)
                    Destroy(transform.parent.gameObject);
            }
        }
        else
        {
            if (DestroyTarget != null)
            {
                DestroyTarget.SetActive(false);
            }
            else
            {
                if (transform.parent.gameObject)
                    transform.parent.gameObject.SetActive(false);
            }
        }
    }

}
