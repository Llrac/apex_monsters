using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Firebase.Database;
using Firebase.Extensions;
using Firebase;

[Serializable]
public class SaveData
{
    public List<int> monsterIDs;
    public string ppSpriteTag;
}

public class DatabaseManager : MonoBehaviour
{
    SaveData saveData;

    FirebaseDatabase db;

    //Function that gets called after load or save
    public delegate void OnLoadedDelegate(DataSnapshot snapshot); //when we want the json stuff
    public delegate void OnLoadedDelegate<T>(T data); //When we want an object directly
    public delegate void OnLoadedMultipleDelegate<T>(List<T> data); //list of objects
    public delegate void OnSaveDelegate();



    #region Roberts Code
    //loads the data at "path" then returns json result to the delegate/callback function
    public void LoadData<T>(string path, OnLoadedDelegate<T> onLoadedDelegate)
    {
        db.RootReference.Child(path).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
                Debug.LogWarning(task.Exception);

            onLoadedDelegate(JsonUtility.FromJson<T>(task.Result.GetRawJsonValue()));
        });
    }

    //Returns one list of objects that we want to load from the database
    public void LoadMultipleData<T>(string path, OnLoadedMultipleDelegate<T> onLoadedDelegate)
    {
        db.RootReference.Child(path).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
                Debug.LogWarning(task.Exception);

            var listOfT = new List<T>();

            foreach (var item in task.Result.Children)
                listOfT.Add(JsonUtility.FromJson<T>(item.GetRawJsonValue()));

            onLoadedDelegate(listOfT);
        });
    }

    //Save the data at the given path, save callback optional
    public void SaveData(string path, string data, OnSaveDelegate onSaveDelegate = null)
    {
        db.RootReference.Child(path).SetRawJsonValueAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
                Debug.LogWarning(task.Exception);

            //Call our delegate if it's not null
            onSaveDelegate?.Invoke();
        });
    }

    //Save the data at the given path, save callback optional
    public void PushData(string path, string data, OnSaveDelegate onSaveDelegate = null)
    {
        db.RootReference.Child(path).Push().SetRawJsonValueAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
                Debug.LogWarning(task.Exception);

            //Call our delegate if it's not null
            onSaveDelegate?.Invoke();
        });
    }

    //Used to remove multiple data from the database based on their age.
    public void RemoveOldestData(string path, int amount)
    {
        //First load the data
        db.RootReference.Child(path).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
                Debug.LogWarning(task.Exception);

            if (task.Result.ChildrenCount < amount)
            {
                amount = (int)task.Result.ChildrenCount;
            }

            //Call remove on each of the data, that we want gone.
            int i = 0;
            foreach (var item in task.Result.Children)
            {
                i++;
                RemoveData(path + "/" + item.Key);

                if (i >= amount)
                {
                    break;
                }
            }
        });
    }

    public void RemoveData(string path)
    {
        db.RootReference.Child(path).RemoveValueAsync();
    }
    #endregion



    public void SendMessage(string userID, string data)
    {
        Debug.Log("Trying to write data...");
        var db = FirebaseDatabase.DefaultInstance;
        db.RootReference.Child("users").Child(userID).SetValueAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
                Debug.LogWarning(task.Exception);
            else
                Debug.Log("DataTestWrite: Complete");
        });
        LoadSavedData();
    }

    public void SetUpFirebase()
    {
        //Setup for talking to firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            //Log if we get any errors from the opperation
            if (task.Exception != null)
                Debug.LogError(task.Exception);

            //the database
            db = FirebaseDatabase.DefaultInstance;

            //Set the value World to the key Hello in the database
            db.RootReference.Child("users").SetValueAsync("World");
        });
    }

    public void OnSceneLoad()
    {
        db = FirebaseDatabase.DefaultInstance;
        db.SetPersistenceEnabled(false); //Fix data cache problems
    }

    public void ResetSaveData()
    {
        saveData = new();
    }

    public void SavePlayerData()
    {
        if (saveData == null) { ResetSaveData(); }

        saveData.monsterIDs = GetComponent<SceneNavigator>().monsterIDs;

        if (FindObjectOfType<ProfilePicture>())
            saveData.ppSpriteTag = FindObjectOfType<ProfilePicture>().spriteTag;

        // this is the last thing we do before uploading the data to the database
        string jsonSaveData = JsonUtility.ToJson(saveData);

        // upload save data to database
        PushData("users/", "I am responding");
    }

    public void DebugSaveData()
    {
        foreach (int monsterID in saveData.monsterIDs)
        {
            MonsterSpawner.SpawnMonsterID(monsterID);
            Debug.Log(monsterID);
        }
        Debug.Log(saveData.ppSpriteTag);
    }

    void OnUserLoaded(SaveData saveData)
    {
        Debug.Log(saveData);
    }

    public void LoadSavedData()
    {
        LoadData<SaveData>("users/" + FindObjectOfType<AccountSettings>().GetUserID, OnUserLoaded);
        Debug.Log("okay");

        //foreach (GameObject baby in FindObjectOfType<GameManager>().babies)
        //{
        //    if (baby.CompareTag(saveData.ppSpriteTag))
        //    {
        //        foreach (Transform child in GameObject.Find("ProfilePicture").transform)
        //        {
        //            if (child.name == "Sprite")
        //            {
        //                child.gameObject.GetComponent<UnityEngine.UI.Image>().sprite = baby.GetComponent<SpriteRenderer>().sprite;
        //            }
        //        }
        //    }
        //}
    }
}
