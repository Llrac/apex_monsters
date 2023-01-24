using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using ZXing.QrCode;
using UnityEngine.UI;
using TMPro;

public class QRCodeGenerator : MonoBehaviour
{
    const string LAST_EMAIL = "email";
    const string LAST_PASSWORD = "password";

    [SerializeField] TMP_InputField emailTextField;
    [SerializeField] TMP_InputField passwordTextField;
    [SerializeField] TextMeshProUGUI feedbackTextField;
    [SerializeField] RawImage scanField;

    Texture2D storedEncodedTexture;

    void Start()
    {
        Debug.Log(feedbackTextField.text);
        storedEncodedTexture = new Texture2D(256, 256);
        scanField.texture = storedEncodedTexture;

        if (PlayerPrefs.HasKey(LAST_EMAIL) && PlayerPrefs.GetString(LAST_EMAIL) != "email")
        {
            emailTextField.text = PlayerPrefs.GetString(LAST_EMAIL);
        }
        if (PlayerPrefs.HasKey(LAST_PASSWORD) && PlayerPrefs.GetString(LAST_PASSWORD) != "password")
        {
            passwordTextField.text = PlayerPrefs.GetString(LAST_PASSWORD);
        }
        if (PlayerPrefs.HasKey(LAST_EMAIL) && PlayerPrefs.GetString(LAST_EMAIL) != "email" &&
            PlayerPrefs.HasKey(LAST_PASSWORD) && PlayerPrefs.GetString(LAST_PASSWORD) != "password")
        {
            EncodeNewTextToQRCode();
        }

        emailTextField.onEndEdit.AddListener(delegate { EncodeNewTextToQRCode(); });
        passwordTextField.onEndEdit.AddListener(delegate { EncodeNewTextToQRCode(); });
    }

    void EncodeNewTextToQRCode()
    {
        feedbackTextField.text = PlayerPrefs.GetString(LAST_EMAIL) + " : " + PlayerPrefs.GetString(LAST_PASSWORD);
        EncodeTextToQRCode();
    }

    void EncodeTextToQRCode()
    {
        string writeText = string.IsNullOrEmpty(feedbackTextField.text) ? "You should write something" : feedbackTextField.text;

        Color32[] convertPixelsToTexture = Encode(writeText, storedEncodedTexture.width, storedEncodedTexture.height);
        storedEncodedTexture.SetPixels32(convertPixelsToTexture);
        storedEncodedTexture.Apply();

        scanField.texture = storedEncodedTexture;

        PlayerPrefs.SetString(LAST_EMAIL, emailTextField.text);
        PlayerPrefs.SetString(LAST_PASSWORD, passwordTextField.text);
    }

    Color32[] Encode(string textForEncoding, int width, int height)
    {
        BarcodeWriter writer = new()
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };

        return writer.Write(textForEncoding);
    }
}
