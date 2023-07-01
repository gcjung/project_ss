using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class GameDataManager
{
    [FirestoreData]
    public class StatusData
    {
        //[FirestoreProperty] public double Gold { get; set; }
        //[FirestoreProperty] public double Gem { get; set; }

    }

    public enum MonsterTemplate_
    {
        None,


        Max,
    }
    public enum StatusTemplate_
    {
        Order,
        TypeName,
        Value_Calc,
        Cost_Calc,

        Max,
    }

    public static Dictionary<string, string[]> MonsterTemplate = new Dictionary<string, string[]>();
    public static Dictionary<string, string[]> StatusTemplate = new Dictionary<string, string[]>();


    public static string[] parseData(string data)
    {
        string[] result = data.Split(',');
        if(result.Length == 1)
        {

        }
        else
        {

        }

        for (int i = 0; i < result.Length; i++)
        {
            Debug.Log($"{i} : {result[i]}");
        }
        
        return result;
    }

}