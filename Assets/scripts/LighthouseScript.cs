using UnityEngine;
using System.Collections;

public interface IDamageable
{
    void TakeDamage(float amount);
}

public class LighthouseScript : MonoBehaviour, IDamageable
{
    public float health = 100f;
    public float shield = 100f;

    private Renderer _renderer;
    private Color _originalEmission;

    public static LighthouseScript Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Repair(int amount){
        health += amount;
        if(health > 100){
            health = 100;
        }
    }


    void Start()
    {
        _renderer = GetComponent<Renderer>();

      
        if (_renderer != null)
        {
            _renderer.material.EnableKeyword("_EMISSION");
            _originalEmission = _renderer.material.GetColor("_EmissionColor");
        }
    }

   public void TakeDamage(float amount)
    {
        if (shield > 0)
        {
            // shield takes less demage by half.
            shield -= amount / 2;
            return;
        }
        health -= amount;

        
        if (_renderer != null)
        {
            StopAllCoroutines();
            StartCoroutine(GlowRed());
        }

        if (health <= 0)
        {
            health = 0;
            die();
        }
            
           
    }

    void die()
    {
        Destroy(gameObject);
        GameMaster.Instance.EndGame();
    }

    IEnumerator GlowRed()
    {
        _renderer.material.SetColor("_EmissionColor", Color.red * 5f);

        yield return new WaitForSeconds(0.1f);

        _renderer.material.SetColor("_EmissionColor", _originalEmission);
    }
}
