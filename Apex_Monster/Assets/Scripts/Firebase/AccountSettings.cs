using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using Firebase;
using Firebase.Extensions;

public class AccountSettings : MonoBehaviour
{
    const string LAST_EMAIL = "";
    const string LAST_PASSWORD = "";

    public Sprite[] sprites = new Sprite[3];

    public TMP_InputField email;
    public TMP_InputField password;
    public TextMeshProUGUI feedbackText;
    public Button playButton;

    FirebaseAuth auth;

    // Start is called before the first frame update
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
                Debug.LogError(task.Exception);

            auth = FirebaseAuth.DefaultInstance;

            if (PlayerPrefs.HasKey(LAST_EMAIL) && PlayerPrefs.GetString(LAST_EMAIL) != "" &&
                PlayerPrefs.HasKey(LAST_PASSWORD) && PlayerPrefs.GetString(LAST_PASSWORD) != "")
            {
                email.text = PlayerPrefs.GetString(LAST_EMAIL);
                password.text = PlayerPrefs.GetString(LAST_PASSWORD);
            }
        });
    }

    public void OnHoverEnter(GameObject button)
    {
        button.GetComponent<Image>().sprite = sprites[1];
    }

    public void OnHoverExit(GameObject button)
    {
        button.GetComponent<Image>().sprite = sprites[0];
    }

    public void OnClick(GameObject button)
    {
        button.GetComponent<Image>().sprite = sprites[2];
    }

    public void SignInButton()
    {
        SignInFirebase(email.text, password.text);
    }

    private void SignInFirebase(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogWarning(task.Exception);
            }
            else
            {
                FirebaseUser newUser = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                  newUser.DisplayName, newUser.UserId);
                feedbackText.text = newUser.Email + "is signed in";

                playButton.interactable = true;
            }
        });
    }

    public void RegisterButton()
    {
        RegisterNewUser(email.text, password.text);
    }

    private void RegisterNewUser(string email, string password)
    {
        Debug.Log("Starting Registration");
        feedbackText.text = "Starting Registration";
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogWarning(task.Exception);
            }
            else
            {
                FirebaseUser newUser = task.Result;
                Debug.LogFormat("User Registerd: {0} ({1})",
                  newUser.DisplayName, newUser.UserId);

                playButton.interactable = true;

                PlayerPrefs.SetString(LAST_EMAIL, email);
                PlayerPrefs.SetString(LAST_PASSWORD, password);
            }
        });
    }

    public void DebugLogIn(int number)
    {
        SignInFirebase("test" + number + "@test.test", "password");
    }

    public void PlayButton()
    {
        Debug.Log("play");
    }
}