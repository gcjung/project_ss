using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Extensions;
using System;

public class User
{
    public string firstName;
    public string lastName;

    public User(string firstName, string lastName)
    {
        this.firstName = firstName;
        this.lastName = lastName;
    }
}
[Obsolete("������ �Ⱦ�",true)]
public class DatabaseManager : MonoBehaviour
{
    //DatabaseReference databaseReference;
    //FirebaseDatabase database;
    //void Start()
    //{
    //    //FirebaseApp.DefaultInstance.Options.DatabaseUrl = new System.Uri("https://gcio-d138b-default-rtdb.firebaseio.com/");
    //    database = FirebaseDatabase.GetInstance("https://gcio-d138b-default-rtdb.firebaseio.com/");
    //    databaseReference = database.RootReference;
    //    WriteDB();
    //    ReadDB();
    //}

    //void writeNewUser() // ������ ȸ�� ���� ��ȣ�� ���� ����� �⺻�� ����
    //{
    //    User user = new User("��", "��");
    //    string json = JsonUtility.ToJson(user);
    //    databaseReference.Child("users").Child("user1").SetRawJsonValueAsync(json).ContinueWith(task =>
    //    {
    //        if (task.IsFaulted)
    //        {
    //            // Handle the error.
    //            Debug.LogError("Failed to write data to Firebase database: " + task.Exception);
    //        }
    //        else if (task.IsCompleted)
    //        {
    //            Debug.Log("Data written successfully.");
    //        }
    //    });
    //}


    //public void WriteDB()
    //{
    //    //GPSdata DATE1 = new GPSdata("seoul", 37.0f, 23.4f, 123f);
    //    //GPSdata DATE2 = new GPSdata("Busan", 137.0f, 1223.4f, 13.5f);

    //    //string jsondate1 = JsonUtility.ToJson(DATE1);
    //    //string jsondate2 = JsonUtility.ToJson(DATE2);

    //    //databaseReference.Child("Korea").Child("area1").SetRawJsonValueAsync(jsondate1);
    //    //databaseReference.Child("Korea").Child("area2").SetRawJsonValueAsync(jsondate2);
    //    databaseReference.Child("Korea").Child("area2").Child("name").SetValueAsync("�׽�Ʈ��");
    //}
    //public void ReadDB()
    //{
    //    DatabaseReference databaseReference = database.GetReference("Korea");
    //    databaseReference.GetValueAsync().ContinueWith(task =>
    //    {
    //        if (task.IsCompleted)
    //        {
    //            DataSnapshot snapshot = task.Result;
    //            foreach (DataSnapshot data in snapshot.Children)
    //            {
    //                IDictionary GPSdata = (IDictionary)data.Value;
    //                Debug.Log($"�̸� : {GPSdata["name"]}, ���� : {GPSdata["latitude_data"]}, �浵 : {GPSdata["longitude_data"]}, �� : {GPSdata["altitude_data"]}");
    //            }
    //        }
    //    });
    //}
}

public class GPSdata
{
    public string name = "";
    public float latitude_data = 0;
    public float longitude_data = 0;
    public float altitude_data = 0;

    public GPSdata(string Name, float Lat, float Lon, float ALT)
    {
        name = Name;
        latitude_data = Lat;
        longitude_data = Lon;
        altitude_data = ALT;
    }
}