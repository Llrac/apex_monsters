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
    
    public bool showingAccountSettings = false;
    public List<GameObject> debugList = new();

    public bool doCountdown = false;
    public GameObject countdownObject = null;
    public int myMonstersMaxTime = 30;
    public float myMonstersRemainingTime = 0;
    public int myMonstersRemainingTimeInteger = 0;

    [HideInInspector] public List<int> myMonsterIDs = new();
    [HideInInspector] public List<int> myMonsterPowers = new();
    [HideInInspector] public List<float> myMonsterSizes = new();

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
            Instance.myMonstersRemainingTimeInteger = Mathf.FloorToInt(Instance.myMonstersRemainingTime);

            if (Instance.myMonstersRemainingTime <= 0)
            {
                Instance.myMonstersRemainingTime = Instance.myMonstersMaxTime;
                Instance.doCountdown = false;
                LoadScene("BattleMonsters");
            }

            else
            {
                if (!Instance.countdownObject)
                    Instance.countdownObject = GameObject.Find("COUNTDOWN").transform.GetChild(0).gameObject;
                Instance.countdownObject.GetComponent<TMPro.TextMeshProUGUI>().text = "Time remaining: " + Instance.myMonstersRemainingTimeInteger.ToString().Normalize() + "s";
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

                // play according music
                FindObjectOfType<AudioManager>().backgroundMusicAS.gameObject.SetActive(false);
                FindObjectOfType<AudioManager>().battleMusicAS.gameObject.SetActive(true);

                Invoke(nameof(ShowUsersCountdown), 5);
                break;
            case "MyMonsters":
                // play according music
                FindObjectOfType<AudioManager>().backgroundMusicAS.gameObject.SetActive(true);
                FindObjectOfType<AudioManager>().battleMusicAS.gameObject.SetActive(false);

                // spawn monsters and set up timer
                FindObjectOfType<MonsterSpawner>().spawnBabies = true;
                Instance.doCountdown = true;
                Instance.myMonstersRemainingTime = Instance.myMonstersMaxTime;
                break;
            case "BattleMonsters":
                Instance.doCountdown = false;

                // play according music
                FindObjectOfType<AudioManager>().backgroundMusicAS.gameObject.SetActive(false);
                FindObjectOfType<AudioManager>().battleMusicAS.gameObject.SetActive(true);

                if (!FindObjectOfType<Monster>()) { Debug.Log("found no monsters"); return; }
                
                myMonsterIDs.Clear();
                myMonsterPowers.Clear();
                myMonsterSizes.Clear();

                foreach (Monster monster in FindObjectsOfType<Monster>().Where(m => m.GetComponent<Monster>().insideInventory && m.GetComponent<Monster>().type != "Baby"))
                {
                    myMonsterIDs.Add(monster.monsterID);
                    myMonsterPowers.Add(monster.power);
                    myMonsterSizes.Add(monster.startSize.x);
                    MonsterSpawner.SpawnMonsterByID(monster.monsterID, monster.power, monster.startSize.x);
                }

                GetComponent<DatabaseManager>().GetBattleMonstersData();

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
