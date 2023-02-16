using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;

public class AccountSettings : MonoBehaviour
{
    const string LAST_NAME = "name";
    const string LAST_EMAIL = "email";
    const string LAST_PASSWORD = "password";

    public Sprite[] sprites = new Sprite[3];

    public TMP_InputField username;
    public TMP_InputField email;
    public TMP_InputField password;
    public TextMeshProUGUI feedbackText;
    [SerializeField] RawImage scanField;

    // Start is called before the first frame update
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
                Debug.LogError(task.Exception);

            if (PlayerPrefs.HasKey(LAST_NAME) && PlayerPrefs.GetString(LAST_NAME) != "name" &&
                PlayerPrefs.HasKey(LAST_EMAIL) && PlayerPrefs.GetString(LAST_EMAIL) != "email" &&
                PlayerPrefs.HasKey(LAST_PASSWORD) && PlayerPrefs.GetString(LAST_PASSWORD) != "password")
            {
                username.text = PlayerPrefs.GetString(LAST_NAME);
                email.text = PlayerPrefs.GetString(LAST_EMAIL);
                password.text = PlayerPrefs.GetString(LAST_PASSWORD);
                LoginFirebase(username.text, email.text, password.text);
            }
        });
    }

    #region Debug Toggler
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
        FindObjectOfType<SceneNavigator>().ToggleDebugging();
    }
    #endregion

    public void RegisterButton()
    {
        RegisterNewUser(username.text, email.text, password.text);
    }

    private void RegisterNewUser(string username, string email, string password)
    {
        Debug.Log("registering...");
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        auth.FetchProvidersForEmailAsync(email).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.Log("fetched account: " + email);
            }
        });
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogWarning(task.Exception);
            }
            else
            {
                FirebaseUser newUser = task.Result;
                Debug.Log(username + ", your account has been created");

                PlayerPrefs.SetString(LAST_NAME, username);
                PlayerPrefs.SetString(LAST_EMAIL, email);
                PlayerPrefs.SetString(LAST_PASSWORD, password);

                FindObjectOfType<DatabaseManager>().UpdateUserData();
            }
        });
    }

    public void LoginButton()
    {
        LoginFirebase(username.text, email.text, password.text);
    }

    public void DebugRegister(int number)
    {
        RegisterNewUser("test" + number, "test" + number + "@test.test", "Pswrd" + number);
    }

    public void DebugLogIn(int number)
    {
        LoginFirebase("test" + number, "test" + number + "@test.test", "Pswrd" + number);
    }

    private void LoginFirebase(string username, string email, string password)
    {
        FindObjectOfType<DatabaseManager>().UpdateUserData();

        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogWarning(task.Exception);
            }
            else
            {
                FirebaseUser newUser = task.Result;
                Debug.Log(username + " has logged in");

                PlayerPrefs.SetString(LAST_NAME, username);
                PlayerPrefs.SetString(LAST_EMAIL, email);
                PlayerPrefs.SetString(LAST_PASSWORD, password);

                this.username.text = username;
                this.email.text = email;
                this.password.text = password;

                FindObjectOfType<QRCodeGenerator>().EncodeTextToQRCode(newUser.UserId);
                FindObjectOfType<DatabaseManager>().LoadUserData();
            }
        });
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