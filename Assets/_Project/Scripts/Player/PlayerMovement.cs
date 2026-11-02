using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform cameraPivot;

    [Header("Movimento")]
    [SerializeField] private float walkSpeed = 7f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float groundAcceleration = 60f;
    [SerializeField] private float airAcceleration = 20f;

    [Header("Pulo e gravidade")]
    [SerializeField] private float jumpHeight = 1.4f;
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.13f;

    [Header("Câmera")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float maxLookAngle = 85f;

    private CharacterController controller;
    private Vector3 horizontalVelocity;
    private float verticalVelocity;
    private float pitch;
    private float lastGroundedTime = -999f;
    private float lastJumpPressedTime = -999f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        LockCursor(true);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if(keyboard == null || mouse == null)
        {
            return;
        }

        HandleCursor(keyboard, mouse);

        if(Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        Lock(mouse);
        Move(keyboard);
    }

    private void HandleCursor(Keyboard keyboard, Mouse mouse)
    {
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            LockCursor(false);
        }

        if (mouse.leftButton.wasPressedThisFrame)
        {
            LockCursor(true);
        }
    }

    public void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void Lock(Mouse mouse)
    {
        Vector2 delta = mouse.delta.ReadValue() * mouseSensitivity;

        transform.Rotate(Vector3.up * delta.x);

        pitch = Mathf.Clamp(pitch - delta.y, -maxLookAngle, maxLookAngle);

        cameraPivot.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    private void Move(Keyboard keyboard)
    {
        Vector2 input = Vector2.zero;

        if(keyboard.wKey.isPressed) input.y += 1f;
        if(keyboard.aKey.isPressed) input.x -= 1f;
        if(keyboard.sKey.isPressed) input.y -= 1f;
        if(keyboard.dKey.isPressed) input.x += 1f;

        input = Vector2.ClampMagnitude(input,1f);

        Vector3 wishDir = transform.right * input.x + transform.forward * input.y;

        float targetSpeed = keyboard.leftShiftKey.isPressed ? sprintSpeed : walkSpeed;

        Vector3 targetVelocity = wishDir * targetSpeed;

        bool grounded = controller.isGrounded;

        if (grounded)
        {
            lastGroundedTime = Time.time;
        }

        bool keepAirMomentum = !grounded && input == Vector2.zero;

        if (!keepAirMomentum)
        {
            float accel = grounded ? groundAcceleration : airAcceleration;

            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                targetVelocity,
                accel * Time.deltaTime
            );
        }

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            lastJumpPressedTime = Time.time;
        }

        if(grounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        bool canJump = Time.time - lastGroundedTime <= coyoteTime;
        bool wantsJump = Time.time - lastJumpPressedTime <= jumpBufferTime;

        if(canJump && wantsJump)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            lastJumpPressedTime = -999f;
            lastGroundedTime = -999f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;

        CollisionFlags flags = controller.Move(velocity * Time.deltaTime);

        if((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
        {
            verticalVelocity = 0f;
        }
    }

}
