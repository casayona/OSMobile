using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Only visual on UI logic.
/// Speedometer, tachometer and current gear information.
/// </summary>
public class CarStateUI :MonoBehaviour
{

    [Header("General Settings")]
    [SerializeField] int UpdateFrameCount = 3;
    private int _currentFrame;

    [Header("Speedometer (Needle/Rotation)")]
    public RectTransform needleArea;
    public float MaxNeedleAngle = -270f;
    public float MinNeedleAngle = 0f;

    [Header("Tachometer (Fill & Gradient)")]
    public Image fillArea;
    public float MaxFillAmount = 0.75f;
    public Gradient rpmGradient;
    public Color topRpmColor = Color.white;

    [Header("Text Elements")]
    public Text SpeedText;
    public Text CurrentGearText;

    // GameController üzerinden PlayerCar referansını alıyoruz
    private CarController SelectedCar { get { return GameController.PlayerCar; } }

    private void Update()
    {
        if (SelectedCar == null) return;

        // Performans optimizasyonu: Metin güncellemeleri her karede yapılmaz
        if (_currentFrame >= UpdateFrameCount)
        {
            UpdateTextElements();
            _currentFrame = 0;
        }
        else
        {
            _currentFrame++;
        }

        // Görsel iğne ve bar hareketleri pürüzsüzlük için her karede güncellenir
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        // 1. Devir (RPM) Hesaplaması (0 ile 1 arası oran)
        float rpmPercent = Mathf.Clamp01(SelectedCar.EngineRPM / SelectedCar.GetMaxRPM);

        // 2. İğne Dönüşü (Needle Rotation)
        float needleAngle = Mathf.Lerp(MinNeedleAngle, MaxNeedleAngle, rpmPercent);
        if (needleArea != null)
        {
            needleArea.rotation = Quaternion.AngleAxis(needleAngle, Vector3.forward);
        }

        // 3. Fill Bar ve Renk Güncelleme (Gradient)
        if (fillArea != null)
        {
            fillArea.fillAmount = rpmPercent * MaxFillAmount;

            // Eğer RPM sona dayandıysa (Redline) yanıp sönme efekti veya sabit renk
            if (rpmPercent >= 0.98f)
            {
                // Basit bir yanıp sönme efekti ekleyebilirsin, şimdilik topRpmColor verdik
                fillArea.color = topRpmColor;
            }
            else if (rpmGradient != null)
            {
                fillArea.color = rpmGradient.Evaluate(rpmPercent);
            }
        }
    }

    void UpdateTextElements()
    {
        // Hız metni güncelleme
        if (SpeedText != null)
        {
            SpeedText.text = SelectedCar.SpeedInHour.ToString("000");
        }

        // Vites metni güncelleme
        if (CurrentGearText != null)
        {
            // Vites 0 ise "R", 1 ise "N" gibi logicleri buraya ekleyebilirsin
            CurrentGearText.text = SelectedCar.CurrentGear.ToString();
        }
    }
}
