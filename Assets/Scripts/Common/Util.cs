using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.TextCore.Text;
using TMPro;
using UnityEditor;
using UnityEngine.UI;

public class Util
{
    public static Color skillSlot_Equip_Color = new Color(1, 1, 1, 100f / 255f);
    public static Color skillSlot_Unequip_Color = Color.white;
    public static Color skillSlot_Lock_Color = new Color(100f / 255f, 100f / 255f, 100f / 255f, 100f / 255f);

    //colorDic.Add("skillSlot_Equip", new Color(1, 1, 1, 100f / 255f));
    //    colorDic.Add("skillSlot_Unequip", Color.white);
    //    colorDic.Add("skillSlot_Lock", new Color(100f / 255f, 100f / 255f, 100f / 255f, 100f / 255f));

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
    public static void SetFontInChildrenText(Transform parent)
    {
        var textArr = parent.GetComponentsInChildren<TMP_Text>();

        foreach (var text in textArr)
        {
            if (text.font == null)
            {
                Debug.Log(text.name + ", 폰트없음@@@@@@@@@@@@@@@");
                continue;
            }

            var font = CommonFunction.GetFont("Font/" + text.font.name);

            if (font == null)
                font = CommonFunction.GetFont("Font/KimjungchulGothic-Regular SDF");

            text.font = font;
        }
    }


    public static void SetRawImageTexture(Transform parent)
    {
        RawImage[] rawImages = parent.GetComponentsInChildren<RawImage>();
        foreach (var ri in rawImages)
        {
            if (ri.texture == null)
                continue;

            if (!string.IsNullOrEmpty(ri.texture.name))
            {
                ri.texture = Resources.Load<Texture>("Texture/" + ri.texture.name);
            }
        }
    }


    public static void ReLinkShader(GameObject obj, bool _isActive = false)
    {
        MeshRenderer[] meshRenderers = obj.GetComponentsInChildren<MeshRenderer>(_isActive);
        foreach (MeshRenderer mr in meshRenderers)
        {
            foreach (var sm in mr.sharedMaterials)
            {
                if (sm == null || sm.shader == null)
                    continue;
                
                if (!string.IsNullOrEmpty(sm.shader.name))
                {
                    sm.shader = Shader.Find(sm.shader.name);
                }
            }
        }

        ParticleSystem[] particleSystem = obj.GetComponentsInChildren<ParticleSystem>(_isActive);
        foreach (ParticleSystem ps in particleSystem)
        {
            if (ps.GetComponent<Renderer>().sharedMaterials != null)
            {
                foreach (var sm in ps.GetComponent<Renderer>().sharedMaterials)
                {
                    if (sm == null || sm.shader == null)
                        continue;
                    
                    if (!string.IsNullOrEmpty(sm.shader.name))
                    {
                        sm.shader = Shader.Find(sm.shader.name);
                    }
                }
            }
        }

        TrailRenderer[] trailRenderers = obj.GetComponentsInChildren<TrailRenderer>();
        foreach (var tr in trailRenderers)
        {
            foreach (var sm in tr.sharedMaterials)
            {
                if (sm == null || sm.shader == null)
                    continue;

                if (!string.IsNullOrEmpty(sm.shader.name))
                {
                    sm.shader = Shader.Find(sm.shader.name);
                }
            }
        }

        //TMP_Text[] TMP_Texts = obj.GetComponentsInChildren<TMP_Text>();
        //foreach (TMP_Text text in TMP_Texts)
        //{
        //    foreach (var sm in text.fontSharedMaterials)
        //    {
        //        if (sm == null || sm.shader == null)
        //            continue;

        //        if (!string.IsNullOrEmpty(sm.shader.name))
        //        {
        //            int originRenderQ = sm.renderQueue;
        //            sm.shader = Shader.Find(sm.shader.name);
        //            sm.renderQueue = originRenderQ;
        //        }
        //    }
        //}
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

    public static void InitGrid(Transform gridTr)
    {
        if (gridTr == null)
            return;

        if (gridTr.childCount > 0)
        {
            while (gridTr.childCount != 0)
            {
                Transform child = gridTr.GetChild(0);
                child.SetParent(null);
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }
    }

    public static Color ConvertGradeToColor(string grade)
    {
        switch(grade)
        {
            case "커먼":
                return Color.white;
            case "언커먼":
                return Color.green;
            case "레어":
                return Color.cyan;
            case "에픽":
                return Color.magenta;
            case "레전더리":
                return Color.red;
            default:
                return Color.black;
        }

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

        if (number < 1000)
        {
            numString = number.ToString();
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
