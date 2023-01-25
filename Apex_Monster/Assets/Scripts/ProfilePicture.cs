using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePicture : MonoBehaviour
{
    public GameObject ppChooser;
    [HideInInspector] public Sprite ppSprite;

    private void Start()
    {
        if (ppSprite != null) { return; }
        StartChoosingProfilePicture(); // without this call we cannot find babySprite
        foreach (Image babySprite in FindObjectsOfType<Image>())
        {
            if (babySprite.CompareTag("Plantlike"))
            {
                ppSprite = babySprite.sprite;
                break;
            }
        }
        StopChoosingProfilePicture();
    }

    public void StartChoosingProfilePicture()
    {
        ppChooser.SetActive(true);
    }

    public void ChooseThisAsProfilePicture(GameObject newProfilePicture)
    {
        GameObject spriteObject = null;
        foreach (Transform child in transform)
        {
            if (child.name == "Sprite")
            {
                spriteObject = child.gameObject;
            }
        }
        spriteObject.GetComponent<Image>().sprite = newProfilePicture.GetComponent<Image>().sprite;
        ppSprite = spriteObject.GetComponent<Image>().sprite;

        StopChoosingProfilePicture();
    }

    public void StopChoosingProfilePicture()
    {
        ppChooser.SetActive(false);
    }
}
