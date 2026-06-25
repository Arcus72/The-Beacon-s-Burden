using UnityEngine;
using System.Collections;

public class PlayerCamera : MonoBehaviour
{
    private Vector3 _originalPos;
    private Camera _camera;
    private bool _isShaking = false;

    [Header("Head Bobbing Settings (Subtle)")]
    [Tooltip("Maksymalne wychylenie góra-dó³ (zmniejszone z 0.05 na 0.02)")]
    public float bobbingAmountY = 0.02f;
    [Tooltip("Maksymalne wychylenie lewo-prawo (zmniejszone z 0.03 na 0.01)")]
    public float bobbingAmountX = 0.01f;
    [Tooltip("Szybkoœæ ko³ysania dostosowana do kroków.")]
    public float bobbingSpeedMultiplier = 2.2f;

    [Header("Dynamic FOV Settings (Subtle)")]
    [Tooltip("Podstawowy FOV kamery.")]
    public float defaultFOV = 60f;
    [Tooltip("O ile maksymalnie FOV ma siê zwiêkszyæ podczas sprintu (zmniejszone z 8 na 3.5)")]
    public float maxFOVIncrease = 3.5f;
    [Tooltip("Jak szybko FOV dostosowuje siê do zmian prêdkoœci.")]
    public float fovSmoothSpeed = 4f;

    private float _bobTimer = 0f;

    void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera == null)
        {
            _camera = GetComponentInChildren<Camera>();
        }
    }

    void Start()
    {
        _originalPos = transform.localPosition;

        if (_camera != null && defaultFOV == 0)
        {
            defaultFOV = _camera.fieldOfView;
        }
    }

    void Update()
    {
        HandleHeadBobAndFOV();
    }

    private void HandleHeadBobAndFOV()
    {
        if (Player.Instance == null || _camera == null || _isShaking) return;

        CharacterController playerController = Player.Instance.GetComponent<CharacterController>();
        if (playerController == null) return;

        Vector3 horizontalVelocity = new Vector3(playerController.velocity.x, 0, playerController.velocity.z);
        float speed = horizontalVelocity.magnitude;

        // --- 1. SUBTELNY HEAD BOBBING ---
        if (playerController.isGrounded && speed > 0.5f)
        {
            _bobTimer += Time.deltaTime * speed * bobbingSpeedMultiplier;

            float newX = Mathf.Cos(_bobTimer / 2) * bobbingAmountX;
            float newY = Mathf.Sin(_bobTimer) * bobbingAmountY;

            Vector3 targetBobPos = new Vector3(_originalPos.x + newX, _originalPos.y + newY, _originalPos.z);

            // Bardzo p³ynne przejœcie (wyg³adzanie 8f), by ruch nie szarpa³ oczu
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetBobPos, Time.deltaTime * 8f);
        }
        else
        {
            _bobTimer = 0f;
            transform.localPosition = Vector3.Lerp(transform.localPosition, _originalPos, Time.deltaTime * 6f);
        }

        // --- 2. KINOWY DYNAMIC FOV ---
        float maxSpeed = Player.Instance.runSpeed;
        float speedFactor = Mathf.Clamp01(speed / maxSpeed);

        // Zmiana FOV o ma³¹ wartoœæ daje poczucie dynamiki, ale nie deformuje obrazu na boki
        float targetFOV = defaultFOV + (speedFactor * maxFOVIncrease);
        _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, targetFOV, Time.deltaTime * fovSmoothSpeed);
    }

    // --- POPRAWIONY SYSTEM SHAKEA ---
    public void Shake(float duration = 0.2f, float magnitude = 0.2f)
    {
        StopAllCoroutines();
        StartCoroutine(ProcessShake(duration, magnitude));
    }

    IEnumerator ProcessShake(float duration, float magnitude)
    {
        _isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(_originalPos.x + x, _originalPos.y + y, _originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // P³ynny powrót ze wstrz¹su do pozycji wyjœciowej zamiast natychmiastowego ciêcia
        float returnElapsed = 0f;
        Vector3 shakeEndPos = transform.localPosition;
        while (returnElapsed < 0.1f)
        {
            transform.localPosition = Vector3.Lerp(shakeEndPos, _originalPos, returnElapsed / 0.1f);
            returnElapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = _originalPos;
        _isShaking = false;
    }
}