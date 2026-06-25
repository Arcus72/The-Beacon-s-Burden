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
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 20f;
    public float lookSpeed = 0.1f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;

    [Header("Health")]
    public float health = 100f;
    public float shield = 50f;

    [Header("Sea Level")]
    public float sealevel = 0f;
    private float underwaterTimer = 0f;

    [Header("Footsteps Settings")]
    public AudioSource footstepsAudioSource; // Przypisz komponent Audio Source w Inspektorze
    public AudioClip[] footstepsClips;        // Wrzu� tutaj pliki d�wi�kowe swoich krok�w
    public float walkStepRate = 0.5f;         // Odst�p mi�dzy krokami przy chodzeniu (w sekundach)
    public float runStepRate = 0.3f;          // Odst�p mi�dzy krokami przy bieganiu (szybciej!)
    public float crouchStepRate = 0.7f;        // Odst�p mi�dzy krokami przy skradaniu (wolniej)
    private float stepTimer = 0f;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    private Vector3 spawnPoint;

    private bool canMove = true;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        spawnPoint = transform.position;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Heal(int amount)
    {
        health += amount;
        if (health > 100)
        {
            health = 100;
        }
    }

    public void RepairShield(int amount)
    {
        shield += amount;
        if (shield > 50)
        {
            shield = 50;
        }
    }

    public void Restart(){
        gameObject.SetActive(true);
        shield = 50;
        health = 100;
        underwaterTimer = 0f;
        characterController.enabled = false;
        transform.position = spawnPoint;
        characterController.enabled = true;
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
        gameObject.SetActive(false);
        GameMaster.Instance.EndGame();
    }

  
    void Update()
    {
        // Get input data
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        if (keyboard == null || mouse == null) return;

        // Movement info
        float moveX = 0;
        float moveY = 0;
        if (keyboard.wKey.isPressed) moveY = 1;
        if (keyboard.sKey.isPressed) moveY = -1;
        if (keyboard.dKey.isPressed) moveX = 1;
        if (keyboard.aKey.isPressed) moveX = -1;

        // Mouse
        Vector2 mouseDelta = mouse.delta.ReadValue();

        // Movement
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = keyboard.leftShiftKey.isPressed;
        bool isCrouching = keyboard.ctrlKey.isPressed && canMove;

        float curSpeedX = canMove ? (isCrouching ? crouchSpeed : (isRunning ? runSpeed : walkSpeed)) * moveY : 0;
        float curSpeedY = canMove ? (isCrouching ? crouchSpeed : (isRunning ? runSpeed : walkSpeed)) * moveX : 0;
        float movementDirectionY = moveDirection.y;

        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Jump
        if (keyboard.spaceKey.wasPressedThisFrame && canMove && characterController.isGrounded)
            moveDirection.y = jumpPower;
        else
            moveDirection.y = movementDirectionY;

        // Gravitation
        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        // Crouch
        if (isCrouching)
        {
            characterController.height = crouchHeight;
        }
        else
        {
            characterController.height = defaultHeight;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        // Rotation
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

        if (characterController.isGrounded && horizontalVelocity.magnitude > 0.1f && canMove)
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
            stepTimer = walkStepRate;
        }
    }
}