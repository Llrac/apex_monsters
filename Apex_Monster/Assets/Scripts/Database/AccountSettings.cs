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
    const string LAST_EMAIL = "email";
    const string LAST_PASSWORD = "password";

    public Sprite[] sprites = new Sprite[3];

    public TMP_InputField email;
    public TMP_InputField password;
    public TextMeshProUGUI feedbackText;
    [SerializeField] RawImage scanField;

    bool showingAccountSettings = true;

    FirebaseAuth auth;

    // Start is called before the first frame update
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
                Debug.LogError(task.Exception);

            auth = FirebaseAuth.DefaultInstance;
            if (PlayerPrefs.HasKey(LAST_EMAIL) && PlayerPrefs.GetString(LAST_EMAIL) != "email" &&
                PlayerPrefs.HasKey(LAST_PASSWORD) && PlayerPrefs.GetString(LAST_PASSWORD) != "password")
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

        if (showingAccountSettings) // hide account settings
        {
            showingAccountSettings = false;

            gameObject.SetActive(false);
        }
        else // show account settings
        {
            showingAccountSettings = true;

            gameObject.SetActive(true);
        }
    }

    public void LoginButton()
    {
        LoginFirebase(email.text, password.text);
        FindObjectOfType<DatabaseManager>().SavePlayerData();
        FindObjectOfType<DatabaseManager>().DebugSavedData();
    }

    private void LoginFirebase(string email, string password)
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
                FindObjectOfType<QRCodeGenerator>().EncodeTextToQRCode(newUser.UserId);
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
                Debug.LogFormat("User registered: {0} ({1})",
                  newUser.DisplayName, newUser.UserId);

                // set scanfield color 100%

                PlayerPrefs.SetString(LAST_EMAIL, email);
                PlayerPrefs.SetString(LAST_PASSWORD, password);
            }
        });
    }

    public void DebugRegister(int number)
    {
        RegisterNewUser("test" + number + "@test.test", "Password" + number);
    }

    public void DebugLogIn(int number)
    {
        LoginFirebase("test" + number + "@test.test", "Password" + number);
    }

    public void DebugRemoveAccount(int number)
    {
        //SignInFirebase("test" + number + "@test.test", "Password" + number);
    }

    public void PlayButton()
    {
        Debug.Log("play");
    }
}