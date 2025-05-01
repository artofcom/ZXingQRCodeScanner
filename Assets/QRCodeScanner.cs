using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using TMPro;
using static System.Net.Mime.MediaTypeNames;

public class QRCodeScanner : MonoBehaviour
{
  //  public RawImage cameraView;       // 카메라 화면 표시용
    public TMP_Text resultText;           // 결과 출력용
    public UnityEngine.UI.Image previewImage;            // UI Image (Sprite 지원)
  //  public AspectRatioFitter ratioFitter;

    private Texture2D tex;
    private WebCamTexture webcamTexture;
    private BarcodeReader barcodeReader;

    public string QRCodeData => resultText?.text;

    void Start()
    {
        // ZXing 초기화
        barcodeReader = new BarcodeReader();

       // 카메라 초기화
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length > 0)
        {
            string cameraName = devices[0].name;
            webcamTexture = new WebCamTexture(cameraName);
           // cameraView.texture = webcamTexture;
            webcamTexture.Play();
        }
        else
        {
            // Debug.LogWarning("No camera detected.");
            webcamTexture = new WebCamTexture();
            webcamTexture.Play();
        }

        // Texture2D 준비 (Sprite 변환용)
        tex = new Texture2D(//1080, 1920, TextureFormat.RGBA32, false);
            webcamTexture.width, webcamTexture.height, TextureFormat.RGBA32, false);
    }

    void Update()
    {
        if (webcamTexture.isPlaying && webcamTexture.didUpdateThisFrame)
        {
            Color32[] pixelData = webcamTexture.GetPixels32();
            int width = webcamTexture.width;
            int height = webcamTexture.height;

            byte[] rawRGB = ConvertColor32ToByteArray(pixelData, width, height);

            // 카메라 프레임 복사
            tex.SetPixels32(webcamTexture.GetPixels32());
            tex.Apply();
            if (previewImage != null)
            {
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                previewImage.sprite = sprite;
            }

            // ZXing의 RGBLuminanceSource에 맞게 변환
            var luminance = new RGBLuminanceSource(rawRGB, width, height, RGBLuminanceSource.BitmapFormat.RGB32);

            var result = barcodeReader.Decode(luminance);
            if (result != null)
            {
                Debug.Log("✅ QR Code Detected: " + result.Text);
                resultText.text = "✅ QR Code Detected: " + result.Text;
            }

            AdjustImageRotation(); // 회전 및 미러링 보정


            AspectRatioFitter fitter = previewImage.GetComponent<AspectRatioFitter>();
            if (fitter != null)
            {
                float aspect = (float)(webcamTexture.width / webcamTexture.height);
                fitter.aspectRatio = aspect;
            }
        }
    }

    // Color32[] → byte[] (RGB순서) 변환
    private byte[] ConvertColor32ToByteArray(Color32[] colors, int width, int height)
    {
        byte[] bytes = new byte[width * height * 4];
        for (int i = 0; i < colors.Length; i++)
        {
            bytes[i * 4] = colors[i].r;
            bytes[i * 4 + 1] = colors[i].g;
            bytes[i * 4 + 2] = colors[i].b;
            bytes[i * 4 + 3] = colors[i].a;
        }
        return bytes;
    }

    private void AdjustImageRotation()
    {
        if (previewImage == null || webcamTexture == null)
            return;

        // 회전 보정
        float rotation = -webcamTexture.videoRotationAngle;

        // 미러 보정 (전면 카메라일 경우 보통 true)
        bool mirrored = webcamTexture.videoVerticallyMirrored;

        // RectTransform 회전 적용
        previewImage.rectTransform.localEulerAngles = new Vector3(0, 0, rotation);

        // 미러링 처리
        Vector3 scale = previewImage.rectTransform.localScale;
        scale.y = mirrored ? -1 : 1;
        previewImage.rectTransform.localScale = scale;
    }

    void OnDestroy()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }
    }
}
