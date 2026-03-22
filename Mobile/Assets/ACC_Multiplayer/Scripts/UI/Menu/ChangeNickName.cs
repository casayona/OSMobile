using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChangeNickName : Singleton<ChangeNickName>
{

    [SerializeField] GameObject Holder;
    [SerializeField] Button ApplyButton;
    [SerializeField] Button CancelButton;
    [SerializeField] TMP_InputField InputField;

    protected override void AwakeSingleton()
    {
        if (!PlayerPrefs.HasKey(C.NickName))
        {
            Show();
        }
        else
        {
            Holder.SetActive(false);
        }

        ApplyButton.onClick.AddListener(OnApplyNickname);
        CancelButton.onClick.AddListener(OnCancel);
    }

    public void Show()
    {
        InputField.text = PlayerProfile.NickName;
        Holder.SetActive(true);
    }

    void OnApplyNickname()
    {
        string newName = InputField.text.Trim(); // Başındaki ve sonundaki gereksiz boşlukları siler

        // PlayFab kuralları: İsim en az 3, en fazla 25 karakter olmalıdır. Aksi halde PlayFab hata verir.
        if (string.IsNullOrEmpty(newName) || newName.Length < 3 || newName.Length > 25)
        {
            Debug.LogWarning("Oyuncu ismi en az 3, en fazla 25 karakter olmalıdır!");
            return; // Şart sağlanmazsa işlemi durdur
        }

        // 1. Oyundaki yerel (PlayerPrefs) sistemine ismi kaydet
        PlayerProfile.NickName = newName;
        Holder.SetActive(false);

        // 2. İsmi PlayFab'e gönder. (PlayFab ismin güncellendiğini onayladığında
        // PlayfabManager içindeki GetLeaderboard() otomatik çalışacak ve ekranı güncelleyecek).
        if (PlayfabManager.instance != null)
        {
            PlayfabManager.instance.UpdateDisplayName(newName);
        }
        else
        {
            Debug.LogError("PlayfabManager sahnede bulunamadı! İsminiz PlayFab'e gönderilemedi.");
        }
    }

    void OnCancel()
    {
        Holder.SetActive(false);
    }
}