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

    public void SavePlayerData()
    {
        playerSaveData = new();
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
}
