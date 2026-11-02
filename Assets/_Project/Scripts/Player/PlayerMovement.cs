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

}
