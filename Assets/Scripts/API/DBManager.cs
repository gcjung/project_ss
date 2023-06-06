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
    int Count { get; set; }

    [FirestoreProperty]
    int update { get; set; }
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
            Debug.Log("title:  " + documentDictionary["gem"] as string);
            Debug.Log("title:  " + documentDictionary["gold"] as string);
        }
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
        docRef.SetAsync(user);
    }

    void AddUser()
    {
        Dictionary<string, string> test = new Dictionary<string, string>();
        test.Add("sdfasd", "12312311");
        DocumentReference docRef = db.Collection("UserDB").Document("1k3OVr2MqO3unUy8LIN7");
        docRef.SetAsync(test).ContinueWithOnMainThread(x => { Debug.Log($"{x}, ¼º°ø"); });
    }

    void TestAddData()
    {
        Counter counter =  new Counter();
    }

   
}