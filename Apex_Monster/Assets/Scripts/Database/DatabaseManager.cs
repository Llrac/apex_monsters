using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

[Serializable]
public class GameData
{
    public int gameProgress = 0;

    // user who shows QR
    public string hostUserID;
    public string hostUsername;
    public string hostSpriteTag;
    public List<int> hostMonsterIDs;
    public List<int> hostMonsterPowers;
    public List<float> hostMonsterSizes;

    // user who scans QR
    public string clientUserID;
    public string clientUsername;
    public string clientSpriteTag;
    public List<int> clientMonsterIDs;
    public List<int> clientMonsterPowers;
    public List<float> clientMonsterSizes;
}

[Serializable]
public class UserData
{
    public string username;
    public string ppSpriteTag;
    public List<int> monsterIDs; // crucial *gameplay* info
    public List<int> monsterPowers; // crucial *gameplay* info
    public List<float> monsterSizes; // crucial *gameplay* info
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

    public string currentGameID = null;
    public string currentOpponentUserID = null;

    public bool maySpawnOpposingMonsters = false;

    public List<int> currentHostMonsterIDs = new();
    public List<int> currentHostMonsterPowers = new();
    public List<float> currentHostMonsterSizes = new();

    public List<int> currentClientMonsterIDs = new();
    public List<int> currentClientMonsterPowers = new();
    public List<float> currentClientMonsterSizes = new();

    public void GetDatabase()
    {
        db = FirebaseDatabase.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        db.SetPersistenceEnabled(false); //Fix data cache problems
    }

    public void ResetUserData() { userData = new(); }
    public void ResetGameData() { gameData = new(); }

    public void RenewUserData()
    {
        if (userData == null) { ResetUserData(); }

        GetUserData();

        //userData.monsterIDs = GetComponent<SceneNavigator>().monsterIDs;
        //userData.monsterPowers = GetComponent<SceneNavigator>().monsterPowers;
        //userData.monsterSizes = GetComponent<SceneNavigator>().monsterSizes;

        if (FindObjectOfType<ProfilePicture>())
            userData.ppSpriteTag = FindObjectOfType<ProfilePicture>().spriteTag;

        if (FindObjectOfType<AccountSettings>())
            userData.username = FindObjectOfType<AccountSettings>().username.text;

        maySpawnOpposingMonsters = false;

        GetDatabase();
        db.RootReference.Child("users").Child(GetUserID).SetRawJsonValueAsync(JsonUtility.ToJson(userData)).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
                Debug.LogWarning(task.Exception);

            //Call our delegate if it's not null
            //onSaveDelegate?.Invoke();
        });
    }

    public void RenewGameData(string gameID)
    {
        if (gameData == null) { ResetGameData(); }

        // get whether this user is host or client
        GetDatabase();
        if (FindObjectOfType<QRCodeGenerator>()) // user is host
        {
            gameData.hostUserID = GetUserID;
            gameData.hostUsername = FindObjectOfType<AccountSettings>().username.text;
            gameData.hostSpriteTag = FindObjectOfType<ProfilePicture>().spriteTag;
        }
        else if (FindObjectOfType<QRCodeScanner>()) // user is client
        {
            gameData.clientUserID = GetUserID;
            gameData.clientUsername = FindObjectOfType<AccountSettings>().username.text;
            gameData.clientSpriteTag = FindObjectOfType<ProfilePicture>().spriteTag;
        }

        gameData.hostMonsterIDs = null;
        gameData.hostMonsterPowers = null;
        gameData.hostMonsterSizes = null;

        gameData.clientMonsterIDs = null;
        gameData.clientMonsterPowers = null;
        gameData.clientMonsterSizes = null;

        // remove all games hosted by this user
        if (db.RootReference.Child("games").Child(gameData.hostUserID).ToString().Contains(gameData.hostUserID))
        {
            //Debug.Log("removed all games hosted by " + gameData.hostUserID);

            db.RootReference.Child("games").Child(gameData.hostUserID).RemoveValueAsync();
        }

        // create a new game hosted by this user
        db.RootReference.Child("games").Child(gameID).SetRawJsonValueAsync(JsonUtility.ToJson(gameData)).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
                Debug.LogWarning(task.Exception);

            //Call our delegate if it's not null
            //onSaveDelegate?.Invoke();
        });
    }

    private void Start()
    {
        StartCoroutine(RepeatGetGameData());
    }

    IEnumerator RepeatGetGameData()
    {
        yield return new WaitForSeconds(0.5f);
        GetDatabase();
        GetGameData(GetUserID);

        yield return StartCoroutine(RepeatGetGameData());
    }

    public void JoinTestGame(int number)
    {
        if (number == 1)
            JoinGame("TVCryXjrNIZdbvvAewW7ybTk28H3");
        else if (number == 2)
            JoinGame("xgBJKkDOaPfdX16ERnZDlAk98Ai1");
    }

    public void JoinGame(string gameID)
    {
        GetDatabase();
        if (db.RootReference.Child("games").Child(gameID).ToString().Contains(gameID))
        {
            GetGameData(gameID, true);
        }
    }

    public void GetGameData(string gameID, bool tryJoin = false)
    {
        GetDatabase();
        if (tryJoin)
            LoadData("games/" + gameID, TryJoinGame);
        else
            LoadData("games/" + gameID, GetGameData);
    }

    public void GetUserData()
    {
        GetDatabase();
        LoadData("users/" + GetUserID, OnGetUserData);
    }

    public void GetShowUsersData()
    {
        GetDatabase();
        LoadData("games/" + currentGameID, OnShowUsersData);
    }

    public void GetBattleMonstersData()
    {
        GetDatabase();
        LoadData("games/" + currentGameID, OnBattleMonstersData);
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

    void OnGetUserData(DataSnapshot snap) // is self
    {
        GetDatabase();
        var loadedData = JsonUtility.FromJson<UserData>(snap.GetRawJsonValue());
        UserData newUserData = new();

        newUserData.ppSpriteTag = loadedData.ppSpriteTag;

        if (FindObjectOfType<ProfilePicture>()) // if we need user's profile picture, show it
        {
            ProfilePicture pp = FindObjectOfType<ProfilePicture>();
            foreach (GameObject baby in FindObjectOfType<GameManager>().babies)
            {
                if (!baby.CompareTag(newUserData.ppSpriteTag)) { continue; }
                foreach (Transform child in pp.transform)
                {
                    if (child.name != "Sprite") { continue; }
                    child.gameObject.GetComponent<UnityEngine.UI.Image>().sprite = baby.GetComponentInChildren<SpriteRenderer>().sprite;
                    pp.SpriteTag = baby.tag;
                    pp.spriteTag = pp.SpriteTag;
                }
            }
        }

        if (FindObjectOfType<AccountSettings>()) // if we need user's username, show it
        {
            AccountSettings accset = FindObjectOfType<AccountSettings>();
        }

        // update any changes
        db.RootReference.Child("users").Child(GetUserID).SetRawJsonValueAsync(JsonUtility.ToJson(newUserData)).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
                Debug.LogWarning(task.Exception);
        });
    }

    void OnShowUsersData(DataSnapshot snap) // both players are in the same game
    {
        GetDatabase();
        var loadedData = JsonUtility.FromJson<GameData>(snap.GetRawJsonValue());
        GameData newGameData = new();

        newGameData.gameProgress = loadedData.gameProgress;

        newGameData.hostUserID = loadedData.hostUserID;
        newGameData.hostUsername = loadedData.hostUsername;
        newGameData.hostSpriteTag = loadedData.hostSpriteTag;

        newGameData.clientUserID = loadedData.clientUserID;
        newGameData.clientUsername = loadedData.clientUsername;
        newGameData.clientSpriteTag = loadedData.clientSpriteTag;

        if (FindObjectOfType<ProfilePicture>())
        {
            // opponent is client
            if (GetUserID == newGameData.hostUserID)
            {
                foreach (ProfilePicture pp in FindObjectsOfType<ProfilePicture>())
                {
                    if (pp.forThisUser)
                    {
                        pp.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = newGameData.hostUsername;
                        pp.SetManualProfilePicture(newGameData.hostSpriteTag);
                    }
                    else if (!pp.forThisUser)
                    {
                        pp.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = newGameData.clientUsername;
                        pp.SetManualProfilePicture(newGameData.clientSpriteTag);
                    }
                }
            }
            // opponent is host
            else if (GetUserID == newGameData.clientUserID)
            {
                foreach (ProfilePicture pp in FindObjectsOfType<ProfilePicture>())
                {
                    if (pp.forThisUser)
                    {
                        pp.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = newGameData.clientUsername;
                        pp.SetManualProfilePicture(newGameData.clientSpriteTag);
                    }
                    else if (!pp.forThisUser)
                    {
                        pp.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = newGameData.hostUsername;
                        pp.SetManualProfilePicture(newGameData.hostSpriteTag);
                    }
                }
            }
        }
    }

    void OnBattleMonstersData(DataSnapshot snap) // both players are in the same game
    {
        GetDatabase();
        var loadedData = JsonUtility.FromJson<GameData>(snap.GetRawJsonValue());
        GameData newGameData = new();

        newGameData.gameProgress = loadedData.gameProgress;

        newGameData.hostUserID = loadedData.hostUserID;
        newGameData.hostMonsterIDs = loadedData.hostMonsterIDs;
        newGameData.hostMonsterPowers = loadedData.hostMonsterPowers;
        newGameData.hostMonsterSizes = loadedData.hostMonsterSizes;

        newGameData.clientUserID = loadedData.clientUserID;
        newGameData.clientMonsterIDs = loadedData.clientMonsterIDs;
        newGameData.clientMonsterPowers = loadedData.clientMonsterPowers;
        newGameData.clientMonsterSizes = loadedData.clientMonsterSizes;

        // if this user is host
        if (GetUserID == newGameData.hostUserID)
        {
            Debug.Log("host");

            newGameData.hostMonsterIDs = GetComponent<SceneNavigator>().myMonsterIDs;
            newGameData.hostMonsterPowers = GetComponent<SceneNavigator>().myMonsterPowers;
            newGameData.hostMonsterSizes = GetComponent<SceneNavigator>().myMonsterSizes;

            maySpawnOpposingMonsters = true;

            db.RootReference.Child("games").Child(newGameData.hostUserID).SetRawJsonValueAsync(JsonUtility.ToJson(newGameData)).ContinueWithOnMainThread(task =>
            {
                if (task.Exception != null)
                    Debug.LogWarning(task.Exception);
            });
        }
        // if this user is client
        else if (GetUserID == newGameData.clientUserID)
        {
            Debug.Log("client");

            newGameData.clientMonsterIDs = GetComponent<SceneNavigator>().myMonsterIDs;
            newGameData.clientMonsterPowers = GetComponent<SceneNavigator>().myMonsterPowers;
            newGameData.clientMonsterSizes = GetComponent<SceneNavigator>().myMonsterSizes;

            maySpawnOpposingMonsters = true;

            db.RootReference.Child("games").Child(newGameData.hostUserID).SetRawJsonValueAsync(JsonUtility.ToJson(newGameData)).ContinueWithOnMainThread(task =>
            {
                if (task.Exception != null)
                    Debug.LogWarning(task.Exception);
            });
        }
    }

    void GetGameData(DataSnapshot snap) // is either host or client
    {
        GetDatabase();
        var loadedData = JsonUtility.FromJson<GameData>(snap.GetRawJsonValue());
        GameData newGameData = new();

        newGameData.gameProgress = loadedData.gameProgress;

        newGameData.hostUserID = loadedData.hostUserID;
        newGameData.hostUsername = loadedData.hostUsername;
        newGameData.hostSpriteTag = loadedData.hostSpriteTag;
        newGameData.hostMonsterIDs = loadedData.hostMonsterIDs;
        newGameData.hostMonsterPowers = loadedData.hostMonsterPowers;
        newGameData.hostMonsterSizes = loadedData.hostMonsterSizes;

        newGameData.clientUserID = loadedData.clientUserID;
        newGameData.clientUsername = loadedData.clientUsername;
        newGameData.clientSpriteTag = loadedData.clientSpriteTag;
        newGameData.clientMonsterIDs = loadedData.clientMonsterIDs;
        newGameData.clientMonsterPowers = loadedData.clientMonsterPowers;
        newGameData.clientMonsterSizes = loadedData.clientMonsterSizes;

        // if this user is host
        if (newGameData.hostUserID == GetUserID && newGameData.clientUserID != "" && newGameData.hostUserID != newGameData.clientUserID)
        {
            currentGameID = newGameData.hostUserID;
            currentOpponentUserID = newGameData.clientUserID;

            if (newGameData.gameProgress == 1)
            {
                // show users
                newGameData.gameProgress = 2;
                
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().ToString() != "ShowUsers") { GetComponent<SceneNavigator>().LoadScene("ShowUsers"); }
            }
            else if (newGameData.gameProgress == 2 && maySpawnOpposingMonsters)
            {
                newGameData.gameProgress = 3;

                Debug.Log("frick yes");
            }
        }
        // if this user is client
        else if (newGameData.clientUserID == GetUserID && newGameData.hostUserID != "" && newGameData.hostUserID != newGameData.clientUserID)
        {
            if (newGameData.gameProgress == 3)
            {
                newGameData.gameProgress = 4;

                Debug.Log("click yes");

                //for (int i = 0; i < newGameData.hostMonsterIDs.Count; i++)
                //{

                //}
            }
        }

        db.RootReference.Child("games").Child(newGameData.hostUserID).SetRawJsonValueAsync(JsonUtility.ToJson(newGameData)).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
                Debug.LogWarning(task.Exception);
        });
    }

    void TryJoinGame(DataSnapshot snap) // is client
    {
        GetDatabase();
        var loadedData = JsonUtility.FromJson<GameData>(snap.GetRawJsonValue());
        GameData newGameData = new();

        newGameData.gameProgress = loadedData.gameProgress;

        newGameData.hostUserID = loadedData.hostUserID;
        newGameData.hostUsername = loadedData.hostUsername;
        newGameData.hostSpriteTag = loadedData.hostSpriteTag;

        newGameData.clientUserID = GetUserID;
        newGameData.clientUsername = FindObjectOfType<AccountSettings>().username.text;
        newGameData.clientSpriteTag = FindObjectOfType<ProfilePicture>().spriteTag;

        // checks if joining game was successful
        if (newGameData.hostUserID != "" && newGameData.hostUserID != newGameData.clientUserID)
        {
            currentGameID = newGameData.hostUserID;
            currentOpponentUserID = newGameData.hostUserID;

            if (newGameData.gameProgress == 0 && UnityEngine.SceneManagement.SceneManager.GetActiveScene().ToString() != "ShowUsers")
            {
                // show users
                newGameData.gameProgress = 1;
                db.RootReference.Child("games").Child(newGameData.hostUserID).SetRawJsonValueAsync(JsonUtility.ToJson(newGameData)).ContinueWithOnMainThread(task =>
                {
                    if (task.Exception != null)
                        Debug.LogWarning(task.Exception);
                });
                GetComponent<SceneNavigator>().LoadScene("ShowUsers");
            }
        }
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
