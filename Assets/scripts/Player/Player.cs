using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour, IDamageable
{
    public static Player Instance;
    public PlayerCamera playerCamera;

    [Header("Movement Speeds")]
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float crouchSpeed = 3f;

    [Header("Movement Smoothness")]
    [Tooltip("Jak szybko gracz przyspiesza i hamuje na ziemi. Wyższe wartości = ostrzejszy ruch.")]
    public float movementSmoothness = 8f;
    [Tooltip("Mnożnik kontroli w powietrzu (0 = brak kontroli, 1 = pełna kontroli jak na ziemi)")]
    [Range(0f, 1f)] public float airControlFactor = 0.2f;
    [Tooltip("Jak szybko kamera dopasowuje się do kucania.")]
    public float crouchSmoothness = 10f;

    public float jumpPower = 7f;
    public float gravity = 20f;
    public float lookSpeed = 0.1f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;

    [Header("Health")]
    public float health = 100f;
    public float shield = 50f;

    [Header("Sea Level")]
    public float sealevel = 0f;
    private float underwaterTimer = 0f;

    [Header("Footsteps Settings")]
    public AudioSource footstepsAudioSource;
    public AudioClip[] footstepsClips;
    public float walkStepRate = 0.5f;
    public float runStepRate = 0.3f;
    public float crouchStepRate = 0.7f;
    private float stepTimer = 0f;

    // Zmienne do płynnego ruchu i bezwładności
    private Vector3 currentVelocity = Vector3.zero;
    private float currentHeight;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    private Vector3 spawnPoint;

    private bool canMove = true;
    private bool wasRunningWhenJumped = false; // Zapamiętuje, czy gracz biegł w momencie odbicia

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        spawnPoint = transform.position;
        currentHeight = defaultHeight;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Heal(int amount)
    {
        health += amount;
        if (health > 100) health = 100;
    }

    public void RepairShield(int amount)
    {
        shield += amount;
        if (shield > 50) shield = 50;
    }

    public void Restart()
    {
        gameObject.SetActive(true);
        shield = 50;
        health = 100;
        underwaterTimer = 0f;
        characterController.enabled = false;
        transform.position = spawnPoint;
        characterController.enabled = true;
        currentVelocity = Vector3.zero;
    }

    public void TakeDamage(float amount)
    {
        if (playerCamera != null)
            playerCamera.Shake(0.15f, 0.3f);

        if (shield > 0)
        {
            shield -= amount / 2;
            return;
        }

        health -= amount;

        if (health <= 0)
        {
            health = 0;
            die();
        }
    }

    void die()
    {
        gameObject.SetActive(false);
        GameMaster.Instance.EndGame();
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        if (keyboard == null || mouse == null) return;

        // Input kierunkowy
        float moveX = 0;
        float moveY = 0;
        if (keyboard.wKey.isPressed) moveY = 1;
        if (keyboard.sKey.isPressed) moveY = -1;
        if (keyboard.dKey.isPressed) moveX = 1;
        if (keyboard.aKey.isPressed) moveX = -1;

        Vector2 mouseDelta = mouse.delta.ReadValue();

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Logika stanów (Kucanie)
        bool isCrouching = keyboard.ctrlKey.isPressed && canMove;

        bool isRunning = false;
        if (characterController.isGrounded)
        {
            // MODYFIKACJA (Krok 3): Możesz zacząć biec tylko wciskając W (moveY > 0)
            isRunning = keyboard.leftShiftKey.isPressed && !isCrouching && moveY > 0 && canMove;
            wasRunningWhenJumped = isRunning;
        }
        else
        {
            isRunning = wasRunningWhenJumped;
        }

        // Dobieranie docelowej prędkości
        float targetSpeed = walkSpeed;
        if (isCrouching) targetSpeed = crouchSpeed;
        else if (isRunning) targetSpeed = runSpeed;

        float targetSpeedX = canMove ? moveY * targetSpeed : 0;
        float targetSpeedY = canMove ? moveX * targetSpeed : 0;

        Vector3 desiredVelocity = (forward * targetSpeedX) + (right * targetSpeedY);

        // MODYFIKACJA (Krok 5): Jeśli jesteśmy w powietrzu, drastycznie zmniejszamy płynność (smoothness), 
        // przez co postać nie reaguje gwałtownie na WSAD i zachowuje pęd lotu.
        float currentSmoothness = characterController.isGrounded ? movementSmoothness : (movementSmoothness * airControlFactor);

        currentVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, Time.deltaTime * currentSmoothness);

        float previousYVelocity = moveDirection.y;
        moveDirection = currentVelocity;
        moveDirection.y = previousYVelocity;

        // Skok i Grawitacja
        if (keyboard.spaceKey.wasPressedThisFrame && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        else if (moveDirection.y < 0)
        {
            moveDirection.y = -2f;
        }

        // Płynne kucanie
        float targetHeight = isCrouching ? crouchHeight : defaultHeight;
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchSmoothness);
        characterController.height = currentHeight;

        characterController.Move(moveDirection * Time.deltaTime);

        // Obrót kamery
        if (canMove)
        {
            rotationX += -mouseDelta.y * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, mouseDelta.x * lookSpeed, 0);
        }

        HandleFootsteps(isRunning, isCrouching);
        HandleUnderwater();
    }

    void HandleUnderwater()
    {
        if (transform.position.y < sealevel)
        {
            underwaterTimer += Time.deltaTime;

            if (underwaterTimer >= Mathf.Floor(underwaterTimer) && Time.deltaTime > 0)
            {
                float elapsed = underwaterTimer;
                float prev = elapsed - Time.deltaTime;
                if (Mathf.FloorToInt(elapsed) > Mathf.FloorToInt(prev))
                    playerCamera.Shake(0.8f, 0.2f);
            }

            if (underwaterTimer >= 3f)
                die();
        }
        else
        {
            underwaterTimer = 0f;
        }
    }

    void HandleFootsteps(bool isRunning, bool isCrouching)
    {
        Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0, characterController.velocity.z);

        if (characterController.isGrounded && horizontalVelocity.magnitude > 0.5f && canMove)
        {
            stepTimer += Time.deltaTime;

            float currentStepRate = walkStepRate;
            if (isCrouching) currentStepRate = crouchStepRate;
            else if (isRunning) currentStepRate = runStepRate;

            if (stepTimer >= currentStepRate)
            {
                if (footstepsClips.Length > 0 && footstepsAudioSource != null)
                {
                    int randomIndex = Random.Range(0, footstepsClips.Length);
                    footstepsAudioSource.PlayOneShot(footstepsClips[randomIndex]);
                }
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }
}