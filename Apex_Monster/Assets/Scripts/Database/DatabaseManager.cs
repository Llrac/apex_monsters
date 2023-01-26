using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SaveData
{
    public List<int> monsterIDs;
    public Sprite profilePicture;
}

public class DatabaseManager : MonoBehaviour
{
    SaveData playerSaveData;
    //Sprite[] possibleProfilePictures = new Sprite[10];

    public void ResetPlayerData()
    {
        playerSaveData = new();
    }

    public void SavePlayerData()
    {
        if (playerSaveData == null) { ResetPlayerData(); }

        playerSaveData.monsterIDs = GetComponent<SceneNavigator>().monsterIDs;

        if (FindObjectOfType<ProfilePicture>() != null)
            playerSaveData.profilePicture = FindObjectOfType<ProfilePicture>().ppSprite;
    }

    public void DebugSavedData()
    {
        foreach (int monsterID in playerSaveData.monsterIDs)
        {
            MonsterSpawner.SpawnMonsterID(monsterID);
            Debug.Log(monsterID);
        }
        Debug.Log(playerSaveData.profilePicture);
    }

    public void LoadSavedData()
    {

    }
}
