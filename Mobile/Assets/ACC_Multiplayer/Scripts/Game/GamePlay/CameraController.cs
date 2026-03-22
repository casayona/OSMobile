using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine; // Unity 6 (Cinemachine 3) kütüphanesi

/// <summary>
/// Cinemachine 3 (Unity 6) entegreli Move and rotation camera controller
/// </summary>
public class CameraController : Singleton<CameraController>
{
    [SerializeField] List<CameraPreset> CamerasPreset = new List<CameraPreset>();   //Camera presets
    [SerializeField] CarController OverrideCar;

    int ActivePresetIndex = -1;
    public CameraPreset ActivePreset { get; private set; }

    CarController TargetCar { get { return OverrideCar ?? GameController.PlayerCar; } }
    GameController GameController { get { return GameController.Instance; } }

    float SqrMinDistance;
    int CurrentFrame = 0;

    // Cinemachine kameralarının takip edeceği görünmez hedef objesi
    private Transform TargetProxy;

    // The target point is calculated from velocity of car.
    Vector3 m_TargetPoint;
    Vector3 TargetPoint
    {
        get
        {
            if (CurrentFrame != Time.frameCount)
            {
                if (!OverrideCar && GameController == null || TargetCar == null)
                {
                    return TargetProxy != null ? TargetProxy.position : transform.position;
                }
                m_TargetPoint = TargetCar.RB.linearVelocity * ActivePreset.VelocityMultiplier;
                m_TargetPoint += TargetCar.transform.position;

                CurrentFrame = Time.frameCount;
            }
            return m_TargetPoint;
        }
    }

    protected override void AwakeSingleton()
    {
        // Cinemachine için sanal bir takip objesi oluşturuyoruz.
        TargetProxy = new GameObject("Cinemachine_TargetProxy").transform;

        // Bütün Cinemachine kameralarını başlangıçta kapatıyoruz
        CamerasPreset.ForEach(c =>
        {
            if (c.Cam != null)
                c.Cam.gameObject.SetActive(false);
        });

        ActivePresetIndex = GameOptions.ActiveCameraIndex;
        UpdateActiveCamera();
    }

    private IEnumerator Start()
    {
        while (!OverrideCar && (GameController == null || TargetCar == null))
        {
            yield return null;
        }

        // Başlangıç pozisyonunu ve rotasyonunu anında ayarla (Kamera buraya direkt ışınlanacak)
        TargetProxy.position = TargetPoint;
        TargetProxy.rotation = TargetCar.transform.rotation;
    }

    private void Update()
    {
        if (TargetProxy == null) return;

        // Eski kodundaki Lerp işlemini Proxy objesine uyguluyoruz. 
        // Böylece hissiyatı %100 korumuş oluyoruz.
        TargetProxy.position = Vector3.LerpUnclamped(TargetProxy.position, TargetPoint, Time.deltaTime * ActivePreset.SetPositionSpeed);

        if (ActivePreset.EnableRotation)
        {
            // Eski kodundaki Y eksenini sıfırlama uzantısı (ZeroHeight)
            var position = TargetProxy.position.ZeroHeight();
            var target = TargetPoint.ZeroHeight();

            if ((position - target).sqrMagnitude >= SqrMinDistance)
            {
                Quaternion rotation = Quaternion.LookRotation(target - position, Vector3.up);
                TargetProxy.rotation = Quaternion.Lerp(TargetProxy.rotation, rotation, Time.deltaTime * ActivePreset.SetRotationSpeed);
            }
        }

        // Kamera geçiş tuşları
        if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Joystick1Button2))
        {
            SetNextCamera();
        }
    }

    public void SetNextCamera()
    {
        ActivePresetIndex = MathExtentions.LoopClamp(ActivePresetIndex + 1, 0, CamerasPreset.Count);
        GameOptions.ActiveCameraIndex = ActivePresetIndex;
        UpdateActiveCamera();
    }

    public void UpdateActiveCamera()
    {
        // Eski kamerayı kapat
        if (ActivePreset != null && ActivePreset.Cam != null)
        {
            ActivePreset.Cam.gameObject.SetActive(false);
        }

        // Yeni kamerayı aktif et
        ActivePreset = CamerasPreset[ActivePresetIndex];

        if (ActivePreset.Cam != null)
        {
            ActivePreset.Cam.gameObject.SetActive(true);

            // Unity 6'da CinemachineCamera'nın hedefini Proxy yapıyoruz.
            ActivePreset.Cam.Follow = TargetProxy;
            ActivePreset.Cam.LookAt = ActivePreset.EnableRotation ? TargetProxy : null;
        }

        SqrMinDistance = ActivePreset.MinDistanceForRotation * 2;
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(TargetPoint, 1);
            if (TargetProxy != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(TargetProxy.position, 0.8f);
            }
            Gizmos.color = Color.white;
        }
    }

    [System.Serializable]
    public class CameraPreset
    {
        // Unity 6 (Cinemachine 3) sınıf adı CinemachineCamera oldu
        public CinemachineCamera Cam;

        public float SetPositionSpeed = 1;              // Change position speed.
        public float VelocityMultiplier;                // Velocity of car multiplier.

        public bool EnableRotation;
        public float MinDistanceForRotation = 0.1f;     // Min distance for rotation, To avoid uncontrolled rotation.
        public float SetRotationSpeed = 1;              // Change rotation speed.
    }
}