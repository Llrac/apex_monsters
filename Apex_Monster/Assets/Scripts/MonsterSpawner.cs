using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Particles")]
    [SerializeField] GameObject windGust;
    public GameObject confetti;
    public GameObject darkConfetti;

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
    }

    #region Screen
    public void UpdateMonsterPositions()
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

        RectTransform safeArea = null;
        foreach (Image imageObject in FindObjectsOfType<Image>())
        {
            if (imageObject.name == "Background")
            {
                safeArea = imageObject.gameObject.GetComponent<RectTransform>();
            }
        }

        screenSize = new(safeArea.rect.width, safeArea.rect.height);
        screenSize = Camera.main.ScreenToWorldPoint(screenSize);

        // TODO: Dots should spawn in SafeArea's corners and screenOffset from SafeArea's edges.
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
        
        UpdateMonsterPositions();

        if (!windGust) { return; }

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
        if ((monster1.CompareTag("Porcine") && monster1.level != 1 && monster2.level == 1 && monster2.type != "Chieftain" && monster2.type != "Baby") ||
            (monster2.CompareTag("Porcine") && monster2.level != 2 && monster1.level == 1 && monster1.type != "Chieftain" && monster1.type != "Baby"))
        {
            SpawnBoss(monster2, monster2.transform.position.x, monster2.transform.position.y);
            monster1.Delete();
            monster2.Delete();
            return;
        }

        switch (monster1.type)
        {
            case "Chieftain":
                if (!monster1.CompareTag("Undead") && monster2.CompareTag("Undead") && monster2.type == "Baby" && monster2.startSize == new Vector3(1, 1, 1))
                {
                    SpawnUndead(0, monster1, monster2);
                    return;
                }
                else if (monster1.CompareTag("Farming") && monster2.CompareTag("Plantlike"))
                {
                    SpawnRandomBabies(3, monster2.transform.position.x, monster2.transform.position.y, false, true);
                    monster2.Delete();
                    return;
                }
                break;

            case "Warrior":
                if (monster2.type == "Warrior" && monster2.CompareTag(monster1.tag))
                {
                    SpawnChieftain(monster2, monster2.transform.position.x, monster2.transform.position.y);
                    monster1.Delete();
                    monster2.Delete();
                    return;
                }
                else if (!monster1.CompareTag("Undead") && monster2.CompareTag("Undead") && monster2.type == "Baby" && monster2.startSize == new Vector3(1, 1, 1))
                {
                    SpawnUndead(1, monster1, monster2);
                    return;
                }
                else if (monster1.CompareTag("Farming") && monster2.CompareTag("Plantlike"))
                {
                    SpawnRandomBabies(2, monster2.transform.position.x, monster2.transform.position.y, false, true);
                    monster2.Delete();
                    return;
                }
                break;

            case "Magic":
                if (!monster1.CompareTag("Undead") && monster2.CompareTag("Undead") && monster2.type == "Baby" && monster2.startSize == new Vector3(1, 1, 1))
                {
                    SpawnUndead(2, monster1, monster2);
                    return;
                }
                else if (monster1.CompareTag("Farming") && monster2.CompareTag("Plantlike") && monster2.type == "Baby")
                {
                    SpawnRandomBabies(1, monster2.transform.position.x, monster2.transform.position.y, false, true);
                    monster2.Delete();
                    return;
                }
                break;

            case "Beast":
                if (!monster1.CompareTag("Undead") && monster2.CompareTag("Undead") && monster2.type == "Baby" && monster2.startSize == new Vector3(1, 1, 1))
                {
                    SpawnUndead(3, monster1, monster2);
                    return;
                }
                else if (monster1.CompareTag("Farming") && monster2.CompareTag("Plantlike") && monster2.type == "Baby")
                {
                    SpawnRandomBabies(1, monster2.transform.position.x, monster2.transform.position.y, false, true);
                    monster2.Delete();
                    return;
                }
                break;

            case "Bird":
                if (!monster1.CompareTag("Undead") && monster2.CompareTag("Undead") && monster2.type == "Baby" && monster2.startSize == new Vector3(1, 1, 1))
                {
                    SpawnUndead(4, monster1, monster2);
                    return;
                }
                else if (monster1.CompareTag("Farming") && monster2.CompareTag("Plantlike") && monster2.type == "Baby")
                {
                    SpawnRandomBabies(1, monster2.transform.position.x, monster2.transform.position.y, false, true);
                    monster2.Delete();
                    return;
                }
                break;

            case "Baby":
                if (monster2.CompareTag("Sinister") && !monster1.CompareTag("Undead") && monster1.type == "Baby" && monster1.startSize == monster2.startSize)
                {
                    SpawnWarrior(monster1, monster2.transform.position.x, monster2.transform.position.y);
                    monster1.Delete();
                    monster2.Delete();
                    return;
                }
                else if (monster1.CompareTag("Sinister") && !monster2.CompareTag("Undead") && monster2.type == "Baby" && monster1.startSize == monster2.startSize)
                {
                    SpawnWarrior(monster2, monster2.transform.position.x, monster2.transform.position.y);
                    monster1.Delete();
                    monster2.Delete();
                    return;
                }

                else if (monster1.CompareTag("Porcine") && !monster2.CompareTag("Undead") && monster2.type == "Baby" && monster1.startSize == monster2.startSize)
                {
                    if (monster2.CompareTag("Porcine") && monster1.startSize == monster2.startSize)
                    {
                        MonsterLevelUp(monster1, monster2);
                        return;
                    }
                    SpawnBeast(monster2, monster2.transform.position.x, monster2.transform.position.y);
                    monster1.Delete();
                    monster2.Delete();
                    return;
                }
                else if (monster1.CompareTag("Robotic") && !monster2.CompareTag("Undead") && monster2.type == "Baby" && monster1.startSize == monster2.startSize)
                {
                    SpawnBobble(monster2, monster2.transform.position.x, monster2.transform.position.y);
                    monster1.Delete();
                    monster2.Delete();
                    return;
                }
                else if (monster1.CompareTag("Reptilian") && !monster2.CompareTag("Undead") && monster2.type == "Baby" && monster1.startSize == monster2.startSize)
                {
                    SpawnFlat(monster2, monster2.transform.position.x, monster2.transform.position.y);
                    monster1.Delete();
                    monster2.Delete();
                    return;
                }
                else if (monster1.CompareTag("Cloudy") && !monster2.CompareTag("Undead") && monster2.type == "Baby" && monster1.startSize == monster2.startSize)
                {
                    SpawnMagic(monster2, monster2.transform.position.x, monster2.transform.position.y);
                    monster1.Delete();
                    monster2.Delete();
                    return;
                }
                else if (monster1.CompareTag("Farming") && !monster2.CompareTag("Undead") && monster2.type == "Baby" && monster1.startSize == monster2.startSize)
                {
                    SpawnBird(monster2, monster2.transform.position.x, monster2.transform.position.y);
                    monster1.Delete();
                    monster2.Delete();
                    return;
                }
                break;

            case "Boss":
                if (!monster1.CompareTag("Undead") && monster2.CompareTag("Undead") && monster2.type == "Baby" && monster2.startSize == new Vector3(1, 1, 1))
                {
                    SpawnUndead(6, monster1, monster2);
                    return;
                }
                else if (monster1.CompareTag("Farming") && monster2.CompareTag("Plantlike") && monster2.type == "Baby")
                {
                    SpawnRandomBabies(1, monster2.transform.position.x, monster2.transform.position.y, false, true);
                    monster2.Delete();
                    return;
                }
                break;

            case "Flat":
                if (!monster1.CompareTag("Undead") && monster2.CompareTag("Undead") && monster2.type == "Baby" && monster2.startSize == new Vector3(1, 1, 1))
                {
                    SpawnUndead(7, monster1, monster2);
                    return;
                }
                else if (monster1.CompareTag("Farming") && monster2.CompareTag("Plantlike") && monster2.type == "Baby")
                {
                    SpawnRandomBabies(1, monster2.transform.position.x, monster2.transform.position.y, false, true);
                    monster2.Delete();
                    return;
                }
                break;

            case "Bobble":
                if (!monster1.CompareTag("Undead") && monster2.CompareTag("Undead") && monster2.type == "Baby" && monster2.startSize == new Vector3(1, 1, 1))
                {
                    SpawnUndead(8, monster1, monster2);
                    return;
                }
                else if (monster1.CompareTag("Robotic") && monster2.CompareTag("Robotic") && monster2.type == "Baby")
                {
                    MonsterLevelUp(monster1, monster2, true);
                    return;
                }
                else if (monster1.CompareTag("Farming") && monster2.CompareTag("Plantlike") && monster2.type == "Baby")
                {
                    SpawnRandomBabies(1, monster2.transform.position.x, monster2.transform.position.y, false, true);
                    monster2.Delete();
                    return;
                }
                break;

            case "Mounted":
                if (!monster1.CompareTag("Undead") && monster2.CompareTag("Undead") && monster2.type == "Baby" && monster2.startSize == new Vector3(1, 1, 1))
                {
                    SpawnUndead(9, monster1, monster2);
                    return;
                }
                else if (monster1.CompareTag("Farming") && monster2.CompareTag("Plantlike") && monster2.type == "Baby")
                {
                    SpawnRandomBabies(1, monster2.transform.position.x, monster2.transform.position.y, false, true);
                    monster2.Delete();
                    return;
                }
                break;

            default:
                Debug.Log("unknown type");
                break;
        }

        if (monster1.type == monster2.type && monster1.CompareTag(monster2.tag) && monster1.startSize == monster2.startSize && monster1.type != "Baby")
        {
            MonsterLevelUp(monster1, monster2);
            return;
        }

        if (!lastTry) // if there were no merges, try the other way and see if that merged anything
        {
            Merge(monster2, monster1, true);
        }
        else
        {
            if ((monster1.CompareTag("Plated") && !monster2.CompareTag("Undead") && monster2.type == "Baby" && monster1.startSize == monster2.startSize) ||
                (monster2.CompareTag("Plated") && !monster1.CompareTag("Undead") && monster1.type == "Baby" && monster1.startSize == monster2.startSize))
            {
                SpawnMounted(monster2, monster2.transform.position.x, monster2.transform.position.y);
                monster1.Delete();
                monster2.Delete();
                return;
            }
            monster1.transform.localScale = monster1.startSize;
            monster2.transform.localScale = monster2.startSize;
            UpdateMonsterPositions();
        }
    }

    public void SpawnRandomBabies(int amount = 1, float x = 0, float y = 0, bool randomizeSpawnPosition = false, bool farmed = false)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject newBaby = Instantiate(gm.babies[Random.Range(0, gm.babies.Length)]);
            if (farmed && newBaby.CompareTag("Plantlike"))
            {
                Destroy(newBaby);
                i--;
                continue;
            }
            
            if (randomizeSpawnPosition)
            {
                newBaby.transform.position = new Vector2(Random.Range(-screenSize.x, screenSize.x), Random.Range(-screenSize.y, screenSize.y));
            }
            else
            {
                newBaby.transform.position = new Vector2(x, y);
            }
            UpdateMonsterPositions();
        }
    }

    void MonsterLevelUp(Monster monster1, Monster monster2, bool roboticUpgrade = false)
    {
        monster2.Delete();
        if (roboticUpgrade)
        {
            monster1.transform.localScale = new Vector3(monster1.startSize.x + 0.25f, monster1.startSize.y + 0.25f, monster1.startSize.z + 0.25f);
            monster1.level++;
            monster1.inventorySize *= 0.9375f;
        }
        else
        {
            monster1.transform.localScale = new Vector3(monster1.startSize.x + 0.5f, monster1.startSize.y + 0.5f, monster1.startSize.z + 0.5f);
            monster1.level++;
            monster1.inventorySize *= 0.75f;
        }
        monster1.startSize = monster1.transform.localScale;
        Celebrate(monster1.gameObject, monster2.gameObject);
        UpdateMonsterPositions();
    }

    void Celebrate(GameObject monster, GameObject infectorMonster = null)
    {
        if (monster.CompareTag("Undead") && infectorMonster != null && infectorMonster.CompareTag("Undead"))
        {
            FindObjectOfType<AudioManager>().PlayCelebrate(monster.transform.position.x, monster.transform.position.y, true);
        }
        else
        {
            FindObjectOfType<AudioManager>().PlayCelebrate(monster.transform.position.x, monster.transform.position.y);
        }
    }

    #region Spawn
    public void SpawnChieftain(Monster monster, float x = 0, float y = 0)
    {
        GameObject newChieftain;
        newChieftain = monster.tag switch
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
        Celebrate(newChieftain, newChieftain);
        UpdateMonsterPositions();
    }

    public void SpawnWarrior(Monster monster, float x = 0, float y = 0)
    {
        GameObject newWarrior;
        newWarrior = monster.tag switch
        {
            "Hypnotic" => Instantiate(gm.warriors[0]),
            "Robotic" => Instantiate(gm.warriors[1]),
            "Reptilian" => Instantiate(gm.warriors[2]),
            "Undead" => Instantiate(gm.warriors[3]),
            "Porcine" => Instantiate(gm.warriors[4]),
            "Plated" => Instantiate(gm.warriors[5]),
            "Cloudy" => Instantiate(gm.warriors[6]),
            "Farming" => Instantiate(gm.warriors[7]),
            "Plantlike" => Instantiate(gm.warriors[8]),
            "Sinister" => Instantiate(gm.warriors[9]),
            _ => Instantiate(gm.warriors[0]),
        };
        newWarrior.transform.position = new Vector2(x, y);
        Celebrate(newWarrior, newWarrior);
        UpdateMonsterPositions();
    }

    public void SpawnMagic(Monster monster, float x = 0, float y = 0)
    {
        GameObject newMagic;
        newMagic = monster.tag switch
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
        newMagic.transform.position = new Vector2(x, y);
        Celebrate(newMagic, newMagic);
        UpdateMonsterPositions();
    }

    public void SpawnBeast(Monster monster, float x = 0, float y = 0)
    {
        GameObject newBeast;
        newBeast = monster.tag switch
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
        newBeast.transform.position = new Vector2(x, y);
        Celebrate(newBeast, newBeast);
        UpdateMonsterPositions();
    }

    public void SpawnBird(Monster monster, float x = 0, float y = 0)
    {
        GameObject newBird;
        newBird = monster.tag switch
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
        Celebrate(newBird, newBird);
        UpdateMonsterPositions();
    }

    public void SpawnBoss(Monster monster, float x = 0, float y = 0)
    {
        GameObject newBoss;
        newBoss = monster.tag switch
        {
            "Hypnotic" => Instantiate(gm.bosses[0]),
            "Robotic" => Instantiate(gm.bosses[1]),
            "Reptilian" => Instantiate(gm.bosses[2]),
            "Undead" => Instantiate(gm.bosses[3]),
            "Porcine" => Instantiate(gm.bosses[4]),
            "Plated" => Instantiate(gm.bosses[5]),
            "Cloudy" => Instantiate(gm.bosses[6]),
            "Farming" => Instantiate(gm.bosses[7]),
            "Plantlike" => Instantiate(gm.bosses[8]),
            "Sinister" => Instantiate(gm.bosses[9]),
            _ => Instantiate(gm.bosses[0]),
        };
        newBoss.transform.position = new Vector2(x, y);
        Celebrate(newBoss, newBoss);
        UpdateMonsterPositions();
    }

    public void SpawnFlat(Monster monster, float x = 0, float y = 0)
    {
        GameObject newFlat;
        newFlat = monster.tag switch
        {
            "Hypnotic" => Instantiate(gm.flats[0]),
            "Robotic" => Instantiate(gm.flats[1]),
            "Reptilian" => Instantiate(gm.flats[2]),
            "Undead" => Instantiate(gm.flats[3]),
            "Porcine" => Instantiate(gm.flats[4]),
            "Plated" => Instantiate(gm.flats[5]),
            "Cloudy" => Instantiate(gm.flats[6]),
            "Farming" => Instantiate(gm.flats[7]),
            "Plantlike" => Instantiate(gm.flats[8]),
            "Sinister" => Instantiate(gm.flats[9]),
            _ => Instantiate(gm.flats[0]),
        };
        newFlat.transform.position = new Vector2(x, y);
        Celebrate(newFlat, newFlat);
        UpdateMonsterPositions();
    }

    public void SpawnBobble(Monster monster, float x = 0, float y = 0)
    {
        GameObject newBobble;
        newBobble = monster.tag switch
        {
            "Hypnotic" => Instantiate(gm.bobbles[0]),
            "Robotic" => Instantiate(gm.bobbles[1]),
            "Reptilian" => Instantiate(gm.bobbles[2]),
            "Undead" => Instantiate(gm.bobbles[3]),
            "Porcine" => Instantiate(gm.bobbles[4]),
            "Plated" => Instantiate(gm.bobbles[5]),
            "Cloudy" => Instantiate(gm.bobbles[6]),
            "Farming" => Instantiate(gm.bobbles[7]),
            "Plantlike" => Instantiate(gm.bobbles[8]),
            "Sinister" => Instantiate(gm.bobbles[9]),
            _ => Instantiate(gm.bobbles[0]),
        };
        newBobble.transform.position = new Vector2(x, y);
        Celebrate(newBobble, newBobble);
        UpdateMonsterPositions();
    }

    public void SpawnMounted(Monster monster, float x = 0, float y = 0)
    {
        GameObject newMounted;
        newMounted = monster.tag switch
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
        Celebrate(newMounted, newMounted);
        UpdateMonsterPositions();
    }

    void SpawnUndead(int monsterTypeID, Monster monster1, Monster monster2)
    {
        monster1.Delete();
        monster2.Delete();

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
        newUndead.GetComponent<Monster>().startSize = monster1.startSize;
        newUndead.transform.localScale = newUndead.GetComponent<Monster>().startSize;
        newUndead.transform.position = new Vector2(monster2.transform.position.x, monster2.transform.position.y);
        Celebrate(newUndead, monster2.gameObject);
        UpdateMonsterPositions();
    }
    #endregion
}
