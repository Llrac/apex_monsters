using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using ZXing.QrCode;
using UnityEngine.UI;
using TMPro;

public class QRCodeGenerator : MonoBehaviour
{
    const string LAST_QRCODE = "code";

    [SerializeField] TextMeshProUGUI feedbackTextField;
    [SerializeField] RawImage scanField;

    Texture2D storedEncodedTexture;

    void Start()
    {
        storedEncodedTexture = new Texture2D(256, 256);
        scanField.texture = storedEncodedTexture;

        if (PlayerPrefs.HasKey(LAST_QRCODE) && PlayerPrefs.GetString(LAST_QRCODE) != "code")
        {
            EncodeTextToQRCode(PlayerPrefs.GetString(LAST_QRCODE));
        }
    }

    public void EncodeTextToQRCode(string userID = null)
    {
        string writeText;

        if (userID != null)
        {
            writeText = userID;
        }
        else
        {
            writeText = Random.Range(100000, 999999).ToString();
        }

        feedbackTextField.text = writeText;

        Color32[] convertPixelsToTexture = Encode(writeText, storedEncodedTexture.width, storedEncodedTexture.height);
        storedEncodedTexture.SetPixels32(convertPixelsToTexture);
        storedEncodedTexture.Apply();

        scanField.texture = storedEncodedTexture;

        PlayerPrefs.SetString(LAST_QRCODE, writeText);
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
