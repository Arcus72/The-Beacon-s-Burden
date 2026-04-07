using UnityEngine;
using UnityEngine.UI;
using TMPro; // Dla tekstu

public class UIManager : MonoBehaviour
{
    [Header("Script refferences")]
    public Player playerScript;
    public LighthouseScript lighthouseScript;
    public GameMaster gameMaster;

    [Header("UI")]
    public Slider playerHealthBar;
    public Slider lighthouseHealthBar;
    public TextMeshProUGUI timeText;

    void Start()
    {

     if (playerScript != null) 
    {
        playerHealthBar.maxValue = playerScript.health; 
        playerHealthBar.value = playerScript.health;
    }
    
    if (lighthouseScript != null) 
    {
        lighthouseHealthBar.maxValue = lighthouseScript.health; 
        lighthouseHealthBar.value = lighthouseScript.health;
    }
    }

    void Update()
    {
        UpdateStats();
        UpdateTimeDisplay();
    }

   void UpdateStats()
    {
        if (playerScript != null)
        {
            playerHealthBar.value = playerScript.health;
        }
        else
        {
            playerHealthBar.value = 0;
        }


        if (lighthouseScript != null)
        {
            lighthouseHealthBar.value = lighthouseScript.health;
        }
        else
        {
            lighthouseHealthBar.value = 0;
        }
    }

    void UpdateTimeDisplay()
    {
        if (gameMaster != null)
        {
            float timeLeft = gameMaster.GetTimeLeft();

            int minutes = Mathf.FloorToInt(timeLeft / 60);
            int seconds = Mathf.FloorToInt(timeLeft % 60);

            string phaseName = gameMaster.isDay ? "DO NOCY" : "DO DNIA";

            timeText.text = $"{phaseName}: {minutes}:{seconds:00}";

            if (timeLeft < 10f)
                timeText.color = Color.red;
            else
                timeText.color = Color.white;
        }
    }
}