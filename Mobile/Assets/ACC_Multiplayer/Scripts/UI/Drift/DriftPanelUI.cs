using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Pun;

/// <summary>
/// Sadece UI üzerindeki görsel mantık.
/// Drift puanı bilgilerini içerir.
/// </summary>
public class DriftPanelUI : MonoBehaviour
{
    [SerializeField] int UpdateFrameCount = 3;
    [SerializeField] TextMeshProUGUI TotalRaceTimeText;
    [SerializeField] TextMeshProUGUI LapText;
    [SerializeField] TextMeshProUGUI TotalScoreText;
    [SerializeField] TextMeshProUGUI BestScoreText;
    [SerializeField] TextMeshProUGUI CurrentScoreText;
    [SerializeField] TextMeshProUGUI MultiplierScoreText;
    [SerializeField] GameObject WrongDirectionObject;
    [SerializeField] Image DriftTimeImage;
    [SerializeField] Image MultiplierTimeImage;
    [SerializeField] GameObject InGameStatistics;
    [SerializeField] DriftEndGameStatisticsUI EndGameStatistics;

    [Header("Combo Text Ayarlari")]
    [SerializeField] TextMeshProUGUI ComboFeedbackText; // Sağda çıkacak "Awesome" texti (Inspector'dan ata)
    [SerializeField] float ComboTextShowDuration = 1.5f; // Ekranda kalma ve silinme süresi

    int CurrentFrame;
    int _previousMultiplier = 1; // Çarpan artışını takip etmek için
    Coroutine _comboFadeCoroutine; // Animasyon çakışmalarını önlemek için

    DriftRaceEntity DriftRaceEntity;
    GameController GameController { get { return GameController.Instance; } }
    CarStatisticsDriftRegime PlayerStatistics { get { return DriftRaceEntity.PlayerDriftStatistics; } }

    private void Start()
    {
        DriftRaceEntity = GameController.RaceEntity as DriftRaceEntity;
        if (DriftRaceEntity == null)
        {
            Debug.LogError("[DriftPanelUI] RaceEntity is not DriftRaceEntity");
            enabled = false;
        }

        EndGameStatistics.Init();
        InGameStatistics.SetActive(true);
        GameController.OnEndGameAction += OnEndGame;

        // Başlangıçta Combo textini gizle
        if (ComboFeedbackText != null)
        {
            ComboFeedbackText.gameObject.SetActive(false);
        }

        // Başlangıçta Çarpan (x) textini de gizle
        MultiplierScoreText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (PlayerStatistics == null)
        {
            return;
        }

        WrongDirectionObject.SetActive(PlayerStatistics.IsWrongDirection);
        DriftTimeImage.fillAmount = PlayerStatistics.DriftTimeProcent;

        if (PlayerStatistics.CurrentMultiplier == B.GameSettings.DriftRegimeSettings.MaxMultiplier)
        {
            MultiplierTimeImage.fillAmount = 1;
        }
        else
        {
            MultiplierTimeImage.fillAmount = PlayerStatistics.MultiplierProcent;
        }

        if (CurrentFrame >= UpdateFrameCount)
        {
            UpdateStatistics();
            CurrentFrame = 0;
        }
        else
        {
            CurrentFrame++;
        }
    }

    void UpdateStatistics()
    {
        LapText.text = string.Format("{0}/{1}", PlayerStatistics.CurrentLap, PlayerStatistics.LapsCount);
        TotalScoreText.text = PlayerStatistics.TotalScore.ToString("########0");
        BestScoreText.text = PlayerStatistics.BestScore.ToString("########0");
        CurrentScoreText.text = PlayerStatistics.CurrentScore.ToString("########0");
        MultiplierScoreText.text = PlayerStatistics.CurrentMultiplier.ToString("x#");
        TotalRaceTimeText.text = PlayerStatistics.TotalRaceTime.ToStringTime();

        // 1. ÖZELLİK: Drift yapılıyorsa (CurrentScore 0 değilse) çarpan görünür olsun, yapılmıyorsa kapansın.
        bool isDrifting = !Mathf.Approximately(0, PlayerStatistics.CurrentScore);
        MultiplierScoreText.gameObject.SetActive(isDrifting);

        // 2. ÖZELLİK: Çarpan (x) değeri arttığında Combo Feedback ver
        int currentMulti = PlayerStatistics.CurrentMultiplier;
        if (currentMulti > _previousMultiplier && isDrifting)
        {
            TriggerComboFeedback(currentMulti);
        }

        // Eğer drift biterse çarpan hafızasını sıfırla ki sonraki driftte tekrar yazılar çıksın
        if (!isDrifting)
        {
            _previousMultiplier = 1;
        }
        else
        {
            _previousMultiplier = currentMulti;
        }
    }

    void TriggerComboFeedback(int multiplier)
    {
        if (ComboFeedbackText == null) return;

        string comboWord = "";

        // Çarpan değerine göre çıkacak yazılar
        switch (multiplier)
        {
            case 2: comboWord = "Good!"; break;
            case 3: comboWord = "Great!"; break;
            case 4: comboWord = "Awesome!"; break;
            case 5: comboWord = "Big Drift!"; break;
            case 6: comboWord = "Insane!"; break;
            default:
                if (multiplier > 6) comboWord = "Godlike!";
                break;
        }

        // Eğer atanacak bir kelime varsa ekranda göster ve kaybolma efektini başlat
        if (!string.IsNullOrEmpty(comboWord))
        {
            ComboFeedbackText.text = comboWord;
            ComboFeedbackText.gameObject.SetActive(true);

            if (_comboFadeCoroutine != null)
            {
                StopCoroutine(_comboFadeCoroutine);
            }
            _comboFadeCoroutine = StartCoroutine(FadeOutComboTextRoutine());
        }
    }

    // 3. ÖZELLİK: Combo textinin yavaşça silinerek kaybolması (Fade-Out)
    IEnumerator FadeOutComboTextRoutine()
    {
        // Önce texti tam görünür (Opak) yap
        Color textColor = ComboFeedbackText.color;
        textColor.a = 1f;
        ComboFeedbackText.color = textColor;

        // Sürenin yarısı kadar ekranda net bir şekilde bekle
        yield return new WaitForSeconds(ComboTextShowDuration * 0.5f);

        // Kalan sürede yavaşça saydamlaştır
        float fadeDuration = ComboTextShowDuration * 0.5f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            textColor.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            ComboFeedbackText.color = textColor;
            yield return null;
        }

        // Tamamen kaybolunca objeyi kapat
        ComboFeedbackText.gameObject.SetActive(false);
    }

    void OnEndGame()
    {
        InGameStatistics.SetActive(false);
    }

    void OnDestroy()
    {
        if (GameController.Instance != null)
        {
            GameController.OnEndGameAction -= OnEndGame;
        }
    }
}