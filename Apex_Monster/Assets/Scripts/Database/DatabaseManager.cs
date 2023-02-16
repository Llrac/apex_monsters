using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

[Serializable]
public class UserData
{
    public string username;
    public string ppSpriteTag;
    public List<int> monsterIDs; // crucial info for gameplay
}

[Serializable]
public class GameData
{
    public string gameID;
    public string hostUserID; // user who shows QR
    public string clientUserID; // user who scans QR
}

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get { return _instance; } }
    readonly static DatabaseManager _instance;
    public string GetUserID { get { return auth.CurrentUser.UserId; } }

    UserData userData;
    GameData gameData;
    FirebaseDatabase db;
    FirebaseAuth auth;

    public delegate void OnLoadedDelegate(DataSnapshot snapshot);
    public delegate void OnSaveDelegate();

    public void GetDatabase()
    {
        db = FirebaseDatabase.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        db.SetPersistenceEnabled(false); //Fix data cache problems
    }

    public void ResetUserData() { userData = new(); }
    public void ResetGameData() { gameData = new(); }

    //void PushData(string path, string data, OnSaveDelegate onSaveDelegate = null)
    //{
    //    GetDatabase();
    //    db.RootReference.Child(path).Push().SetRawJsonValueAsync(data).ContinueWithOnMainThread(task =>
    //    {
    //        if (task.Exception != null)
    //            Debug.LogWarning(task.Exception);

    //        //Call our delegate if it's not null
    //        onSaveDelegate?.Invoke();
    //    });
    //}

    //void SaveData(string path, string data, OnSaveDelegate onSaveDelegate = null)
    //{
        
    //}

    public void UpdateUserData()
    {
        if (userData == null) { ResetUserData(); }

        userData.monsterIDs = GetComponent<SceneNavigator>().monsterIDs;

        if (FindObjectOfType<ProfilePicture>())
            userData.ppSpriteTag = FindObjectOfType<ProfilePicture>().spriteTag;

        if (FindObjectOfType<AccountSettings>())
            userData.username = FindObjectOfType<AccountSettings>().username.text;

        GetDatabase();
        db.RootReference.Child("users").Child(GetUserID).SetRawJsonValueAsync(JsonUtility.ToJson(userData)).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
                Debug.LogWarning(task.Exception);

            //Call our delegate if it's not null
            //onSaveDelegate?.Invoke();
        });
    }

    public void UpdateGameData(string gameID)
    {
        if (gameData == null) { ResetGameData(); }

        GetDatabase();
        gameData.gameID = gameID;
        if (FindObjectOfType<QRCodeGenerator>()) // user is host
        {
            gameData.hostUserID = GetUserID;
        }
        else if (FindObjectOfType<QRCodeScanner>()) // user is client
        {
            gameData.clientUserID = GetUserID;
        }

        db.RootReference.Child("games").Child(gameData.gameID).SetRawJsonValueAsync(JsonUtility.ToJson(gameData)).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
                Debug.LogWarning(task.Exception);

            //Call our delegate if it's not null
            //onSaveDelegate?.Invoke();
        });
    }

    void LoadData(string path, OnLoadedDelegate onLoadedDelegate)
    {
        GetDatabase();
        db.RootReference.Child(path).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
                Debug.LogWarning(task.Exception);

            DataSnapshot snap = task.Result;
            onLoadedDelegate(snap);
        });
    }

    public void JoinGame(string gameID)
    {
        UpdateGameData(gameID);
        //LoadData("games/" + gameID, OnLoadedGameData);
    }

    //public void JoinTestGame(int number)
    //{
    //    if (number == 1)
    //        LoadData("games/TVCryXjrNIZdbvvAewW7ybTk28H3", OnLoadedGameData);
    //    else if (number == 2)
    //        LoadData("games/xgBJKkDOaPfdX16ERnZDlAk98Ai1", OnLoadedGameData);
    //}

    public void LoadUserData()
    {
        LoadData("users/" + GetUserID, OnLoadedUserData);
    }

    public void LoadGameData()
    {
        LoadData("games/" + gameData.gameID, OnLoadedGameData);
    }

    void OnLoadedUserData(DataSnapshot snap)
    {
        Debug.Log(snap.GetRawJsonValue());
        var loadedData = JsonUtility.FromJson<UserData>(snap.GetRawJsonValue());

        if (FindObjectOfType<ProfilePicture>()) // if we need profile picture, use it
        {
            ProfilePicture pp = FindObjectOfType<ProfilePicture>();
            foreach (GameObject baby in FindObjectOfType<GameManager>().babies)
            {
                if (!baby.CompareTag(loadedData.ppSpriteTag)) { continue; }
                foreach (Transform child in pp.transform)
                {
                    if (child.name != "Sprite") { continue; }
                    child.gameObject.GetComponent<UnityEngine.UI.Image>().sprite = baby.GetComponentInChildren<SpriteRenderer>().sprite;
                    pp.SpriteTag = baby.tag;
                    pp.spriteTag = pp.SpriteTag;
                }
            }
        }

        if (FindObjectOfType<AccountSettings>()) // if we need username, use it
        {
            AccountSettings accset = FindObjectOfType<AccountSettings>();
            Debug.Log(accset.username.text);
        }
    }

    void OnLoadedGameData(DataSnapshot snap)
    {
        Debug.Log(snap.GetRawJsonValue());
        var loadedData = JsonUtility.FromJson<GameData>(snap.GetRawJsonValue());

    }

    #region Unused Code

    //public void ListGames()
    //{
    //    Debug.Log("Listing Games");

    //    foreach (Transform child in publicGamesListHolder)
    //        GameObject.Destroy(child.gameObject);

    //    LoadMultipleData("games/", ShowGames);
    //}

    //public void ShowGames(string json)
    //{
    //    var gameInfo = JsonUtility.FromJson<GameData>(json);

    //    if (userInfo.activeGames.Contains(gameInfo.gameID) || gameInfo.players.Count > 1)
    //    {
    //        //Don't list our own games or full games.
    //        return;
    //    }

    //    var newButton = Instantiate(buttonPrefab, publicGamesListHolder).GetComponent<Button>();
    //    newButton.GetComponentInChildren<TextMeshProUGUI>().text = gameInfo.displayName;
    //    newButton.onClick.AddListener(() => JoinGame(gameInfo));
    //}

    //Save the data at the given path, save callback optional


    ////Used to remove multiple data from the database based on their age.
    //void RemoveOldestData(string path, int amount)
    //{
    //    GetDatabase();
    //    //First load the data
    //    db.RootReference.Child(path).GetValueAsync().ContinueWithOnMainThread(task =>
    //    {
    //        if (task.Exception != null)
    //            Debug.LogWarning(task.Exception);

    //        if (task.Result.ChildrenCount < amount)
    //        {
    //            amount = (int)task.Result.ChildrenCount;
    //        }

    //        //Call remove on each of the data, that we want gone.
    //        int i = 0;
    //        foreach (var item in task.Result.Children)
    //        {
    //            i++;
    //            RemoveData(path + "/" + item.Key);

    //            if (i >= amount)
    //            {
    //                break;
    //            }
    //        }
    //    });
    //}

    //void RemoveData(string path)
    //{
    //    GetDatabase();
    //    db.RootReference.Child(path).RemoveValueAsync();
    //}


    //public void SetUpFirebase()
    //{
    //    //Setup for talking to firebase
    //    FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
    //    {
    //        //Log if we get any errors from the opperation
    //        if (task.Exception != null)
    //            Debug.LogError(task.Exception);

    //        GetDatabase();
    //        db.RootReference.Child("users").SetValueAsync("World");
    //    });
    //}
    #endregion
}
