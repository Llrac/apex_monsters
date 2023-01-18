using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using UnityEngine.UI;
using TMPro;

public class QRCodeScanner : MonoBehaviour
{
    [SerializeField] RawImage _rawImageBackground;
    [SerializeField] AspectRatioFitter _aspectRatioFitter;
    [SerializeField] TextMeshProUGUI _textOut;
    [SerializeField] RectTransform _scanZone;

    bool _isCamAvailable;
    WebCamTexture _cameraTexture;

    void Start()
    {
        InvokeRepeating(nameof(Scan), 0.5f, 0.5f);
        InvokeRepeating(nameof(UpdateCameraRender), 0.5f, 0.5f);
    }

    void LookForCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            _isCamAvailable = false;
            return;
        }

        for (int i = 0; i < devices.Length; i++)
        {
            if (!devices[i].isFrontFacing)
            {
                _cameraTexture = new WebCamTexture(devices[i].name, (int)_scanZone.rect.width, (int)_scanZone.rect.height);
            }
        }

        if (_cameraTexture != null)
        {
            _cameraTexture.Play();
            _rawImageBackground.texture = _cameraTexture;
            _isCamAvailable = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_cameraTexture == null)
            LookForCamera();
    }

    void UpdateCameraRender()
    {
        if (!_isCamAvailable)
        {
            return;
        }
        float ratio = (float)_cameraTexture.width / (float)_cameraTexture.height;
        _aspectRatioFitter.aspectRatio = ratio;

        int orientation = -_cameraTexture.videoRotationAngle;
        _rawImageBackground.rectTransform.localEulerAngles = new Vector3(0, 0, orientation);
    }

    public void OnClickScan()
    {
        //Scan();
    }

    void Scan()
    {
        try
        {
            IBarcodeReader barcodeReader = new BarcodeReader();
            Result result = barcodeReader.Decode(_cameraTexture.GetPixels32(), _cameraTexture.width, _cameraTexture.height);
            if (result != null)
            {
                _textOut.text = result.Text;
            }
            else
            {
                _textOut.text = "FAILED TO READ QR CODE";
            }
        }
        catch
        {
            _textOut.text = "FAILED IN TRY";
        }
    }
}
