using UnityEngine;
using UnityEngine.UI;
using TMPro; // Eğer TextMeshPro kullanıyorsan bunu aç, kullanmıyorsan kapatabilirsin

public class CarStateUI : MonoBehaviour
{
    [Header("General Settings")]
    [Tooltip("Metinlerin kaç saniyede bir yenileneceği (Performans için)")]
    public float textRefreshRate = 0.1f;
    private float _textTimer;

    [Header("Speedometer (Needle/Rotation)")]
    public RectTransform needleArea;
    public float minNeedleAngle = 0f;
    public float maxNeedleAngle = -270f;

    [Header("Tachometer (Fill & Gradient)")]
    public Image fillArea;
    [Range(0f, 1f)] public float maxFillAmount = 0.75f;
    public Gradient rpmGradient;
    public Color topRpmColor = Color.red;
    [Tooltip("RPM sona dayandığında yanıp sönme hızı")]
    public float flashSpeed = 20f;

    [Header("Text Elements")]
    public Text speedText; // Normal UI Text
    public Text currentGearText;

    // GameController üzerinden arabaya erişim
    private CarController SelectedCar { get { return GameController.PlayerCar; } }

    private void Update()
    {
        // Eğer sahnede araba yoksa hata vermemesi için kontrol
        if (SelectedCar == null) return;

        // 1. GÖRSEL GÜNCELLEMELER (Her karede pürüzsüz çalışmalı)
        UpdateVisuals();

        // 2. METİN GÜNCELLEMELERİ (Belirli aralıklarla çalışarak performans kazandırır)
        _textTimer += Time.deltaTime;
        if (_textTimer >= textRefreshRate)
        {
            UpdateTextElements();
            _textTimer = 0;
        }
    }

    void UpdateVisuals()
    {
        // RPM Oranı hesapla (0.0 ile 1.0 arası)
        float rpmPercent = Mathf.Clamp01(SelectedCar.EngineRPM / SelectedCar.GetMaxRPM);

        // --- İğne Dönüşü ---
        if (needleArea != null)
        {
            float targetAngle = Mathf.Lerp(minNeedleAngle, maxNeedleAngle, rpmPercent);
            // UI'da rotasyon için en güvenli yol localEulerAngles kullanmaktır
            needleArea.localEulerAngles = new Vector3(0, 0, targetAngle);
        }

        // --- Doluluk Oranı (Fill Amount) ---
        if (fillArea != null)
        {
            fillArea.fillAmount = rpmPercent * maxFillAmount;

            // --- Renk ve Yanıp Sönme Efekti ---
            if (rpmPercent >= 0.98f)
            {
                // RPM sona dayandıysa yanıp sönme efekti (Speedometer2'deki top rpm mantığı)
                float flash = Mathf.PingPong(Time.time * flashSpeed, 1);
                fillArea.color = Color.Lerp(rpmGradient.Evaluate(1f), topRpmColor, flash);
            }
            else if (rpmGradient != null)
            {
                fillArea.color = rpmGradient.Evaluate(rpmPercent);
            }
        }
    }

    void UpdateTextElements()
    {
        // Hız Metni (086 gibi 3 haneli formatta)
        if (speedText != null)
        {
            speedText.text = SelectedCar.SpeedInHour.ToString("000");
        }

        // Vites Metni
        if (currentGearText != null)
        {
            int gear = SelectedCar.CurrentGear;

            // Vites mantığı (Örn: 0 ise Geri, 1 ise Boş vb. yapabilirsin)
            if (gear == 0) currentGearText.text = "R";
            else if (gear == 1) currentGearText.text = "N";
            else currentGearText.text = (gear - 1).ToString(); // Arabanın vites mantığına göre düzenle
        }
    }
}