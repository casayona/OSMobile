using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TotalMoney : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI TotalMoneyText;

    void Awake()
    {
        PlayerProfile.OnMoneyChanged += SetMoney;
        SetMoney(PlayerProfile.Money);
    }

    void Start()
    {
        // BURADAKİ PlayfabManager.instance.GetUserMoney(); satırını sildik!
        // Çünkü PlayfabManager giriş başarılı olunca bunu kendi yapacak.
    }

    void Update()
    {
        if (TotalMoneyText != null && WindowsController.Instance != null)
        {
            TotalMoneyText.gameObject.SetActive(WindowsController.Instance.HasWindowsHistory);
        }
    }

    void OnDestroy()
    {
        PlayerProfile.OnMoneyChanged -= SetMoney;
    }

    void SetMoney(int money)
    {
        if (TotalMoneyText != null)
            TotalMoneyText.text = string.Format("${0}", money);
    }
}