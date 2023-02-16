using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ProfilePicture : MonoBehaviour
{
    public string SpriteTag
    {
        get { return PlayerPrefs.GetString(LAST_SPRITETAG); }
        set { PlayerPrefs.SetString(LAST_SPRITETAG, value); }
    }
    [HideInInspector] public string spriteTag;
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
            foreach (Image babySprite in FindObjectsOfType<Image>().Where(a => a.CompareTag(PlayerPrefs.GetString(LAST_SPRITETAG))))
            {
                ppSprite = babySprite.sprite;
                spriteTag = PlayerPrefs.GetString(LAST_SPRITETAG);
                break;
            }
        }
        else
        {
            foreach (Image babySprite in FindObjectsOfType<Image>().Where(a => a.CompareTag("Plantlike")))
            {
                ppSprite = babySprite.sprite;
                spriteTag = "Plantlike";
                break;
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
        spriteTag = PlayerPrefs.GetString(LAST_SPRITETAG);

        StopChoosingProfilePicture();

        FindObjectOfType<DatabaseManager>().UpdateUserData();
    }

    public void StopChoosingProfilePicture()
    {
        ppChooser.SetActive(false);
    }
}
