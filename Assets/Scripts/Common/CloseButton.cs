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
        serializedObject.Update();
       
        EditorGUILayout.PropertyField(serializedObject.FindProperty("destroyTarget"), new GUIContent("Destroy Target"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isDontDestroy"), new GUIContent("isDontDestroy"));

        serializedObject.ApplyModifiedProperties();

        // public 변수 다 보임
        //DrawDefaultInspector();



        //if (GUILayout.Button("Test Button"))
        //{
        //    Debug.Log("버튼버튼 누름");
        //}

        //CloseButton x = (CloseButton)target;
        //x.isDontDestroy = EditorGUILayout.Toggle("isDontDestoy", x.isDontDestroy);
        //x.DestroyTarget = (GameObject)EditorGUILayout.ObjectField("DestroyTarget", x.DestroyTarget, typeof(GameObject), true);

        //if (GUI.changed)
        //    EditorUtility.SetDirty(target);
    }
}
#endif
public class CloseButton : Button
{
    public bool isDontDestroy = false;
    public GameObject destroyTarget;

    override protected void Start()
    {
        onClick.AddListener(CloseObject);
    }

    public void CloseObject()
    {
        if (!isDontDestroy)
        {
            if (destroyTarget != null)
                Destroy(destroyTarget);
            else
                Destroy(transform.parent.gameObject);
        }
        else
        {
            if (destroyTarget != null)
                destroyTarget.SetActive(false);
            else
                transform.parent.gameObject.SetActive(false);
        }
    }
}
