using UnityEngine;
using System.Collections;


public class PlayerCamera : MonoBehaviour
{

    private Vector3 _originalPos;

    void OnEnable()
    {
        _originalPos = transform.localPosition;
    }

    public void Shake(float duration = 0.2f, float magnitude = 0.2f)
    {
        StopAllCoroutines();
        StartCoroutine(ProcessShake(duration, magnitude));
    }

    IEnumerator ProcessShake(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
           
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(_originalPos.x + x, _originalPos.y + y, _originalPos.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

      
        transform.localPosition = _originalPos;
    }
}
