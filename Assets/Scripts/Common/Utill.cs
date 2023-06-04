using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;
using UnityEngine.UI;
public static class Utill
{ 
    public static void SetParent(this GameObject gameObject, Transform transform)
    {
        if(transform == null)
        {
            Debug.LogError($" {nameof(SetParent)} null");
            return;
        }
        gameObject.transform.parent = transform;
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localScale = Vector3.one;
        gameObject.transform.localRotation = Quaternion.identity;
    }

    //public static GameObject ShowMessagePopup(string Title, string Desc, Action ButtonAction = null)
    //{
    //    GameObject obj = null;

    //    if (TitleManager.Instance)
    //        obj = GetPrefab("PopUp/MessagePopUp", TitleManager.Instance.transform.Find("Canvas"));
    //    else if (UIManager.instance)
    //        obj = GetPrefab("PopUp/MessagePopUp", UIManager.instance.transform.Find("PopupUI_5"));

    //    obj.transform.Find("AchieveTitle").GetComponent<TextMeshProUGUI>().text = Title;
    //    obj.transform.Find("Desc").GetComponent<TextMeshProUGUI>().text = Desc;

    //    var btns = obj.transform.GetComponentsInChildren<CloseButton>();

    //    foreach (var item in btns)
    //    {
    //        item.closeAction += ButtonAction;
    //    }

    //    return obj;
    //}

    //public static GameObject CreateYesOrNoPopUp(string title, string desc, Transform parent, Action yesEvent, Action noEvent, bool isDestroy = true, bool isCheckBox = false)
    //{
    //    GameObject popUpObj = GetPrefab("PopUp/YesOrNoPopUp", parent);

    //    popUpObj.transform.Find("tabname").GetComponent<Text>().text = title;
    //    popUpObj.transform.Find("Desc").GetComponent<Text>().text = desc;

    //    if (isDestroy)
    //    {
    //        yesEvent += () => { ClosePopup(popUpObj); SoundManager.Instance.Play(SoundType.SE_ButtonClick); ; };
    //        noEvent += () => { ClosePopup(popUpObj); SoundManager.Instance.Play(SoundType.SE_ButtonClick); };
    //    }

    //    Button yesBtn = popUpObj.transform.Find("YesButton").GetComponentInChildren<Button>();
    //    yesBtn.onClick.RemoveAllListeners();
    //    if (yesEvent != null)
    //        yesBtn.onClick.AddListener(yesEvent.Invoke);

    //    Button noBtn = popUpObj.transform.Find("NoButton").GetComponentInChildren<Button>();
    //    noBtn.onClick.RemoveAllListeners();
    //    if (noEvent != null)
    //        noBtn.onClick.AddListener(noEvent.Invoke);

    //    if (isCheckBox)
    //    {
    //        popUpObj.transform.Find("Check").gameObject.SetActive(true);
    //        popUpObj.transform.Find("Check/Toggle").GetComponent<Toggle>().isOn = false;
    //    }

    //    return popUpObj;
    //}
}
