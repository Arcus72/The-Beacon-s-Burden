using UnityEngine;
using UnityEngine.InputSystem;

public class CheatCodeManager : MonoBehaviour
{
    [SerializeField] private string dayCheat = "ccsetday";
    [SerializeField] private string nightCheat = "ccsetnight";
    [SerializeField] private string killMonstersCheat = "cckill";
    [SerializeField] private string freeShopCheat = "ccfree";
    [SerializeField] private string instaKillCheat = "ccinstakill";

    public AudioSource activationSound;
    

    private string inputBuffer = "";
    private const int MaxBufferLength = 25;

    private void OnEnable()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput += OnTextInput;
    }

    private void OnDisable()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= OnTextInput;
    }

    private void OnTextInput(char c)
    {
        inputBuffer += char.ToLower(c);

        if (inputBuffer.Length > MaxBufferLength)
            inputBuffer = inputBuffer.Substring(inputBuffer.Length - MaxBufferLength);
       
        CheckCheats();
    }

    private void CheckCheats()
    {
        if (inputBuffer.Contains(dayCheat))
        {
            ActivateDayCheat();
            ClearBuffer(); 
        }
        else if (inputBuffer.Contains(nightCheat))
        {
            ActivateNightCheat();
            ClearBuffer();
        }
         else if (inputBuffer.Contains(killMonstersCheat))
        {
            ActivateKillMonstersCheat();
            ClearBuffer();
        }
         else if (inputBuffer.Contains(freeShopCheat))
        {
            ActivateFreeShopCheat();
            ClearBuffer();
        }
        else if (inputBuffer.Contains(instaKillCheat))
        {
            ActivateInstaKillCheat();
            ClearBuffer();
        }
    }

    private void ClearBuffer()
    {
        activationSound.Play();
        inputBuffer = "";
    }

    // --- Your Cheat Functions Here ---

    private void ActivateDayCheat()
    {
        Debug.Log("Cheat Activated: Day time");
        GameMaster.Instance.setDay();
    }

    private void ActivateNightCheat()
    {
        Debug.Log("Cheat Activated: Night time");
        GameMaster.Instance.setNight();
    }

     private void ActivateKillMonstersCheat()
    {
        Debug.Log("Cheat Activated: All monsters killed");
        GameMaster.Instance.ClearAllMonsters();
    }

    private void ActivateFreeShopCheat()
    {
        Debug.Log("Cheat Activated: free shop");
        PlayerInventory.Instance.ToogleFreeShop();
    }

    private void ActivateInstaKillCheat(){
        Debug.Log("Cheat Activated: Insta kill");
        Player.Instance.TakeDamage(200);
        Player.Instance.TakeDamage(200);
    }
}
