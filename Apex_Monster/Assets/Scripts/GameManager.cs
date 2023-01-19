using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
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

    void Start()
    {
        universalSortingOrderID = 10;
        Application.targetFrameRate = 60;
    }
}