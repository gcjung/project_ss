using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.TextCore.Text;
using TMPro;


public class Util
{
    
    public static T FindChildComponent<T>(Transform parent) where T : Component
    {
        T foundComponent = null;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            T component = child.GetComponent<T>();
            if (component != null)
            {
                foundComponent = component;
                break;
            }

            foundComponent = FindChildComponent<T>(child);
            if (foundComponent != null)
                break;
        }

        return foundComponent;
    }
    public static void SetFontInChildrenText(Transform parent, TMP_FontAsset font)
    {
        var textArr = parent.GetComponentsInChildren<TMP_Text>();

        foreach (var text in textArr)
        {
            text.font = font;
        }
    }
    //public static T[] FindChildComponents<T>(Transform parent) where T : Component
    //{
    //    T[] foundComponent = null;

    //    for (int i = 0; i < parent.childCount; i++)
    //    {
    //        Transform child = parent.GetChild(i);

    //        T component = child.GetComponent<T>();
    //        if (component != null)
    //        {
    //            foundComponent = component;
    //            break;
    //        }

    //        foundComponent = FindChildComponents<T>(child);
    //        if (foundComponent != null)
    //            break;
    //    }

    //    return foundComponent;
    //}
    //public static GameObject ShowMessagePopup(string Title, string Desc, Action ButtonAction = null)
    //{
    //    GameObject obj = null;

    //    //if (TitleManager.Instance)
    //    //    obj = GetPrefab("PopUp/MessagePopUp", TitleManager.Instance.transform.Find("Canvas"));
    //    //else if (UIManager.instance)
    //        obj = CommonFuntion.GetPrefab("PopUp/MessagePopUp", UIManager.instance.transform.Find("PopupUI_0"));

    //    obj.transform.Find("AchieveTitle").GetComponent<TextMeshProUGUI>().text = Title;
    //    obj.transform.Find("Desc").GetComponent<TextMeshProUGUI>().text = Desc;


    //    //var btns = obj.transform.GetComponentsInChildren<CloseButton>();

    //    //foreach (var item in btns)
    //    //{
    //    //    item.closeAction += ButtonAction;
    //    //}

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

    static readonly string[] Units = new string[] { "", "A", "B", "C", "D", "E", "F", "G",
        "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y",
        "Z", "AA", "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI", "AJ", "AK", "AL", "AM", "AN",
        "AO", "AP", "AQ", "AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY", "AZ", "BA", "BB", "BC",
        "BD", "BE", "BF", "BG", "BH", "BI", "BJ", "BK", "BL", "BM", "BN", "BO", "BP", "BQ", "BR",
        "BS", "BT", "BU", "BV", "BW", "BX", "BY", "BZ", "CA", "CB", "CC", "CD", "CE", "CF", "CG",
        "CH", "CI", "CJ", "CK", "CL", "CM", "CN", "CO", "CP", "CQ", "CR", "CS", "CT", "CU", "CV", "CW", "CX" };

    public static string BigNumCalculate(double number)
    {
        string numString = string.Empty;
        string unitString = string.Empty;

        string[] splitNum = number.ToString("E").Split('+');

        int.TryParse(splitNum[1], out int result);

        int quotient = result / 3;
        int remain = result % 3;

        if (result < 3)
        {
            numString = System.Math.Truncate(number).ToString();
        }
        else
        {
            var temp = double.Parse(splitNum[0].Replace("E", "")) * System.Math.Pow(10, remain);

            numString = temp.ToString("F").Replace(".00", "");
        }

        unitString = Units[quotient];

        return string.Format("{0}{1}", numString, unitString);
    }

}
