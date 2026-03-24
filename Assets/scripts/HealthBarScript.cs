using UnityEngine;
using UnityEngine.UI;

public class MonsterHealthBar : MonoBehaviour
{

    public Image _healthbarSprite;
    private float _deafoultBarSize = 1;
    public float currentHealth;
  
   

    public void UpdateHealtBar(float maxHealth, float currentHealth1)
    {
        currentHealth = currentHealth1;
        float newX = currentHealth * _deafoultBarSize / maxHealth;
        _healthbarSprite.rectTransform.sizeDelta = new Vector2(newX, _healthbarSprite.rectTransform.sizeDelta.y);
    }

}
