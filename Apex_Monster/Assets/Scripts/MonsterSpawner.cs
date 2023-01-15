using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Particles")]
    [SerializeField] GameObject windGust;
    [SerializeField] GameObject confetti;

    [Header("Screen Customizable")]
    [SerializeField] GameObject screenDot;
    public float inventoryScreenOffset = 1.55f;
    public float screenOffset = 0.75f;
    readonly List<GameObject> screenDots = new();
    Vector2 screenSize;

    [Header("Debug: Spawn Monster Baby")]
    [SerializeField] bool spawnBabies = false;
    [SerializeField] int spawnAmount = 1;
    GameManager gm;

    void Start()
    {
        gm = FindObjectOfType<GameManager>();

        UpdateScreenDots();

        //SpawnRandomBabies(7, 0, 0, true);
    }

    #region Screen Functions
    public void CheckMonstersAtScreenEdge()
    {
        foreach (GameObject dot in screenDots)
        {
            Destroy(dot);
        }

        UpdateScreenDots();

        foreach (Monster monster in FindObjectsOfType<Monster>())
        {
            if (monster.transform.position.x < -screenSize.x + screenOffset || monster.transform.position.x > screenSize.x - screenOffset ||
                monster.transform.position.y < -screenSize.y + inventoryScreenOffset || monster.transform.position.y > screenSize.y - screenOffset)
            {
                RepositionMonster(monster);
            }
        }
    }

    void UpdateScreenDots()
    {
        screenDots.Clear();
        screenSize = new(Screen.width, Screen.height);
        screenSize = Camera.main.ScreenToWorldPoint(screenSize);

        screenDots.Add(Instantiate(screenDot, new Vector2(-screenSize.x, -screenSize.y), Quaternion.identity, transform));
        screenDots.Add(Instantiate(screenDot, new Vector2(screenSize.x, -screenSize.y), Quaternion.identity, transform));
        screenDots.Add(Instantiate(screenDot, new Vector2(-screenSize.x, screenSize.y), Quaternion.identity, transform));
        screenDots.Add(Instantiate(screenDot, new Vector2(screenSize.x, screenSize.y), Quaternion.identity, transform));

        screenDots.Add(Instantiate(screenDot, new Vector2(-screenSize.x + screenOffset, -screenSize.y + inventoryScreenOffset), Quaternion.identity, transform));
        screenDots.Add(Instantiate(screenDot, new Vector2(screenSize.x - screenOffset, -screenSize.y + inventoryScreenOffset), Quaternion.identity, transform));
        screenDots.Add(Instantiate(screenDot, new Vector2(-screenSize.x + screenOffset, screenSize.y - screenOffset), Quaternion.identity, transform));
        screenDots.Add(Instantiate(screenDot, new Vector2(screenSize.x - screenOffset, screenSize.y - screenOffset), Quaternion.identity, transform));
    }

    void RepositionMonster(Monster monster)
    {
        if (monster.insideInventory && monster.mergeProgress == 0)
            return;

        Vector2 lastDirection = monster.gameObject.transform.position;
        monster.gameObject.transform.position = new Vector2(
            Mathf.Clamp(monster.gameObject.transform.position.x, -screenSize.x + screenOffset * 1.2f, screenSize.x - screenOffset * 1.2f),
            Mathf.Clamp(monster.gameObject.transform.position.y, -screenSize.y + inventoryScreenOffset + screenOffset * 0.2f, screenSize.y - screenOffset * 1.2f));

        GameObject newWindGust = Instantiate(windGust);
        newWindGust.transform.position = monster.transform.position;
        Destroy(newWindGust, 1);

        if (lastDirection.x < monster.gameObject.transform.position.x)
        {
            newWindGust.transform.eulerAngles = new Vector3(0, -90, -90);
        }
        else if (lastDirection.x > monster.gameObject.transform.position.x)
        {
            newWindGust.transform.eulerAngles = new Vector3(-180, -90, -90);
        }
        if (lastDirection.y < monster.gameObject.transform.position.y)
        {
            newWindGust.transform.eulerAngles = new Vector3(90, -90, -90);
        }
        else if (lastDirection.y > monster.gameObject.transform.position.y)
        {
            newWindGust.transform.eulerAngles = new Vector3(-90, -90, -90);
        }
        CheckMonstersAtScreenEdge();
    }
    #endregion

    void Update()
    {
        // Debugging
        if (spawnBabies)
        {
            SpawnRandomBabies(spawnAmount, 0, 0, true);
            spawnBabies = false;
        }
    }

    public void Merge(Monster monster1, Monster monster2, bool lastTry = false)
    {
        switch (monster1.type)
        {
            case "Chieftain":
                if (!monster1.CompareTag("Undead") && monster2.CompareTag("Undead") && monster2.type == "Baby")
                {
                    SpawnUndead(0, monster1, monster2);
                    return;
                }
                else if (monster1.CompareTag("Farming"))
                {
                    if (monster2.CompareTag("Plantlike"))
                    {
                        monster1.LoseHealth(1);
                        SpawnRandomBabies(monster2.currentHealth * 2, monster2.transform.position.x, monster2.transform.position.y);
                        monster2.Delete();
                        return;
                    }
                }
                break;

            case "Warrior":
                if (monster2.type == "Warrior" && monster2.CompareTag(monster1.tag))
                {
                    SpawnChieftain(monster1, monster2.transform.position.x, monster2.transform.position.y);
                    monster1.Delete();
                    monster2.Delete();
                    return;
                }
                else if (!monster1.CompareTag("Undead") && monster2.CompareTag("Undead") && monster2.type == "Baby")
                {
                    SpawnUndead(1, monster1, monster2);
                    return;
                }
                else if (monster1.CompareTag("Farming"))
                {
                    if (monster2.CompareTag("Plantlike"))
                    {
                        monster1.LoseHealth(1);
                        SpawnRandomBabies(monster2.currentHealth, monster2.transform.position.x, monster2.transform.position.y);
                        monster2.Delete();
                        return;
                    }
                }
                break;

            case "Magic":
                if (!monster1.CompareTag("Undead") && monster2.CompareTag("Undead") && monster2.type == "Baby")
                {
                    SpawnUndead(2, monster1, monster2);
                    return;
                }
                break;

            case "Beast":
                if (!monster1.CompareTag("Undead") && monster2.CompareTag("Undead") && monster2.type == "Baby")
                {
                    SpawnUndead(3, monster1, monster2);
                    return;
                }
                break;

            case "Bird":
                if (!monster1.CompareTag("Undead") && monster2.CompareTag("Undead") && monster2.type == "Baby")
                {
                    SpawnUndead(4, monster1, monster2);
                    return;
                }
                break;

            case "Baby":
                if (monster1.CompareTag("Farming") && monster2.type == "Baby")
                {
                    SpawnBird(monster2, monster2.transform.position.x, monster2.transform.position.y);
                    monster1.Delete();
                    monster2.Delete();
                    return;
                }
                else if (monster1.CompareTag("Plated") && monster2.type == "Baby")
                {
                    SpawnMounted(monster2, monster2.transform.position.x, monster2.transform.position.y);
                    monster1.Delete();
                    monster2.Delete();
                    return;
                }
                else if (monster1.CompareTag("Cloudy") && monster2.type == "Baby")
                {
                    SpawnMagic(monster2, monster2.transform.position.x, monster2.transform.position.y);
                    monster1.Delete();
                    monster2.Delete();
                    return;
                }
                else if (monster1.CompareTag("Porcine") && monster2.type == "Baby")
                {
                    SpawnBeast(monster2, monster2.transform.position.x, monster2.transform.position.y);
                    monster1.Delete();
                    monster2.Delete();
                    return;
                }
                break;

            case "Boss":
                // do nothing
                break;

            case "Flat":
                // do nothing
                break;

            case "Bobble":
                // do nothing
                break;

            case "Mounted":
                if (!monster1.CompareTag("Undead") && monster2.CompareTag("Undead") && monster2.type == "Baby")
                {
                    SpawnUndead(9, monster1, monster2);
                    return;
                }
                break;

            default:
                Debug.Log("unknown type");
                break;
        }

        if (monster1.type == monster2.type && monster1.CompareTag(monster2.tag) && monster1.startSize == monster2.startSize)
        {
            MonsterLevelUp(monster1);
            monster2.Delete();
            return;
        }

        if (!lastTry) // if there were no merges, try the other way and see if that merged anything
        {
            Merge(monster2, monster1, true);
        }
        else
        {
            monster1.transform.localScale = monster1.startSize;
            monster2.transform.localScale = monster2.startSize;
            CheckMonstersAtScreenEdge();
        }
    }

    void SpawnUndead(int monsterTypeID, Monster monster1, Monster monster2)
    {
        monster1.Delete();
        monster2.LoseHealth(1);

        GameObject newUndead;
        newUndead = monsterTypeID switch
        {
            0 => Instantiate(gm.chieftains[3]),
            1 => Instantiate(gm.warriors[3]),
            2 => Instantiate(gm.magics[3]),
            3 => Instantiate(gm.beasts[3]),
            4 => Instantiate(gm.birds[3]),
            5 => Instantiate(gm.babies[3]),
            6 => Instantiate(gm.bosses[3]),
            7 => Instantiate(gm.flats[3]),
            8 => Instantiate(gm.bobbles[3]),
            9 => Instantiate(gm.mounted[3]),
            _ => Instantiate(gm.chieftains[3]),
        };
        newUndead.transform.position = new Vector2(monster2.transform.position.x, monster2.transform.position.y);
        CheckMonstersAtScreenEdge();
    }

    private void MonsterLevelUp(Monster monster1)
    {
        monster1.transform.localScale = new Vector3(monster1.startSize.x + 0.5f, monster1.startSize.y + 0.5f, monster1.startSize.z + 0.5f);
        monster1.startSize = monster1.transform.localScale;
        monster1.level++;
        monster1.inventorySize *= 0.75f;
        GameObject newConfetti = Instantiate(confetti);
        newConfetti.transform.position = monster1.transform.position;
        FindObjectOfType<AudioManager>().celebrationAS.PlayOneShot(FindObjectOfType<AudioManager>().celebration);
        CheckMonstersAtScreenEdge();
    }

    public void SpawnRandomBabies(int amount = 1, float x = 0, float y = 0, bool randomizeSpawnPosition = false)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject newBaby = Instantiate(gm.babies[Random.Range(0, gm.babies.Length)]);
            if (randomizeSpawnPosition)
            {
                newBaby.transform.position = new Vector2(Random.Range(-screenSize.x, screenSize.x), Random.Range(-screenSize.y, screenSize.y));
            }
            else
            {
                newBaby.transform.position = new Vector2(x, y);
            }
            CheckMonstersAtScreenEdge();
        }
    }

    public void SpawnChieftain(Monster monster1, float x = 0, float y = 0)
    {
        GameObject newChieftain;
        newChieftain = monster1.tag switch
        {
            "Hypnotic" => Instantiate(gm.chieftains[0]),
            "Robotic" => Instantiate(gm.chieftains[1]),
            "Reptilian" => Instantiate(gm.chieftains[2]),
            "Undead" => Instantiate(gm.chieftains[3]),
            "Porcine" => Instantiate(gm.chieftains[4]),
            "Plated" => Instantiate(gm.chieftains[5]),
            "Cloudy" => Instantiate(gm.chieftains[6]),
            "Farming" => Instantiate(gm.chieftains[7]),
            "Plantlike" => Instantiate(gm.chieftains[8]),
            "Sinister" => Instantiate(gm.chieftains[9]),
            _ => Instantiate(gm.chieftains[0]),
        };
        newChieftain.transform.position = new Vector2(x, y);
        CheckMonstersAtScreenEdge();
    }

    public void SpawnMagic(Monster monster1, float x = 0, float y = 0)
    {
        GameObject newChieftain;
        newChieftain = monster1.tag switch
        {
            "Hypnotic" => Instantiate(gm.magics[0]),
            "Robotic" => Instantiate(gm.magics[1]),
            "Reptilian" => Instantiate(gm.magics[2]),
            "Undead" => Instantiate(gm.magics[3]),
            "Porcine" => Instantiate(gm.magics[4]),
            "Plated" => Instantiate(gm.magics[5]),
            "Cloudy" => Instantiate(gm.magics[6]),
            "Farming" => Instantiate(gm.magics[7]),
            "Plantlike" => Instantiate(gm.magics[8]),
            "Sinister" => Instantiate(gm.magics[9]),
            _ => Instantiate(gm.magics[0]),
        };
        newChieftain.transform.position = new Vector2(x, y);
        CheckMonstersAtScreenEdge();
    }

    public void SpawnBeast(Monster monster1, float x = 0, float y = 0)
    {
        GameObject newChieftain;
        newChieftain = monster1.tag switch
        {
            "Hypnotic" => Instantiate(gm.beasts[0]),
            "Robotic" => Instantiate(gm.beasts[1]),
            "Reptilian" => Instantiate(gm.beasts[2]),
            "Undead" => Instantiate(gm.beasts[3]),
            "Porcine" => Instantiate(gm.beasts[4]),
            "Plated" => Instantiate(gm.beasts[5]),
            "Cloudy" => Instantiate(gm.beasts[6]),
            "Farming" => Instantiate(gm.beasts[7]),
            "Plantlike" => Instantiate(gm.beasts[8]),
            "Sinister" => Instantiate(gm.beasts[9]),
            _ => Instantiate(gm.beasts[0]),
        };
        newChieftain.transform.position = new Vector2(x, y);
        CheckMonstersAtScreenEdge();
    }

    public void SpawnBird(Monster monster1, float x = 0, float y = 0)
    {
        GameObject newBird;
        newBird = monster1.tag switch
        {
            "Hypnotic" => Instantiate(gm.birds[0]),
            "Robotic" => Instantiate(gm.birds[1]),
            "Reptilian" => Instantiate(gm.birds[2]),
            "Undead" => Instantiate(gm.birds[3]),
            "Porcine" => Instantiate(gm.birds[4]),
            "Plated" => Instantiate(gm.birds[5]),
            "Cloudy" => Instantiate(gm.birds[6]),
            "Farming" => Instantiate(gm.birds[7]),
            "Plantlike" => Instantiate(gm.birds[8]),
            "Sinister" => Instantiate(gm.birds[9]),
            _ => Instantiate(gm.birds[0]),
        };
        newBird.transform.position = new Vector2(x, y);
        CheckMonstersAtScreenEdge();
    }

    public void SpawnMounted(Monster monster1, float x = 0, float y = 0)
    {
        GameObject newMounted;
        newMounted = monster1.tag switch
        {
            "Hypnotic" => Instantiate(gm.mounted[0]),
            "Robotic" => Instantiate(gm.mounted[1]),
            "Reptilian" => Instantiate(gm.mounted[2]),
            "Undead" => Instantiate(gm.mounted[3]),
            "Porcine" => Instantiate(gm.mounted[4]),
            "Plated" => Instantiate(gm.mounted[5]),
            "Cloudy" => Instantiate(gm.mounted[6]),
            "Farming" => Instantiate(gm.mounted[7]),
            "Plantlike" => Instantiate(gm.mounted[8]),
            "Sinister" => Instantiate(gm.mounted[9]),
            _ => Instantiate(gm.mounted[0]),
        };
        newMounted.transform.position = new Vector2(x, y);
        CheckMonstersAtScreenEdge();
    }
}
