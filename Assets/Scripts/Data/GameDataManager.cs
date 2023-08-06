using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;


public class GameDataManager : Manager<GameDataManager>
{
    public enum HeroTemplate_
    {
        None = -1,
        HeroId,
        Name,
        Attack,
        AttackSpeed,
        Critical,
        Hp,

        Max,
    }
    public enum MonsterTemplate_
    {
        None = -1,
        MonsterId,
        Name,
        Hp,
        Attack,
        Gold,

        Max,
    }
    public enum StatusTemplate_
    {
        None = -1,
        Type,
        TypeName,
        Order,
        Value_Calc,
        Cost_Calc,

        Max,
    }
    public enum StageTemplate_
    {
        None = -1,
        StageId,
        Stage,
        Monster,
        MapImage,
        Boss,

        Max,
    }
    public enum SkillTemplate_
    {
        None = -1,
        Index,
        Grade,
        Name,
        Rrder,
        Cooltime,
        Damage,
        SpriteName,
        Desc,

        Max,
    }

    public static Dictionary<string, string[]> HeroTemplate;
    public static Dictionary<string, string[]> MonsterTemplate;
    public static Dictionary<string, string[]> StatusTemplate;
    public static Dictionary<string, string[]> StageTemplate;
    public static Dictionary<string, string[]> SkillTemplate;

    public override void Init()
    {       
        if (Ininialized)
        {
            Debug.Log("GameDataManmger already Initialized");
            return;
        }
        HeroTemplate = CSVRead("data/Hero_Template");
        MonsterTemplate = CSVRead("data/Monster_Template");
        StatusTemplate = CSVRead("data/Status_Template");
        StageTemplate = CSVRead("data/Stage_Template");
        SkillTemplate = CSVRead("data/Skill_Template");

        Ininialized = true;
    }

    public override void InitializedFininsh()
    {
        //throw new System.NotImplementedException();
    }

    const string COMMA_SPLIT = @"\s*,\s*";
    const string LINE_SPLIT = @"\r\n|\n\r|\n|\r";
    public Dictionary<string, string[]> CSVRead(string file)
    {
        var dic = new Dictionary<string, string[]>();
        TextAsset data = Resources.Load(file) as TextAsset;

        var lines = Regex.Split(data.text, LINE_SPLIT);
        if (lines.Length <= 1) return dic;

        var header = Regex.Split(lines[0], COMMA_SPLIT);
        for (var i = 1; i < lines.Length; i++)
        {
            //var values = Regex.Split(Regex.Replace(lines[i], @"\s+", string.Empty), COMMA_SPLIT); // csv 데이터 공백없애기
            var values = Regex.Split(lines[i], COMMA_SPLIT); 
            
            if (values.Length < header.Length) continue;

            bool isValid = true;
            for (int j = 0; j < values.Length; j++)
            {
                if (string.IsNullOrEmpty(values[j]))    // csv에 데이터가 유효한지 확인
                    isValid = false;
            }

            if (isValid)
            {
                dic.Add(values[0], values);
            }
        }
        return dic;
    }


}