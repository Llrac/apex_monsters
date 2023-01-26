using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using ZXing.QrCode;
using UnityEngine.UI;
using TMPro;

public class QRCodeGenerator : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI feedbackTextField;
    [SerializeField] RawImage scanField;

    Texture2D storedEncodedTexture;

    void Start()
    {
        
        storedEncodedTexture = new Texture2D(256, 256);
        scanField.texture = storedEncodedTexture;
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
