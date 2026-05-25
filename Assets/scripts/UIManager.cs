using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Script refferences")]
    public Player playerScript;
    public LighthouseScript lighthouseScript;
    public GameMaster gameMaster;

    [Header("UI")]
    public Slider playerHealthBar;
    public Slider playerShieldBar;
    public Slider lighthouseHealthBar;
    public Slider lighthouseShieldBar;
    public TextMeshProUGUI timeText;

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

        lighthouseShieldBar.maxValue = lighthouseScript.health; 
        lighthouseShieldBar.value = lighthouseScript.health;


    }
    }

    void Update()
    {
        UpdateStats();
        UpdateTimeDisplay();
    }

   void UpdateStats()
    {
        if (playerScript != null){
            playerHealthBar.value = playerScript.health;
            playerShieldBar.value = playerScript.shield;
        }
            
        else{
            playerHealthBar.value = 0;
            playerShieldBar.value = 0;
       
        }
         
        if (lighthouseScript != null){
            lighthouseHealthBar.value = lighthouseScript.health;
            lighthouseShieldBar.value = lighthouseScript.shield;
        }
        else{
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

            string phaseName = gameMaster.isDay ? "DO NOCY" : "DO DNIA";

            timeText.text = $"{phaseName}: {minutes}:{seconds:00}";

            if (timeLeft < 10f)
                timeText.color = Color.red;
            else
                timeText.color = Color.white;
        }
    }
}