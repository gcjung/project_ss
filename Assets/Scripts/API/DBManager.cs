using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Threading.Tasks;
using static UnityEditor.Progress;



[FirestoreData]
public class UserDatas
{
    [FirestoreProperty] public int Gold { get; set; }
    [FirestoreProperty] public int Gem { get; set; }

}


public class DBManager : SingletonObject<DBManager>
{
    public Dictionary<string, object> userObject = new Dictionary<string, object>();

    CollectionReference firebaseUserDB;
    DocumentReference uidRef;
    UserDatas userData;
    public UserDatas UserData => userData;

    public override void Awake()
    {
        base.Awake();
        firebaseUserDB = FirebaseFirestore.DefaultInstance.Collection("UserDB");
        uidRef = firebaseUserDB.Document(FirebaseAuthManager.Instance.UserId);
        
        userData = new UserDatas();
    }
    private async void Start()
    {
        DocumentSnapshot snapshot = await uidRef.GetSnapshotAsync();
        if (snapshot.Exists)
        {
            Debug.Log(String.Format("Document data for {0} document:", snapshot.Id));
            userObject = snapshot.ToDictionary();

            //userData = snapshot.ConvertTo<UserDatas>();
        }
        else
        {
            await uidRef.SetAsync(userData).ContinueWithOnMainThread(t => {
                Debug.Log($"처음 접속 시, UserDB 초기화");
            });
        }

        //uidRef.Listen(snapshot => {
        //    Debug.Log("Callback received document snapshot.");
        //    Debug.Log(String.Format("Document data for {0} document:", snapshot.Id));
        //    Dictionary<string, object> city = snapshot.ToDictionary();

        //    foreach (KeyValuePair<string, object> pair in city)
        //    {
        //        Debug.Log(String.Format("{0}: {1}", pair.Key, pair.Value));
        //    }
        //});


    }
    public async Task<T> GetUserData<T>(string key)
    {
        DocumentSnapshot snapshot = await uidRef.GetSnapshotAsync();

        Dictionary<string, object> userData = snapshot.ToDictionary();
        if (userData.ContainsKey(key))
            return (T)Convert.ChangeType(userData[key], typeof(T));
        
        return default(T);
    }
    public async void UpdateUserData<T>(string key, T value)
    {
        userObject[key] = value;

        Dictionary<string, object> updates = new Dictionary<string, object>
        { 
            { key, value }
        };
        await uidRef.UpdateAsync(updates).ContinueWithOnMainThread(task =>
        {
            Debug.Log($"{key} : {value} Update");
        });
    }

  
}