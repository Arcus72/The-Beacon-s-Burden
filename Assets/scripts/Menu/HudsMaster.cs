using UnityEngine;
using UnityEngine.InputSystem;

public class HudsMaster : MonoBehaviour
{
    public static HudsMaster Instance;

    [Header("Cameras")]
    public GameObject menuCameraRig;
    public GameObject gameplayCamera;

    [Header("UI Canvas")]
    public GameObject menuCanvas;
    public GameObject playerCanvas;
    public GameObject deathCanvas;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Force the cursor to be visible and free to move when the menu loads
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Swiching UI
        if (menuCameraRig != null) menuCameraRig.SetActive(true);
        if (gameplayCamera != null) gameplayCamera.SetActive(false);
        if (deathCanvas != null) deathCanvas.SetActive(false);

        // Swiching Camera
        if (menuCanvas != null) menuCanvas.SetActive(true);
        if (playerCanvas != null) playerCanvas.SetActive(false);
        
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current[Key.Escape].wasPressedThisFrame)
        {
            ExitGame();
        }

        if (menuCanvas != null && menuCanvas.activeInHierarchy)
        {
            if (!Cursor.visible || Cursor.lockState != CursorLockMode.None)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    public void StartGame()
    {
        // Canvas
        if (menuCanvas != null) menuCanvas.SetActive(false);
        if (playerCanvas != null) playerCanvas.SetActive(true);
        if (deathCanvas != null) deathCanvas.SetActive(false);

        // Cameras
        if (menuCameraRig != null) menuCameraRig.SetActive(false);
        if (gameplayCamera != null) gameplayCamera.SetActive(true);

        // Cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Zerowanie licznika czasu w GameMasterze
        if (GameMaster.Instance != null)
        {
            GameMaster.Instance.currentDay = 1;
            GameMaster.Instance.setNight();
            GameMaster.Instance.ClearAllMonsters();
        }

        // Wymuszenie natychmiastowego pokazania napisu NIGHT 1 na �rodku ekranu przez UIManager
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.StartGameResetNotification();
        }

        // Setting game
        Player.Instance.Restart();
        LighthouseScript.Instance.Restart();
        PlayerInventory.Instance.RestartInventory();
        WeaponManager.Instance.Restart();
    }

    public void showDeathScreen()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Cameras
        if (menuCameraRig != null) menuCameraRig.SetActive(true);
        if (gameplayCamera != null) gameplayCamera.SetActive(false);

        // Canvas
        if (menuCanvas != null) menuCanvas.SetActive(false);
        if (playerCanvas != null) playerCanvas.SetActive(false);
        if (deathCanvas != null) deathCanvas.SetActive(true);
    }

    public void ExitGame()
    {
        Debug.Log("Exit Button Clicked");
        Application.Quit();
    }
}