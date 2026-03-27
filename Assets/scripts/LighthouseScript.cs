using UnityEngine;
using System.Collections;

public interface IDamageable
{
    void TakeDamage(float amount);
}

public class LighthouseScript : MonoBehaviour, IDamageable
{
    public float health = 100f;

    private Renderer _renderer;
    private Color _originalEmission;

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
        health -= amount;

        
        if (_renderer != null)
        {
            StopAllCoroutines();
            StartCoroutine(GlowRed());
        }

        if (health <= 0)
            die();
           
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
