using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using static GameDataManager;


public class GameDataManager : Manager<GameDataManager>
{
    [FirestoreData]
    public class StatusData
    {
        //[FirestoreProperty] public double Gold { get; set; }
        //[FirestoreProperty] public double Gem { get; set; }

    }

    public enum MonsterTemplate_
    {
        None = -1,
        Name,
        Hp,
        Attack,
        Gold,

        Max,
    }
    public enum StatusTemplate_
    {
        TypeName,
        Order,
        Value_Calc,
        Cost_Calc,

        Max,
    }

    public static Dictionary<string, string[]> MonsterTemplate;
    public static Dictionary<string, string[]> StatusTemplate;

    private void Awake()
    {
        MonsterTemplate = CSVRead("data/Monster_Template");
        StatusTemplate = CSVRead("data/Status_Template");

        Ininialized = true;
    }


    const string SPLIT = ",";
    const string LINE_SPLIT = @"\r\n|\n\r|\n|\r";

    public Dictionary<string, string[]> CSVRead(string file)
    {
        var dic = new Dictionary<string, string[]>();
        TextAsset data = Resources.Load(file) as TextAsset;

        var lines = Regex.Split(data.text, LINE_SPLIT);
        if (lines.Length <= 1) return dic;

        var header = Regex.Split(lines[0], SPLIT);
        for (var i = 1; i < lines.Length; i++)
        {
            var values = Regex.Split(lines[i], SPLIT);

            if (values.Length < header.Length) continue;

            bool isValid = true;
            for (var j = 0; j < values.Length; j++)
            {
                if (string.IsNullOrEmpty(values[j]))    // csv에 데이터가 유효한지 확인
                    isValid = false;
            }

            if (isValid)
            {
                dic.Add(values[0], values);
                //string str = $"key : {values[0]}";
                //for (int j = 0; j < values.Length; j++)
                //    str += $", value : {values[j]}";

                //Debug.Log($"{str}");
            }
        }
        return dic;
    }

    public override void Init()
    {
        //throw new System.NotImplementedException();
    }

    public override void InitializedFininsh()
    {
        //throw new System.NotImplementedException();
    }
}