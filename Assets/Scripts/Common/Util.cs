using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.TextCore.Text;
using TMPro;


public class Util
{
    public static void ChangeLayer(GameObject obj, int layer)
    {
        obj.layer = layer;

        for (int i = 0; i < obj.transform.childCount; i++)
        {
            GameObject childObj = obj.transform.GetChild(i).gameObject;
            ChangeLayer(childObj, layer);
        }
    }
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

    // 데이터 용량으로 변환 해주는 함수
    public static string ConvertBytes(long bytes)
    {
        const int scale = 1024;
        string[] orders = new string[] { "GB", "MB", "KB", "Bytes" };
        long max = (long)Math.Pow(scale, orders.Length - 1);

        foreach (string order in orders)
        {
            if (bytes > max)
                return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);

            max /= scale;
        }

        return "0 Bytes";
    }
    
    // Text에 숫자가 자연스럽게 올라가는 함수
    IEnumerator CountingNumber(float target, float current, TMP_Text text)
    {
        float duration = 0.5f; // 카운팅에 걸리는 시간 설정. 
        float offset = (target - current) / duration;

        while (current < target)
        {
            current += offset * Time.deltaTime;
            text.text = ((int)current).ToString();
            yield return null;
        }

        current = target;
        text.text = ((int)current).ToString();
    }

    #region 사용예정인 공용함수
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
    #endregion 사용예정인 공용함수

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

        if (number < 1)
        {
            numString = number.ToString("F");
            unitString = Units[0];

            return string.Format("{0}{1}", numString, unitString);
        }

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
