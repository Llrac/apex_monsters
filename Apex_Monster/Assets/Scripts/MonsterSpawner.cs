using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [HideInInspector] public bool sceneNavigated = false;

    public GameObject monsterCanvas;

    public enum MonsterType
    {
        chieftain,
        warrior,
        magic,
        beast,
        bird,
        baby,
        boss,
        flat,
        bobble,
        mounted
    }

    [Header("Debug: Spawn Monster Baby")]
    [SerializeField] public bool spawnBabies = false;
    [SerializeField] int spawnAmount = 1;
    [SerializeField] bool spawnMonsterID = false;
    [SerializeField] int monsterID = 1;
    GameManager gm;

    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        gm.UpdateScreen();
    }

    void Update()
    {
        // Debugging
        if (spawnBabies)
        {
            SpawnRandomBabies(spawnAmount, 0, 0, true);
            spawnBabies = false;
        }
        if (spawnMonsterID)
        {
            SpawnMonsterID(monsterID);
            spawnMonsterID = false;
        }
    }

    public bool MergeConditions(Monster monster1, Monster monster2, MonsterType transformIntoMonsterType)
    {
        switch (transformIntoMonsterType)
        {
            case MonsterType.chieftain:
                if (monster2.type == "Warrior" && monster2.CompareTag(monster1.tag))
                    return true;
                break;
            case MonsterType.warrior:
                if (monster1.CompareTag("Sinister") && !monster2.CompareTag("Undead") && monster2.type == "Baby" && monster1.startSize == monster2.startSize)
                    return true;
                break;
            case MonsterType.magic:
                if (monster1.CompareTag("Cloudy") && !monster2.CompareTag("Undead") && monster2.type == "Baby" && monster1.startSize == monster2.startSize)
                    return true;
                break;
            case MonsterType.beast:
                if (monster1.CompareTag("Porcine") && !monster2.CompareTag("Undead") && monster2.type == "Baby" && monster1.startSize == monster2.startSize)
                    return true;
                break;
            case MonsterType.bird:
                if (monster1.CompareTag("Farming") && !monster2.CompareTag("Undead") && monster2.type == "Baby" && monster1.startSize == monster2.startSize)
                    return true;
                break;
            case MonsterType.baby:
                break;
            case MonsterType.boss:
                if (monster1.CompareTag("Porcine") && monster1.level == monster2.level + 1 && monster1.type == "Baby" && monster2.type != "Chieftain" && monster2.type != "Boss" && monster2.type != "Mounted" && monster2.type != "Baby")
                    return true;
                break;
            case MonsterType.flat:
                if (monster1.CompareTag("Reptilian") && !monster2.CompareTag("Undead") && monster2.type == "Baby" && monster1.startSize == monster2.startSize)
                    return true;
                break;
            case MonsterType.bobble:
                if (monster1.CompareTag("Robotic") && !monster2.CompareTag("Undead") && monster2.type == "Baby" && monster1.startSize == monster2.startSize)
                    return true;
                break;
            case MonsterType.mounted:
                if (monster1.CompareTag("Cloudy") && monster1.level > monster2.level && monster1.type == "Baby" && monster2.type != "Chieftain" && monster2.type != "Boss" && monster2.type != "Mounted" && monster2.type != "Baby")
                    return true;
                break;
            default:
                break;
        }

        return false;
    }

    public void Merge(Monster monster1, Monster monster2, bool lastTry = false)
    {
        // Spawn Boss
        if (MergeConditions(monster1, monster2, MonsterType.boss))
        {
            SpawnBoss(monster2, monster2.transform.position.x, monster2.transform.position.y);
            monster1.Delete();
            monster2.Delete();
            return;
        }
        else if (MergeConditions(monster2, monster1, MonsterType.boss))
        {
            SpawnBoss(monster1, monster2.transform.position.x, monster2.transform.position.y);
            monster1.Delete();
            monster2.Delete();
            return;
        }
        // Spawn Mounted
        else if (MergeConditions(monster1, monster2, MonsterType.mounted))
        {
            SpawnMounted(monster2, monster2.transform.position.x, monster2.transform.position.y);
            monster1.Delete();
            monster2.Delete();
            return;
        }
        else if (MergeConditions(monster2, monster1, MonsterType.mounted))
        {
            SpawnMounted(monster1, monster2.transform.position.x, monster2.transform.position.y);
            monster1.Delete();
            monster2.Delete();
            return;
        }

        switch (monster1.type)
        {
            case "Chieftain":
                CheckSpecialCriteria(monster1, monster2, 0, 3);
                break;

            case "Warrior":
                if (MergeConditions(monster1, monster2, MonsterType.chieftain))
                {
                    SpawnChieftain(monster2, monster2.transform.position.x, monster2.transform.position.y);
                    monster1.Delete();
                    monster2.Delete();
                    return;
                }
                else
                {
                    CheckSpecialCriteria(monster1, monster2, 1, 2);
                }
                break;

            case "Magic":
                CheckSpecialCriteria(monster1, monster2, 2);
                break;

            case "Beast":
                CheckSpecialCriteria(monster1, monster2, 3);
                break;

            case "Bird":
                CheckSpecialCriteria(monster1, monster2, 4);
                break;

            case "Baby":
                if (MergeConditions(monster2, monster1, MonsterType.warrior))
                {
                    SpawnWarrior(monster1, monster2.transform.position.x, monster2.transform.position.y);
                    monster1.Delete();
                    monster2.Delete();
                    return;
                }
                else if (MergeConditions(monster1, monster2, MonsterType.warrior))
                {
                    SpawnWarrior(monster2, monster2.transform.position.x, monster2.transform.position.y);
                    monster1.Delete();
                    monster2.Delete();
                    return;
                }

                else if (MergeConditions(monster1, monster2, MonsterType.beast))
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
                else if (MergeConditions(monster1, monster2, MonsterType.bobble))
                {
                    SpawnBobble(monster2, monster2.transform.position.x, monster2.transform.position.y);
                    monster1.Delete();
                    monster2.Delete();
                    return;
                }
                else if (MergeConditions(monster1, monster2, MonsterType.flat))
                {
                    SpawnFlat(monster2, monster2.transform.position.x, monster2.transform.position.y);
                    monster1.Delete();
                    monster2.Delete();
                    return;
                }
                else if (MergeConditions(monster1, monster2, MonsterType.magic))
                {
                    if (monster2.CompareTag("Cloudy") && monster1.startSize == monster2.startSize)
                    {
                        MonsterLevelUp(monster1, monster2);
                        return;
                    }
                    SpawnMagic(monster2, monster2.transform.position.x, monster2.transform.position.y);
                    monster1.Delete();
                    monster2.Delete();
                    return;
                }
                else if (MergeConditions(monster1, monster2, MonsterType.bird))
                {
                    SpawnBird(monster2, monster2.transform.position.x, monster2.transform.position.y);
                    monster1.Delete();
                    monster2.Delete();
                    return;
                }
                break;

            case "Boss":
                CheckSpecialCriteria(monster1, monster2, 6);
                break;

            case "Flat":
                CheckSpecialCriteria(monster1, monster2, 7);
                break;

            case "Bobble":
                CheckSpecialCriteria(monster1, monster2, 8);
                break;

            case "Mounted":
                CheckSpecialCriteria(monster1, monster2, 9);
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
    }

    private void CheckSpecialCriteria(Monster monster1, Monster monster2, int monsterType, int spawnBabiesAmount = 1)
    {
        if (!monster1.CompareTag("Undead") && monster2.CompareTag("Undead") && monster2.type == "Baby" && monster2.startSize == new Vector3(1, 1, 1))
        {
            SpawnUndead(monsterType, monster1, monster2);
            return;
        }
        else if (monster1.CompareTag("Robotic") && monster2.CompareTag("Robotic") && monster2.type == "Baby")
        {
            MonsterLevelUp(monster1, monster2, true);
            return;
        }
        else if (monster1.CompareTag("Farming") && monster2.CompareTag("Plantlike"))
        {
            SpawnRandomBabies(spawnBabiesAmount, monster2.transform.position.x, monster2.transform.position.y, false, true);
            monster2.Delete();
            return;
        }
    }

    void SpawnRandomBabies(int amount = 1, float x = 0, float y = 0, bool randomizeSpawnPosition = false, bool farmed = false)
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
                newBaby.transform.position = new Vector2(Random.Range(-gm.screenSize.x, gm.screenSize.x), Random.Range(-gm.screenSize.y, gm.screenSize.y));
            }
            else
            {
                newBaby.transform.position = new Vector2(x, y);
            }
            gm.UpdateScreen();
        }
    }

    static public void SpawnMonsterID(int monsterID, float x = 0, float y = 0, bool randomizeSpawnPosition = false)
    {
        GameManager gm = FindObjectOfType<GameManager>();
        GameObject newMonster;

        newMonster = monsterID switch
        {
            // 01-10    BABIES
            1 => Instantiate(gm.babies[0]),
            2 => Instantiate(gm.babies[1]),
            3 => Instantiate(gm.babies[2]),
            4 => Instantiate(gm.babies[3]),
            5 => Instantiate(gm.babies[4]),
            6 => Instantiate(gm.babies[5]),
            7 => Instantiate(gm.babies[6]),
            8 => Instantiate(gm.babies[7]),
            9 => Instantiate(gm.babies[8]),
            10 => Instantiate(gm.babies[9]),

            // 11-20    BEASTS
            11 => Instantiate(gm.beasts[0]),
            12 => Instantiate(gm.beasts[1]),
            13 => Instantiate(gm.beasts[2]),
            14 => Instantiate(gm.beasts[3]),
            15 => Instantiate(gm.beasts[4]),
            16 => Instantiate(gm.beasts[5]),
            17 => Instantiate(gm.beasts[6]),
            18 => Instantiate(gm.beasts[7]),
            19 => Instantiate(gm.beasts[8]),
            20 => Instantiate(gm.beasts[9]),

            // 21-30    BIRDS
            21 => Instantiate(gm.birds[0]),
            22 => Instantiate(gm.birds[1]),
            23 => Instantiate(gm.birds[2]),
            24 => Instantiate(gm.birds[3]),
            25 => Instantiate(gm.birds[4]),
            26 => Instantiate(gm.birds[5]),
            27 => Instantiate(gm.birds[6]),
            28 => Instantiate(gm.birds[7]),
            29 => Instantiate(gm.birds[8]),
            30 => Instantiate(gm.birds[9]),

            // 31-40    BOBBLES
            31 => Instantiate(gm.bobbles[0]),
            32 => Instantiate(gm.bobbles[1]),
            33 => Instantiate(gm.bobbles[2]),
            34 => Instantiate(gm.bobbles[3]),
            35 => Instantiate(gm.bobbles[4]),
            36 => Instantiate(gm.bobbles[5]),
            37 => Instantiate(gm.bobbles[6]),
            38 => Instantiate(gm.bobbles[7]),
            39 => Instantiate(gm.bobbles[8]),
            40 => Instantiate(gm.bobbles[9]),

            // 41-50    BOSSES
            41 => Instantiate(gm.bosses[0]),
            42 => Instantiate(gm.bosses[1]),
            43 => Instantiate(gm.bosses[2]),
            44 => Instantiate(gm.bosses[3]),
            45 => Instantiate(gm.bosses[4]),
            46 => Instantiate(gm.bosses[5]),
            47 => Instantiate(gm.bosses[6]),
            48 => Instantiate(gm.bosses[7]),
            49 => Instantiate(gm.bosses[8]),
            50 => Instantiate(gm.bosses[9]),

            // 51-60    CHIEFTAINS
            51 => Instantiate(gm.chieftains[0]),
            52 => Instantiate(gm.chieftains[1]),
            53 => Instantiate(gm.chieftains[2]),
            54 => Instantiate(gm.chieftains[3]),
            55 => Instantiate(gm.chieftains[4]),
            56 => Instantiate(gm.chieftains[5]),
            57 => Instantiate(gm.chieftains[6]),
            58 => Instantiate(gm.chieftains[7]),
            59 => Instantiate(gm.chieftains[8]),
            60 => Instantiate(gm.chieftains[9]),

            // 61-70    FLATS
            61 => Instantiate(gm.flats[0]),
            62 => Instantiate(gm.flats[1]),
            63 => Instantiate(gm.flats[2]),
            64 => Instantiate(gm.flats[3]),
            65 => Instantiate(gm.flats[4]),
            66 => Instantiate(gm.flats[5]),
            67 => Instantiate(gm.flats[6]),
            68 => Instantiate(gm.flats[7]),
            69 => Instantiate(gm.flats[8]),
            70 => Instantiate(gm.flats[9]),

            // 71-80    MAGICS
            71 => Instantiate(gm.magics[0]),
            72 => Instantiate(gm.magics[1]),
            73 => Instantiate(gm.magics[2]),
            74 => Instantiate(gm.magics[3]),
            75 => Instantiate(gm.magics[4]),
            76 => Instantiate(gm.magics[5]),
            77 => Instantiate(gm.magics[6]),
            78 => Instantiate(gm.magics[7]),
            79 => Instantiate(gm.magics[8]),
            80 => Instantiate(gm.magics[9]),

            // 81-90    MOUNTED
            81 => Instantiate(gm.mounted[0]),
            82 => Instantiate(gm.mounted[1]),
            83 => Instantiate(gm.mounted[2]),
            84 => Instantiate(gm.mounted[3]),
            85 => Instantiate(gm.mounted[4]),
            86 => Instantiate(gm.mounted[5]),
            87 => Instantiate(gm.mounted[6]),
            88 => Instantiate(gm.mounted[7]),
            89 => Instantiate(gm.mounted[8]),
            90 => Instantiate(gm.mounted[9]),

            // 91-100   WARRIORS
            91 => Instantiate(gm.warriors[0]),
            92 => Instantiate(gm.warriors[1]),
            93 => Instantiate(gm.warriors[2]),
            94 => Instantiate(gm.warriors[3]),
            95 => Instantiate(gm.warriors[4]),
            96 => Instantiate(gm.warriors[5]),
            97 => Instantiate(gm.warriors[6]),
            98 => Instantiate(gm.warriors[7]),
            99 => Instantiate(gm.warriors[8]),
            100 => Instantiate(gm.warriors[9]),

            // missing monsterID
            _ => Instantiate(gm.babies[0]),
        };

        if (randomizeSpawnPosition)
        {
            newMonster.transform.position = new Vector2(Random.Range(-gm.screenSize.x, gm.screenSize.x), Random.Range(-gm.screenSize.y, gm.screenSize.y));
        }
        else
        {
            newMonster.transform.position = new Vector2(x, y);
        }
        newMonster.transform.SetParent(FindObjectOfType<SceneNavigator>().transform);
        gm.UpdateScreen();
    }

    void MonsterLevelUp(Monster monster1, Monster monster2, bool roboticUpgrade = false)
    {
        monster2.Delete();
        if (roboticUpgrade)
        {
            monster1.startSize = new Vector3(monster1.startSize.x + 0.25f, monster1.startSize.y + 0.25f, monster1.startSize.z + 0.25f);
            monster1.inventorySize *= 0.9375f;
        }
        else
        {
            monster1.startSize = new Vector3(monster1.startSize.x + 0.5f, monster1.startSize.y + 0.5f, monster1.startSize.z + 0.5f);
            monster1.level++;
            monster1.inventorySize *= 0.75f;
        }
        monster1.attack += monster2.attack;
        monster1.startHealth += monster2.startHealth;
        monster1.currentHealth = monster1.startHealth;

        monster1.transform.localScale = monster1.startSize;
        monster1.UpdateMonsterCanvas();
        Celebrate(monster1.gameObject, monster2.gameObject);
        gm.UpdateScreen();
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
        UpdateSpawnedMonsterStats(monster, x, y, newChieftain);
        Celebrate(newChieftain, newChieftain);
        gm.UpdateScreen();
    }

    private static void UpdateSpawnedMonsterStats(Monster monster, float x, float y, GameObject newMonster)
    {
        newMonster.GetComponent<Monster>().attack *= monster.level;
        newMonster.GetComponent<Monster>().startHealth *= monster.level;
        newMonster.GetComponent<Monster>().level = monster.level;
        newMonster.transform.localScale = monster.startSize;
        newMonster.transform.position = new Vector2(x, y);
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
        UpdateSpawnedMonsterStats(monster, x, y, newWarrior);
        Celebrate(newWarrior, newWarrior);
        gm.UpdateScreen();
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
        UpdateSpawnedMonsterStats(monster, x, y, newMagic);
        Celebrate(newMagic, newMagic);
        gm.UpdateScreen();
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
        UpdateSpawnedMonsterStats(monster, x, y, newBeast);
        Celebrate(newBeast, newBeast);
        gm.UpdateScreen();
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
        UpdateSpawnedMonsterStats(monster, x, y, newBird);
        Celebrate(newBird, newBird);
        gm.UpdateScreen();
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
        UpdateSpawnedMonsterStats(monster, x, y, newBoss);
        Celebrate(newBoss, newBoss);
        gm.UpdateScreen();
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
        UpdateSpawnedMonsterStats(monster, x, y, newFlat);
        Celebrate(newFlat, newFlat);
        gm.UpdateScreen();
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
        UpdateSpawnedMonsterStats(monster, x, y, newBobble);
        Celebrate(newBobble, newBobble);
        gm.UpdateScreen();
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
        UpdateSpawnedMonsterStats(monster, x, y, newMounted);
        Celebrate(newMounted, newMounted);
        gm.UpdateScreen();
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
        newUndead.GetComponent<Monster>().attack = monster1.attack + 1;
        newUndead.GetComponent<Monster>().startHealth = monster1.startHealth;
        newUndead.GetComponent<Monster>().startSize = monster1.startSize;
        newUndead.GetComponent<Monster>().level = monster1.level;
        newUndead.transform.localScale = monster1.startSize;
        newUndead.transform.position = new Vector2(monster2.transform.position.x, monster2.transform.position.y);
        newUndead.GetComponent<Monster>().UpdateMonsterCanvas();
        Celebrate(newUndead, monster2.gameObject);
        gm.UpdateScreen();
    }
    #endregion
}
