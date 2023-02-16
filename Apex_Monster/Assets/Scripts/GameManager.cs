using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [HideInInspector] public bool sceneNavigated = false;

    public AnimationCurve positionDuringMergeCurve;

    public int universalSortingOrderID;
    public bool enableDragGlow = false;

    public GameObject[] chieftains = new GameObject[10];
    public GameObject[] warriors = new GameObject[10];
    public GameObject[] magics = new GameObject[10];
    public GameObject[] beasts = new GameObject[10];
    public GameObject[] birds = new GameObject[10];
    public GameObject[] babies = new GameObject[10];
    public GameObject[] bosses = new GameObject[10];
    public GameObject[] flats = new GameObject[10];
    public GameObject[] bobbles = new GameObject[10];
    public GameObject[] mounted = new GameObject[10];

    [Header("Particles")]
    [SerializeField] GameObject windGust;
    public GameObject confetti;
    public GameObject darkConfetti;

    [Header("Screen Settings")]
    [SerializeField] GameObject screenDot;
    public float inventoryScreenOffset = 1.55f;
    public float screenOffset = 0.75f;
    readonly List<GameObject> screenDots = new();
    [HideInInspector] public Vector2 screenSize;

    void Start()
    {
        universalSortingOrderID = 10;
        Application.targetFrameRate = 60;
    }

    public void UpdateScreen()
    {
        foreach (GameObject dot in screenDots)
        {
            Destroy(dot);
        }

        //UpdateDots();

        foreach (Monster monster in FindObjectsOfType<Monster>())
        {
            if (monster.transform.position.x < -screenSize.x + screenOffset || monster.transform.position.x > screenSize.x - screenOffset ||
                monster.transform.position.y < -screenSize.y + inventoryScreenOffset || monster.transform.position.y > screenSize.y - screenOffset)
            {
                RepositionMonster(monster);
            }
        }
    }

    void UpdateDots()
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

        UpdateScreen();

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
}