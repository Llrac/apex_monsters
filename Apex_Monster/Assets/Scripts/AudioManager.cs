using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [HideInInspector] public AudioSource backgroundMusicAS;
    [HideInInspector] public AudioSource battleMusicAS;
    [HideInInspector] public AudioSource sfxAS;
    [HideInInspector] public AudioSource celebrationAS;

    [Header("Background Music")]
    public AudioClip dropletsOfDew = null;
    public AudioClip shadesOfOrange = null;

    [Header("Battle Music")]
    public AudioClip undeniable = null;
    public AudioClip tuffEnough = null;

    [Header("SFX")]
    public AudioClip mergePop1 = null;
    public AudioClip mergePop2 = null;
    public AudioClip celebration = null;
    public bool playCelebration = false;

    [HideInInspector] public float sfxDelay = 0.6f;
    AudioClip nextPopSFX = null;

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
                    if (shadesOfOrange != null)
                    {
                        backgroundMusicAS.clip = shadesOfOrange;
                        backgroundMusicAS.Play();
                    }
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
        if (playCelebration)
        {
            celebrationAS.PlayOneShot(celebration);
            playCelebration = false;
        }
    }

    public void GetNextPopSFX()
    {
        sfxAS.PlayOneShot(nextPopSFX);
        int randomPop = Random.Range(0, 2);
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
}
