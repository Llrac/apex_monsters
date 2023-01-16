using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using ZXing.QrCode;
using UnityEngine.UI;
using TMPro;

public class QRCodeGenerator : MonoBehaviour
{
    [SerializeField] RawImage _rawImageReceiver;
    [SerializeField] TMP_InputField _textInputField;

    Texture2D _storeEncodedTexture;

    void Start()
    {
        _storeEncodedTexture = new Texture2D(256, 256);
    }

    // inputField to code
    Color32[] Encode(string textForEncoding, int width, int height)
    {
        BarcodeWriter writer = new BarcodeWriter
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

        Color32[] _convertPixelToTexture = Encode(textWrite, _storeEncodedTexture.width, _storeEncodedTexture.height);
        _storeEncodedTexture.SetPixels32(_convertPixelToTexture);
        _storeEncodedTexture.Apply();

        _rawImageReceiver.texture = _storeEncodedTexture;
    }
}
