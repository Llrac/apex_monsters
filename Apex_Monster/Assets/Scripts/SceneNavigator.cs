using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    public static SceneNavigator Instance { get; set; }

    Scene lastScene;
    AudioManager am;
    GameManager gm;

    [HideInInspector] public List<int> monsterIDs = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            foreach (AudioManager audioManager in FindObjectsOfType<AudioManager>())
            {
                if (!audioManager.sceneNavigated)
                {
                    Destroy(audioManager.gameObject);
                }
            }
            foreach (GameManager gameManager in FindObjectsOfType<GameManager>())
            {
                if (!gameManager.sceneNavigated)
                {
                    Destroy(gameManager.gameObject);
                }
            }
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;

        GetComponent<DatabaseManager>().OnSceneLoad();

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
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("deleted all keys");
        }
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.S))
        {
            FindObjectOfType<DatabaseManager>().SendMessage(FindObjectOfType<AccountSettings>().GetUserID, "browhat");
        }
    }

    private void SaveAndDebugPlayerData()
    {
        GetComponent<DatabaseManager>().SavePlayerData();
        GetComponent<DatabaseManager>().DebugSaveData();
    }

    public void LoadNextScene()
    {
        lastScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(lastScene.buildIndex + 1);

        PrepareValuesForScene("BattleMonsters");
        SaveAndDebugPlayerData();
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);

        PrepareValuesForScene(sceneName);
        SaveAndDebugPlayerData();
    }

    void PrepareValuesForScene(string sceneName)
    {
        if (sceneName == "ShowQR")
        {

        }
        else if (sceneName == "ScanQR")
        {
            //
        }
        else if (sceneName == "ShowOpponent")
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
            foreach (Monster monster in FindObjectsOfType<Monster>())
            {
                if (monster.insideInventory && monster.type != "Baby")
                {
                    monsterIDs.Add(monster.monsterID);
                }
            }
        }
    }
}
