using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using System;


[FirestoreData]
public struct Counter
{
    [FirestoreProperty]
    public int Count { get; set; }

    [FirestoreProperty]
    public int update { get; set; }
}

[FirestoreData]
public struct UserData
{
    [FirestoreProperty]
    public int Gold { get; set; }

    [FirestoreProperty]
    public int Gem { get; set; }

    [FirestoreProperty]
    public int gem { get; set; }
    public int gold { get; set; }
}
public class DBManager : MonoBehaviour
{
    CollectionReference userDB;
    private async void Start()
    {
        userDB = FirebaseFirestore.DefaultInstance.Collection("UserDB");

        QuerySnapshot snapshot = await userDB.GetSnapshotAsync();
        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            Dictionary<string, object> documentDictionary = document.ToDictionary();
            Debug.Log($"{documentDictionary.Count} : " + documentDictionary["gem"] as string);
            Debug.Log($"{documentDictionary.Count} : " + documentDictionary["gold"] as string);
        }

        GetData();
    }
    void GetData()
    {
        db.Collection("UserDB").Document("1k3OVr2MqO3unUy8LIN7").GetSnapshotAsync().ContinueWithOnMainThread(tast =>
        {
            DocumentSnapshot snapshots = tast.Result;

            //UserData userData = tast.Result.ConvertTo<UserData>();
            Dictionary<string, object>  dic = tast.Result.ToDictionary();
            Debug.Log($"userData.gem : {dic["gem"]}");
            Debug.Log($"userData.gold : {dic["gold"]}");
            //Debug.Log($"userData.Gold : {userData.Gold}");
            //Debug.Log($"userData.Gem : {userData.Gem}");
        });

    }

    FirebaseFirestore db;
    void dataadd()
    {
        DocumentReference docRef = db.Collection("users").Document("alovelace");
        Dictionary<string, object> user = new Dictionary<string, object>
            {
                { "First", "Ada" },
                { "Last", "Lovelace" },
                { "Born", 1815 }
            };
        docRef.SetAsync(user).ContinueWithOnMainThread(t => Debug.Log("데이터 올림"));
    }

    void AddUser()
    {
        Dictionary<string, string> test = new Dictionary<string, string>();
        test.Add("sdfasd", "12312311");
        DocumentReference docRef = db.Collection("UserDB").Document("1k3OVr2MqO3unUy8LIN7");
        docRef.SetAsync(test).ContinueWithOnMainThread(x => { Debug.Log($"{x}, 성공"); });
    }

    void TestAddData()
    {
        Counter counter = new Counter()
        {
            Count = 0
        };

        
    }

   
}