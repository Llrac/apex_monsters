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
            Destroy(Instance);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);

        if (FindObjectOfType<AudioManager>() != null)
        {
            FindObjectOfType<AudioManager>().transform.SetParent(gameObject.transform);
            am = GetComponentInChildren<AudioManager>();
        }
        if (FindObjectOfType<GameManager>() != null)
        {
            FindObjectOfType<GameManager>().transform.SetParent(gameObject.transform);
            gm = GetComponentInChildren<GameManager>();
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("deleted all keys");
        }
    }

    public void LoadNextScene()
    {
        lastScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(lastScene.buildIndex + 1);

        //FindObjectOfType<PlayerSaveDataManager>().ResetSavedData();

        if (FindObjectOfType<Monster>() == null) { return; }

        monsterIDs.Clear();

        foreach (Monster monster in FindObjectsOfType<Monster>())
        {
            if (monster.insideInventory && monster.type != "Baby")
            {
                monsterIDs.Add(monster.monsterID);
            }
        }
        FindObjectOfType<DatabaseManager>().SavePlayerData();
        FindObjectOfType<DatabaseManager>().DebugSavedData();
    }

    public void LoadScanQRScene()
    {
        SceneManager.LoadScene("ScanQR");
    }

    public void LoadShowQRScene()
    {
        SceneManager.LoadScene("ShowQR");
    }
}
