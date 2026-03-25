using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem; // Dodane!

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour, IDamageable
{
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

    public float health = 100;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;

    private bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void TakeDamage(float amount)
    {
        health -= amount;

     
        if (playerCamera != null)
        {
            playerCamera.Shake(0.15f, 0.3f); // (Duration, Strength)
        }

        if (health <= 0)
        {
            //TODO: End game after players's death
            Destroy(gameObject);
        }
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

        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * moveY : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * moveX : 0;
        float movementDirectionY = moveDirection.y;

        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Jump
        if (keyboard.spaceKey.wasPressedThisFrame && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Gravitation
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Crouch
        if (keyboard.ctrlKey.isPressed && canMove)
        {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;
        }
        else
        {
            characterController.height = defaultHeight;
            walkSpeed = 6f;
            runSpeed = 12f;
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
    }
}