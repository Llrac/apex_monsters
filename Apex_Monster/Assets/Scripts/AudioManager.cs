using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [HideInInspector] public bool sceneNavigated = false;

    [HideInInspector] public AudioSource backgroundMusicAS;
    [HideInInspector] public AudioSource battleMusicAS;
    [HideInInspector] public AudioSource sfxAS;
    [HideInInspector] public AudioSource celebrationAS;

    [Header("Background Music")]
    public AudioClip dropletsOfDew = null;
    public AudioClip shadesOfOrange = null;
    public AudioClip anAutumn = null;

    [Header("Battle Music")]
    public AudioClip undeniable = null;
    public AudioClip tuffEnough = null;

    [Header("SFX")]
    public AudioClip mergePop1 = null;
    public AudioClip mergePop2 = null;
    public AudioClip celebrate = null;
    public bool playCelebration = false;

    [HideInInspector] public float sfxDelay = 0.6f;
    AudioClip nextPopSFX = null;

    public float celebrateDelay = 1f;
    float celebrateTimer = 10;

    int randomizedBackgroundOST;
    List<AudioClip> backgroundOSTs = new();

    void Start()
    {
        nextPopSFX = mergePop1;
        sfxDelay = 0.6f;

        foreach (Transform child in transform)
        {
            if (child.gameObject.GetComponent<AudioSource>() != null)
            {
                if (child.name == "Background_Music")
                {
                    backgroundMusicAS = child.gameObject.GetComponent<AudioSource>();
                    PlayRandomBackgroundMusic();
                }
                else if (child.name == "Battle_Music")
                {
                    battleMusicAS = child.gameObject.GetComponent<AudioSource>();
                    if (undeniable != null)
                    {
                        battleMusicAS.clip = undeniable;
                        battleMusicAS.Play();
                    }
                }
                else if (child.name == "SFX")
                {
                    sfxAS = child.gameObject.GetComponent<AudioSource>();
                }
                else if (child.name == "Celebration")
                {
                    celebrationAS = child.gameObject.GetComponent<AudioSource>();
                }
            }
        }
    }

    void Update()
    {
        if (!backgroundMusicAS.isPlaying)
        {
            PlayRandomBackgroundMusic();
        }
        celebrateTimer += Time.deltaTime;

        if (playCelebration)
        {
            PlayCelebrate();
            playCelebration = false;
        }
    }
    
    void PlayRandomBackgroundMusic()
    {
        if (backgroundOSTs.Count > 0)
        {
            PlayRemainingMusic();
            return;
        }
        if (dropletsOfDew)
            backgroundOSTs.Add(dropletsOfDew);
        if (shadesOfOrange)
            backgroundOSTs.Add(shadesOfOrange);
        if (anAutumn)
            backgroundOSTs.Add(anAutumn);
        PlayRemainingMusic();
    }

    void PlayRemainingMusic()
    {
        randomizedBackgroundOST = Random.Range(0, backgroundOSTs.Count - 1);

        backgroundMusicAS.clip = backgroundOSTs[randomizedBackgroundOST];
        backgroundMusicAS.Play();

        backgroundOSTs.RemoveAt(randomizedBackgroundOST);
    }

    public void PlayPopSFX()
    {
        sfxAS.PlayOneShot(nextPopSFX);
        int randomPop = Random.Range(0, 1);
        switch (randomPop)
        {
            case 0:
                nextPopSFX = mergePop1;
                sfxDelay = 0.6f;
                break;
            case 1:
                nextPopSFX = mergePop2;
                sfxDelay = 0.4f;
                break;
            default:
                nextPopSFX = mergePop1;
                sfxDelay = 0.6f;
                break;
        }
    }

    public void PlayCelebrate(float xPos = 0, float yPos = 0, bool darkConfetti = false)
    {
        if (celebrateTimer < celebrateDelay) { return; }
        celebrateTimer = 0;

        GameObject newConfetti;
        if (darkConfetti)
        {
            celebrationAS.pitch = 1.25f;
            newConfetti = Instantiate(FindObjectOfType<GameManager>().darkConfetti);
        }
        else
        {
            celebrationAS.pitch = 1.75f;
            newConfetti = Instantiate(FindObjectOfType<GameManager>().confetti);
        }
        celebrationAS.PlayOneShot(celebrate);
        newConfetti.transform.position = new Vector2(xPos, yPos);
        Destroy(newConfetti, 2);
    }
}
