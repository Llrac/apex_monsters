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
    [HideInInspector] public string gameCode;
    const float updateFeedbackRate = 1f;
    int waitingNumber = 1;

    void Start()
    {
        storedEncodedTexture = new Texture2D(256, 256);
        scanField.texture = storedEncodedTexture;
    }

    public void EncodeTextToQRCode(string userID = null)
    {
        gameCode = "";
        if (userID != null)
        {
            gameCode = userID + " : " + Random.Range(100000, 999999);
        }
        else
        {
            Debug.LogError("no code string to encode");
            return;
        }

        waitingNumber = 1;
        CancelInvoke(nameof(OngoingFeedback));
        InvokeRepeating(nameof(OngoingFeedback), 0, updateFeedbackRate);

        Color32[] convertPixelsToTexture = Encode(gameCode, storedEncodedTexture.width, storedEncodedTexture.height);
        storedEncodedTexture.SetPixels32(convertPixelsToTexture);
        storedEncodedTexture.Apply();
        scanField.texture = storedEncodedTexture;

        FindObjectOfType<DatabaseManager>().UpdateGameData(gameCode);
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

    void OngoingFeedback()
    {
        string dots;
        switch (waitingNumber)
        {
            case 1:
                dots = ".";
                break;
            case 2:
                dots = "..";
                break;
            case 3:
                dots = "...";
                break;
            default:
                Debug.Log("missing waitingNumber");
                return;
        }
        feedbackTextField.text = "waiting for a player to join" + dots;
        waitingNumber++;
        if (waitingNumber > 3)
            waitingNumber = 1;
    }
}
