using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // Sahne geçiþlerini kontrol etmek için eklendi

public class PlayfabManager : MonoBehaviour
{
    public static PlayfabManager instance;

    [Header("Leaderboard UI Settings")]
    public GameObject rowPrefab;
    public Transform rowsParent;

    [Header("Currency Settings")]
    public string currencyCode = "GD";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Sahne her deðiþtiðinde OnSceneLoaded fonksiyonunu çalýþtýr
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Sahne deðiþtiðinde çalýþan fonksiyon
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Eðer menü sahnesine döndüysen referanslarý otomatik bulmaya çalýþalým
        // "Content" ismini Hierarchy'deki Scroll View içindeki Content objesiyle ayný yapmalýsýn
        if (rowsParent == null)
        {
            GameObject foundContent = GameObject.Find("Content");
            if (foundContent != null) rowsParent = foundContent.transform;
        }

        // Menüye girince otomatik listeyi tazele
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            GetLeaderboard();
            GetUserMoney();
        }
    }

    private void Start()
    {
        Login();
    }

    // --- GÝRÝÞ VE BAÐLANTI ---
    void Login()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnError);
    }

    void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("PlayFab Giriþ baþarýlý!");
        GetUserMoney();
        GetLeaderboard();
    }

    // --- PARA SÝSTEMÝ ---
    public void GetUserMoney()
    {
        if (!PlayFabClientAPI.IsClientLoggedIn()) return;

        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), result =>
        {
            if (result.VirtualCurrency.ContainsKey(currencyCode))
            {
                PlayerProfile.Money = result.VirtualCurrency[currencyCode];
                Debug.Log("Para güncellendi: " + PlayerProfile.Money);
            }
        }, OnError);
    }

    public void AddMoneyToCloud(int amount)
    {
        if (amount <= 0) return;
        var request = new AddUserVirtualCurrencyRequest { VirtualCurrency = currencyCode, Amount = amount };
        PlayFabClientAPI.AddUserVirtualCurrency(request, result => {
            PlayerProfile.Money = result.Balance;
        }, OnError);
    }

    // --- ÝSÝM VE SKOR ---
    public void UpdateDisplayName(string name)
    {
        var request = new UpdateUserTitleDisplayNameRequest { DisplayName = name };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, res => Invoke("GetLeaderboard", 1.5f), OnError);
    }

    public void SendLeaderboard(int score)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate> { new StatisticUpdate { StatisticName = "DriftScore", Value = score } }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, res => Invoke("GetLeaderboard", 1.5f), OnError);
    }

    // --- LÝDERLÝK TABLOSU ---
    public void GetLeaderboard()
    {
        // Sahne deðiþtiðinde rowsParent null kalmýþ olabilir, tekrar kontrol et
        if (rowsParent == null)
        {
            GameObject foundContent = GameObject.Find("Content");
            if (foundContent != null) rowsParent = foundContent.transform;
        }

        var request = new GetLeaderboardRequest
        {
            StatisticName = "DriftScore",
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(request, OnGetLeaderboardResult, OnError);
    }

    void OnGetLeaderboardResult(GetLeaderboardResult result)
    {
        if (rowsParent == null || rowPrefab == null) return;

        // Eski listeyi güvenli temizle (Asset silme hatasýný önlemek için)
        foreach (Transform child in rowsParent)
        {
            if (child.gameObject.scene.name != null) // Sadece sahnede olanlarý sil
            {
                Destroy(child.gameObject);
            }
        }

        // Yeni listeyi oluþtur
        foreach (var item in result.Leaderboard)
        {
            GameObject newRow = Instantiate(rowPrefab, rowsParent);
            TMP_Text[] texts = newRow.GetComponentsInChildren<TMP_Text>();

            if (texts.Length >= 3)
            {
                texts[0].text = (item.Position + 1).ToString();
                texts[1].text = string.IsNullOrEmpty(item.DisplayName) ? "Adsýz Oyuncu" : item.DisplayName;
                texts[2].text = item.StatValue.ToString();
            }
        }
    }

    void OnError(PlayFabError error)
    {
        Debug.LogError("PlayFab Hatasý: " + error.GenerateErrorReport());
    }
}