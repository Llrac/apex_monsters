using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class SceneNavigator : MonoBehaviour
{
    public static SceneNavigator Instance { get; set; }

    //Scene lastScene;
    AudioManager am;
    GameManager gm;
    MonsterSpawner ms;

    [HideInInspector] public List<int> monsterIDs = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            foreach (var item in FindObjectsOfType<AudioManager>().Where(am => !am.sceneNavigated)) { Destroy(item.gameObject); }
            foreach (var item in FindObjectsOfType<GameManager>().Where(gm => !gm.sceneNavigated)) { Destroy(item.gameObject); }
            foreach (var item in FindObjectsOfType<MonsterSpawner>().Where(ms => !ms.sceneNavigated)) { Destroy(item.gameObject); }
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;

        if (FindObjectOfType<AudioManager>())
        {
            FindObjectOfType<AudioManager>().transform.SetParent(gameObject.transform);
            am = GetComponentInChildren<AudioManager>();
            am.sceneNavigated = true;
        }
        if (FindObjectOfType<GameManager>())
        {
            FindObjectOfType<GameManager>().transform.SetParent(gameObject.transform);
            gm = GetComponentInChildren<GameManager>();
            gm.sceneNavigated = true;
        }
        if (FindObjectOfType<MonsterSpawner>())
        {
            FindObjectOfType<MonsterSpawner>().transform.SetParent(gameObject.transform);
            ms = GetComponentInChildren<MonsterSpawner>();
            ms.sceneNavigated = true;
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("deleted all keys");
        }
        //if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.S))
        //{
        //    FindObjectOfType<DatabaseManager>().SendMessage(FindObjectOfType<AccountSettings>().GetUserID, "got message");
        //}
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);

        GetDataForScene(sceneName);

        GetComponent<DatabaseManager>().SaveUserData();
    }

    void GetDataForScene(string sceneName)
    {
        if (sceneName == "ShowQR")
        {

        }
        else if (sceneName == "ScanQR")
        {
            
        }
        else if (sceneName == "ShowUsers")
        {
            // get playerName and profilePicture from ShowQR so that both players see each other's names and pictures
        }
        else if (sceneName == "MyMonsters")
        {
            // nothing
        }
        else if (sceneName == "BattleMonsters")
        {
            // get monsters from MyMonsters that you will go to battle with
            if (FindObjectOfType<Monster>() == null) { return; }
            monsterIDs.Clear();
            foreach (Monster monster in FindObjectsOfType<Monster>().Where(m => m.GetComponent<Monster>().insideInventory && m.GetComponent<Monster>().type != "Baby"))
            {
                monsterIDs.Add(monster.monsterID);
            }
        }
    }
}
