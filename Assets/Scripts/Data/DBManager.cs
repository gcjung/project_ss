using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static GameDataManager;
using static SOGamdData;

[FirestoreData]
public class UserDatas
{
    //[FirestoreProperty] public double Gold { get; set; }
    //[FirestoreProperty] public double Gem { get; set; }
}

public enum UserDoubleDataType
{
    Gold,
    Gem,
    AttackLevel,
    AttackSpeedLevel,
    CriticalLevel,
    HpLevel,
    CurrentStageId,
    CurrentHeroId,
}
public enum UserStringDataType
{
    EquippedSkill,
    SkillData,
}

public enum GameDataType
{
    AssetBundleVersion,
}

public class DBManager : Manager<DBManager>
{
    Dictionary<string, double> userDoubleDataDic = new Dictionary<string, double>();
    Dictionary<string, string> userStringDataDic = new Dictionary<string, string>();

    Dictionary<string, object> gameData = new Dictionary<string, object>();

    DocumentReference uidRef;

    public override void Init()
    {
        if (Ininialized)
        {
            Debug.Log("DBManager already Initialized");
            return;
        }
        LoadGameData();

        if (FirebaseAuthManager.Instance.isCurrentLogin())
        {
            InitUserDBSetting();
        }
        else
        {
            Ininialized = true;
        }
    }
    public void InitUserDBSetting()
    {
        CollectionReference firebaseUserDB = FirebaseFirestore.DefaultInstance.Collection("UserDB");
        uidRef = firebaseUserDB.Document(FirebaseAuthManager.Instance.UserId);
        LoadUserDBFromFirebase();
    }
    public override void InitializedFininsh()
    {
        SetDB_SkillAcquireData();
    }
    async void LoadGameData()
    {
        DocumentReference dataRef = FirebaseFirestore.DefaultInstance.Collection("GameData").Document("Data");
        await dataRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                gameData = snapshot.ToDictionary();
            }
            else
            {

            }
        });
    }

    async void LoadUserDBFromFirebase()
    {
        DocumentSnapshot snapshot = await uidRef.GetSnapshotAsync();
        if (snapshot.Exists)
        {
            Debug.Log(String.Format("Document data for {0} document:", snapshot.Id));

            Dictionary<string, object> data = snapshot.ToDictionary();
            foreach (KeyValuePair<string, object> pair in data)
            {
                if (pair.Value is Double)
                {
                    userDoubleDataDic.Add(pair.Key, Convert.ToDouble(pair.Value));
                }
                else if (pair.Value is Int64)   // firestore���� ���������� ��
                {
                    userDoubleDataDic.Add(pair.Key, Convert.ToDouble(pair.Value));
                }
                else if (pair.Value is string)
                {
                    userStringDataDic.Add(pair.Key, (string)pair.Value);
                }
                else
                {
                    Debug.Log($"else : {pair.Key}, {pair.Value}");
                }
            }
        }
        else            // ���� ���ӽ�
        {
            await uidRef.SetAsync(new UserDatas()).ContinueWithOnMainThread(t =>
            {
                Debug.Log($"ó�� ���� ��, UserDB �ʱ�ȭ");
            });
        }

        Ininialized = true;
    }

    public double GetUserDoubleData(UserDoubleDataType key_, double defaultValue = 0)
    {
        string key = key_.ToString();
        if (userDoubleDataDic.ContainsKey(key.ToString()))
            return userDoubleDataDic[key.ToString()];
        else
            UpdateUserData(key, defaultValue);

        return defaultValue;
    }
    public double GetUserDoubleData(string key, double defaultValue = 0)
    {
        if (userDoubleDataDic.ContainsKey(key))
            return userDoubleDataDic[key];
        else
            UpdateUserData(key, defaultValue);

        return defaultValue;
    }
    public string GetUserStringData(UserStringDataType key_, string defaultValue = "")
    {
        string key = key_.ToString();
        if (userStringDataDic.ContainsKey(key))
            return userStringDataDic[key];
        else
            UpdateUserData(key, defaultValue);

        return defaultValue;
    }
    public string GetUserStringData(string key, string defaultValue = "")
    {
        if (userStringDataDic.ContainsKey(key))
            return userStringDataDic[key];
        else
            UpdateUserData(key, defaultValue);

        return defaultValue;
    }


    public void UpdateUserData(UserDoubleDataType key_, double value)
    {
        string key = key_.ToString();
        if (userDoubleDataDic.ContainsKey(key))
            userDoubleDataDic[key] = value;
        else
            userDoubleDataDic.Add(key, value);

        UpdateFirebaseUserData(key, value);
    }
    public void UpdateUserData(string key, double value)
    {
        if (userDoubleDataDic.ContainsKey(key))
            userDoubleDataDic[key] = value;
        else
            userDoubleDataDic.Add(key, value);

        UpdateFirebaseUserData(key, value);
    }
    public void UpdateUserData(UserStringDataType key_, string value)
    {
        string key = key_.ToString();
        if (userStringDataDic.ContainsKey(key))
            userStringDataDic[key] = value;
        else
            userStringDataDic.Add(key, value);

        UpdateFirebaseUserData(key, value);
    }
    public void UpdateUserData(string key, string value)
    {
        if (userStringDataDic.ContainsKey(key))
            userStringDataDic[key] = value;
        else
            userStringDataDic.Add(key, value);

        UpdateFirebaseUserData(key, value);
    }

    public string GetGameData(GameDataType key_)
    {
        string key = key_.ToString();
        if (gameData.ContainsKey(key))
            return gameData[key.ToString()].ToString();
        
        return string.Empty;
    }
    public T GetGameData<T>(GameDataType key_)
    {
        string key = key_.ToString();

        if (gameData.ContainsKey(key))
        {
            return (T)Convert.ChangeType(gameData[key], typeof(T));
        }

        return default;
    }

    void SetDB_SkillAcquireData() 
    {
        var skillAcquireData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.SkillData).Split('@');

        if (skillAcquireData.Length == 1)           // skillAcquireData ���� ���
        {
            string[] skillData = new string[SkillTemplate.Count];
            for (int i = 0; i < skillData.Length; i++)
            {
                int skillLevel = 1;
                int holdingCount = 0;
                skillData[i] = $"{skillLevel},{holdingCount}";
            }

            GlobalManager.Instance.DBManager.UpdateUserData(UserStringDataType.SkillData, string.Join('@', skillData));
        }
        else if (skillAcquireData.Length < SkillTemplate.Count)   // ���ø��� ���ο� ���̺� �߰����� ���
        {
            string[] skillData = new string[SkillTemplate.Count];
            for (int i = 0; i < skillData.Length; i++)
            {
                int skillLevel = 1;
                int holdingCount = 0;
                skillData[i] = $"{skillLevel},{holdingCount}";
            }

            skillAcquireData.CopyTo(skillData, 0);
            GlobalManager.Instance.DBManager.UpdateUserData(UserStringDataType.SkillData, string.Join('@', skillData));
        }
    }

    #region ���׸� ����
    //public T GetUserData<T>(string key)
    //{
    //    if (typeof(T) == typeof(double))
    //    {
    //        if (userDoubleDataDic.ContainsKey(key))
    //        {
    //            return (T)Convert.ChangeType(userDoubleDataDic[key], typeof(T));
    //        }
    //    }
    //    else
    //    {
    //        if (usesrStringDataDic.ContainsKey(key))
    //        {
    //            return (T)Convert.ChangeType(userDoubleDataDic[key], typeof(T));
    //        }
    //    }
    //    Debug.Log($"{key} default ��ȯ");
    //    return default(T);
    //}
    //public void UpdateUserData<T>(string key, T value)
    //{

    //    if (typeof(T) == typeof(double))
    //    {
    //        if (userDoubleDataDic.ContainsKey(key))
    //            userDoubleDataDic[key] = (double)Convert.ChangeType(value, typeof(double));
    //        else
    //            return;
    //            //intDataDic.Add(key, (int)Convert.ChangeType(value, typeof(int)));
    //    }
    //    else
    //    {
    //        if (usesrStringDataDic.ContainsKey(key))
    //            usesrStringDataDic[key] = (string)Convert.ChangeType(value, typeof(string));
    //        else
    //            return;
    //            //stringDataDic.Add(key, (string)Convert.ChangeType(value, typeof(string)));
    //    }

    //    UpdateFirebaseUserData(key, value);
    //}
    #endregion ���׸�����

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
        await uidRef.UpdateAsync(key, value).ContinueWith(task =>
        {
            //Debug.Log($"{key} : {value} Update");
        });
    }





}

#region �Ƹ� �Ⱦ�
/*
    public async void UpdateFirebaseUserData<T>(string key, T value)
    {
        Dictionary<string, object> updates = new Dictionary<string, object>
        { 
            { key, value }
        };
        await uidRef.UpdateAsync(updates).ContinueWith(task =>
        {
            //Debug.Log($"{key} : {value} Update");
        });
    }
 
 */

/// DB�о�ö� object������ ���������� �پ��� �õ�..?
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
#endregion �Ƹ� �Ⱦ�