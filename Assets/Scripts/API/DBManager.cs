using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[FirestoreData]
public class UserDatas
{
    //[FirestoreProperty] public double Gold { get; set; }
    //[FirestoreProperty] public double Gem { get; set; }

}


public class DBManager : Manager<DBManager>
{
    Dictionary<string, double> doubleDataDic = new Dictionary<string, double>();
    Dictionary<string, string> stringDataDic = new Dictionary<string, string>();
    
    CollectionReference firebaseUserDB;
    DocumentReference uidRef;
    UserDatas dataTable;
    

    public void Awake()
    {
        firebaseUserDB = FirebaseFirestore.DefaultInstance.Collection("UserDB");
        uidRef = firebaseUserDB.Document(FirebaseAuthManager.Instance.UserId);
        dataTable = new UserDatas();

        LoadFirebaseData();
    }

    async void LoadFirebaseData()
    {
        DocumentSnapshot snapshot = await uidRef.GetSnapshotAsync();
        if (snapshot.Exists)
        {
            Debug.Log(String.Format("Document data for {0} document:", snapshot.Id));

            Dictionary<string, object> data = snapshot.ToDictionary();
            //foreach (var dic in data)
            //{
            //    Debug.Log($"{dic.Key} : {dic.Value}");
            //}

            foreach (KeyValuePair<string, object> pair in data)
            {
                
                if (pair.Value is Double)
                {
                    doubleDataDic.Add(pair.Key, Convert.ToDouble(pair.Value));
                    Debug.Log($"Double : {pair.Key}, {pair.Value}");

                }
                else if (pair.Value is string)
                {
                    stringDataDic.Add(pair.Key, (string)pair.Value);
                    //Debug.Log($"string : {pair.Key}, {pair.Value}");
                }
                else
                {
                    //Debug.Log($"else : {pair.Key}, {pair.Value}");
                }
            }
        }
        else            // 최초 접속시
        {
            await uidRef.SetAsync(dataTable).ContinueWithOnMainThread(t =>
            {   
                Debug.Log($"처음 접속 시, UserDB 초기화");
            });
        }

        Ininialized = true;
    }

    public double GetUserDoubleData(string key)
    {
        if (doubleDataDic.ContainsKey(key))
            return doubleDataDic[key];

        return default;
    }
    public string GetUserStringData(string key)
    {
        if (stringDataDic.ContainsKey(key))
            return stringDataDic[key];

        return default;
    }
    public void UpdateUserData(string key, double value)
    {
        if (doubleDataDic.ContainsKey(key))
            doubleDataDic[key] = value;
        else
            doubleDataDic.Add(key, value);

        UpdateFirebaseUserData(key, value);
    }
    public void UpdateUserData(string key, string value)
    {
        if (stringDataDic.ContainsKey(key))
            stringDataDic[key] = value;
        else
            stringDataDic.Add(key, value);

        UpdateFirebaseUserData(key, value);
    }




    #region 제네릭 버전
    public T GetUserData<T>(string key)
    {
        if (typeof(T) == typeof(double))
        {
            if (doubleDataDic.ContainsKey(key))
            {
                return (T)Convert.ChangeType(doubleDataDic[key], typeof(T));
            }
        }
        else
        {
            if (stringDataDic.ContainsKey(key))
            {
                return (T)Convert.ChangeType(doubleDataDic[key], typeof(T));
            }
        }
        Debug.Log($"{key} default 반환");
        return default(T);
    }
    public void UpdateUserData<T>(string key, T value)
    {
        
        if (typeof(T) == typeof(double))
        {
            if (doubleDataDic.ContainsKey(key))
                doubleDataDic[key] = (double)Convert.ChangeType(value, typeof(double));
            else
                return;
                //intDataDic.Add(key, (int)Convert.ChangeType(value, typeof(int)));
        }
        else
        {
            if (stringDataDic.ContainsKey(key))
                stringDataDic[key] = (string)Convert.ChangeType(value, typeof(string));
            else
                return;
                //stringDataDic.Add(key, (string)Convert.ChangeType(value, typeof(string)));
        }

        UpdateFirebaseUserData(key, value);
    }
    #endregion 제네릭버전

    public async Task<T> GetFirebaseUserData<T>(string key)
    {
        DocumentSnapshot snapshot = await uidRef.GetSnapshotAsync();

        Dictionary<string, object> userData = snapshot.ToDictionary();
        if (userData.ContainsKey(key))
            return (T)Convert.ChangeType(userData[key], typeof(T));
        
       return default(T);
    }
    public async void UpdateFirebaseUserData<T>(string key, T value)
    {
        Dictionary<string, object> updates = new Dictionary<string, object>
        { 
            { key, value }
        };

        await uidRef.UpdateAsync(updates).ContinueWithOnMainThread(task =>
        {
            Debug.Log($"{key} : {value} Update");
        });
    }
    public override void InitializedFininsh()
    {
        //
    }
    public override void Init()
    {

    }

}


/// DB읽어올때 object형으로 안읽으려고 다양한 시도..?
/*
            Dictionary<string, object> data = snapshot.ToDictionary();
            
            Dictionary<string, object> mapData = data["string"] as Dictionary<string, object>;
            Debug.Log($"{mapData["Nickname"]} + {mapData["job"]}");




            //Debug.Log("@@" + data["Test"] + "##" + data["string"]);
            Dictionary<string, string> stringData = data["string"] as Dictionary<string, string>;
            Dictionary<string, int> intData = data["int"] as Dictionary<string, int>;
            Debug.Log(stringData);
            Debug.Log(stringData["Nickname"] + stringData["job"]);
            //foreach (KeyValuePair<string, string> value in stringData) 
            //{
            //    Debug.Log($"{value.Key} : {value.Value}");
            //}
            //foreach (var value in intData)
            //{
            //    Debug.Log($"{value.Key} : {value.Value}");
            //}
 */