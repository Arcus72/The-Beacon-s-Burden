using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Script references")]
    public Player playerScript;
    public LighthouseScript lighthouseScript;
    public GameMaster gameMaster;

    [Header("UI")]
    public Slider playerHealthBar;
    public Slider playerShieldBar;
    public Slider lighthouseHealthBar;
    public Slider lighthouseShieldBar;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI cyclePhaseText;

    [Header("Center Notification Settings")]
    [Tooltip("Przeci¹gnij tutaj tekst na œrodku ekranu (np. CenterNotificationText)")]
    public TextMeshProUGUI centerNotificationText;

    private int _lastDay = -1;
    private bool _lastIsDay = false;
    private Coroutine _fadeCoroutine;

    void Start()
    {
        if (playerScript != null)
        {
            playerHealthBar.maxValue = playerScript.health;
            playerHealthBar.value = playerScript.health;

            playerShieldBar.maxValue = playerScript.shield;
            playerShieldBar.value = playerScript.shield;
        }

        if (lighthouseScript != null)
        {
            lighthouseHealthBar.maxValue = lighthouseScript.health;
            lighthouseHealthBar.value = lighthouseScript.health;

            lighthouseShieldBar.maxValue = lighthouseScript.shield;
            lighthouseShieldBar.value = lighthouseScript.shield;
        }

        // Ukrycie napisu na œrodku i wymuszenie wyœrodkowania tekstu w ramce
        if (centerNotificationText != null)
        {
            centerNotificationText.alignment = TextAlignmentOptions.Center;
            Color c = centerNotificationText.color;
            c.a = 0f;
            centerNotificationText.color = c;
        }

        if (gameMaster != null)
        {
            _lastDay = gameMaster.currentDay;
            _lastIsDay = gameMaster.isDay;
        }
    }

    void Update()
    {
        UpdateStats();
        UpdateTimeDisplay();
        HandleCenterNotificationTrigger();
    }

    void UpdateStats()
    {
        if (playerScript != null)
        {
            playerHealthBar.value = playerScript.health;
            playerShieldBar.value = playerScript.shield;
        }
        else
        {
            playerHealthBar.value = 0;
            playerShieldBar.value = 0;
        }

        if (lighthouseScript != null)
        {
            lighthouseHealthBar.value = lighthouseScript.health;
            lighthouseShieldBar.value = lighthouseScript.shield;
        }
        else
        {
            lighthouseHealthBar.value = 0;
            lighthouseShieldBar.value = 0;
        }
    }

    void UpdateTimeDisplay()
    {
        if (gameMaster != null)
        {
            float timeLeft = gameMaster.GetTimeLeft();

            int minutes = Mathf.FloorToInt(timeLeft / 60);
            int seconds = Mathf.FloorToInt(timeLeft % 60);

            string phaseName = gameMaster.isDay ? "Until Night" : "Until Day";

            timeText.text = $"{phaseName}: {minutes}:{seconds:00}";

            if (timeLeft < 10f)
                timeText.color = Color.red;
            else
                timeText.color = Color.white;

            if (cyclePhaseText != null)
            {
                if (gameMaster.isDay)
                {
                    cyclePhaseText.text = $"Day {gameMaster.currentDay}";
                    cyclePhaseText.color = Color.white;
                }
                else
                {
                    cyclePhaseText.text = $"Night {gameMaster.currentDay}";
                    cyclePhaseText.color = Color.white;
                }
            }
        }
    }

    void HandleCenterNotificationTrigger()
    {
        if (gameMaster == null || centerNotificationText == null) return;

        // Jeœli menu g³ówne jest w³¹czone, nie pozwalamy na automatyczne triggery
        if (HudsMaster.Instance != null && HudsMaster.Instance.menuCanvas != null && HudsMaster.Instance.menuCanvas.activeInHierarchy)
        {
            return;
        }

        if (gameMaster.currentDay != _lastDay || gameMaster.isDay != _lastIsDay)
        {
            _lastDay = gameMaster.currentDay;
            _lastIsDay = gameMaster.isDay;

            string message = gameMaster.isDay ? $"DAY {gameMaster.currentDay}" : $"NIGHT {gameMaster.currentDay}";
            Color targetColor = new Color(1f, 1f, 1f, 0f);

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeNotificationRoutine(message, targetColor));
        }
    }

    // Wywo³ywane rêcznie przez przycisk Start w HudsMasterze, aby wymusiæ bia³y napis startowy
    public void StartGameResetNotification()
    {
        if (gameMaster == null || centerNotificationText == null) return;

        _lastDay = gameMaster.currentDay;
        _lastIsDay = gameMaster.isDay;

        string message = gameMaster.isDay ? $"DAY {gameMaster.currentDay}" : $"NIGHT {gameMaster.currentDay}";
        Color targetColor = new Color(1f, 1f, 1f, 0f);

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeNotificationRoutine(message, targetColor));
    }

    IEnumerator FadeNotificationRoutine(string text, Color baseColor)
    {
        centerNotificationText.text = text;

        // 1. FADE IN (1 sekunda)
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            baseColor.a = Mathf.Clamp01(elapsed / 1f);
            centerNotificationText.color = baseColor;
            yield return null;
        }

        baseColor.a = 1f;
        centerNotificationText.color = baseColor;

        // 2. STAY (2 sekundy)
        yield return new WaitForSeconds(2f);

        // 3. FADE OUT (1 sekunda)
        elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            baseColor.a = Mathf.Clamp01(1f - (elapsed / 1f));
            centerNotificationText.color = baseColor;
            yield return null;
        }

        baseColor.a = 0f;
        centerNotificationText.color = baseColor;
    }
}