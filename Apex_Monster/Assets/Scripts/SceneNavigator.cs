using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class SceneNavigator : MonoBehaviour
{
    public static SceneNavigator Instance { get; set; }
    [HideInInspector] public bool sceneNavigator = false;

    AudioManager am;
    GameManager gm;
    MonsterSpawner ms;

    [HideInInspector] public List<int> monsterIDs = new();
    public bool showingAccountSettings = false;
    public List<GameObject> debugList = new();

    public bool doCountdown = false;
    public int myMonstersMaxTime = 30;
    public float myMonstersRemainingTime = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            foreach (var item in FindObjectsOfType<SceneNavigator>().Where(sn => sn.sceneNavigator)) { item.ToggleDebugging(false); }
            foreach (var item in FindObjectsOfType<AudioManager>().Where(am => !am.sceneNavigated)) { Destroy(item.gameObject); }
            foreach (var item in FindObjectsOfType<GameManager>().Where(gm => !gm.sceneNavigated)) { Destroy(item.gameObject); }
            foreach (var item in FindObjectsOfType<MonsterSpawner>().Where(ms => !ms.sceneNavigated)) { Destroy(item.gameObject); }

            Destroy(gameObject);
            return;
        }
        Application.targetFrameRate = 60;
        DontDestroyOnLoad(gameObject);
        Instance = this;
        sceneNavigator = true;
        ToggleDebugging(false);

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

    public void ToggleDebugging(bool doToggle = true)
    {
        debugList.Clear();
        foreach (GameObject item in FindObjectsOfType<GameObject>().Where(a => a.CompareTag("Debug"))) { debugList.Add(item); }

        if (doToggle)
            showingAccountSettings = !showingAccountSettings;

        if (showingAccountSettings)
        {
            foreach (GameObject item in debugList)
            {
                foreach (Transform child in item.transform) { child.gameObject.SetActive(true); }
            }
        }

        else
        {
            foreach (GameObject item in debugList)
            {
                foreach (Transform child in item.transform) { child.gameObject.SetActive(false); }
            }
        }
    }

    void Update()
    {
        if (Instance.doCountdown)
        {
            Instance.myMonstersRemainingTime -= Time.deltaTime;
            if (Instance.myMonstersRemainingTime <= 0)
            {
                LoadScene("BattleMonsters");
            }
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("deleted all keys");
        }
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        PrepareDataForScene(sceneName);
    }

    void PrepareDataForScene(string sceneName)
    {
        switch (sceneName)
        {
            default:
                break;
            case "ShowQR":
                break;
            case "ScanQR":
                break;
            case "ShowUsers":
                GetComponent<DatabaseManager>().GetShowUsersData();
                Invoke(nameof(ShowUsersCountdown), 5);
                break;
            case "MyMonsters":
                FindObjectOfType<MonsterSpawner>().spawnBabies = true;
                Instance.doCountdown = true;
                Instance.myMonstersRemainingTime = Instance.myMonstersMaxTime;
                break;
            case "BattleMonsters":
                // get monsters from MyMonsters that you will go to battle with
                if (FindObjectOfType<Monster>() == null) { return; }
                monsterIDs.Clear();
                foreach (Monster monster in FindObjectsOfType<Monster>().Where(m => m.GetComponent<Monster>().insideInventory && m.GetComponent<Monster>().type != "Baby"))
                {
                    monsterIDs.Add(monster.monsterID);
                    MonsterSpawner.SpawnMonsterID(monster.monsterID);
                }
                break;
            case "WinMonsters":
                break;
        }
    }

    void ShowUsersCountdown()
    {
        LoadScene("MyMonsters");
    }
}
