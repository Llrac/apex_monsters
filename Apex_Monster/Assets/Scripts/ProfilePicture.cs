using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePicture : MonoBehaviour
{
    const string LAST_SPRITETAG = "spritetag";

    public GameObject ppChooser;
    GameObject spriteObject = null;
    [HideInInspector] public Sprite ppSprite;

    void Start()
    {
        foreach (Transform child in transform)
        {
            if (child.name == "Sprite")
            {
                spriteObject = child.gameObject;
            }
        }

        StartChoosingProfilePicture(); // without this call we cannot find babySprite
        if (PlayerPrefs.HasKey(LAST_SPRITETAG) && PlayerPrefs.GetString(LAST_SPRITETAG) != "spritetag")
        {
            foreach (Image babySprite in FindObjectsOfType<Image>())
            {
                if (babySprite.CompareTag(PlayerPrefs.GetString(LAST_SPRITETAG)))
                {
                    ppSprite = babySprite.sprite;
                    break;
                }
            }
        }
        else
        {
            foreach (Image babySprite in FindObjectsOfType<Image>())
            {
                if (babySprite.CompareTag("Plantlike"))
                {
                    ppSprite = babySprite.sprite;
                    break;
                }
            }
        }
        StopChoosingProfilePicture();

        spriteObject.GetComponent<Image>().sprite = ppSprite;
    }

    public void StartChoosingProfilePicture()
    {
        ppChooser.SetActive(true);
    }

    public void ChooseThisAsProfilePicture(GameObject newProfilePicture)
    {
        spriteObject.GetComponent<Image>().sprite = newProfilePicture.GetComponent<Image>().sprite;
        ppSprite = spriteObject.GetComponent<Image>().sprite;

        PlayerPrefs.SetString(LAST_SPRITETAG, newProfilePicture.tag);

        StopChoosingProfilePicture();
    }

    public void StopChoosingProfilePicture()
    {
        ppChooser.SetActive(false);
    }
}
