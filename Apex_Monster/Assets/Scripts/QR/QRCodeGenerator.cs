using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using ZXing.QrCode;
using UnityEngine.UI;
using TMPro;

public class QRCodeGenerator : MonoBehaviour
{
    const string QRCODE = "";

    [SerializeField] RawImage _rawImageReceiver;
    [SerializeField] TMP_InputField _textInputField;

    Texture2D _storeEncodedTexture;

    void Start()
    {
        _storeEncodedTexture = new Texture2D(256, 256);
        _rawImageReceiver.texture = _storeEncodedTexture;

        if (PlayerPrefs.HasKey(QRCODE) && PlayerPrefs.GetString(QRCODE) != "")
        {
            _textInputField.text = PlayerPrefs.GetString(QRCODE);
            EncodeTextToQRCode();
            _textInputField.text = "";
        }

        _textInputField.onValueChanged.AddListener(delegate { EncodeTextToQRCode(); });
    }

    // inputField to code
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

    public void OnClickEncode()
    {
        EncodeTextToQRCode();
    }

    void EncodeTextToQRCode()
    {
        string textWrite = string.IsNullOrEmpty(_textInputField.text) ? "You should write something" : _textInputField.text;

        Color32[] _convertPixelsToTexture = Encode(textWrite, _storeEncodedTexture.width, _storeEncodedTexture.height);
        _storeEncodedTexture.SetPixels32(_convertPixelsToTexture);
        _storeEncodedTexture.Apply();

        _rawImageReceiver.texture = _storeEncodedTexture;

        PlayerPrefs.SetString(QRCODE, _textInputField.text);
    }
}
