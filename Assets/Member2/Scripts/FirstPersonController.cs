using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private InputActionAsset inputActions;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float sprintSpeed = 5.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float maxLookAngle = 80f;

    private CharacterController controller;
    private InputAction moveAction, lookAction, sprintAction;
    private float pitch;
    private float yVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        InputActionMap map = inputActions.FindActionMap("Player", true);
        moveAction   = map.FindAction("Move", true);
        lookAction   = map.FindAction("Look", true);
        sprintAction = map.FindAction("Sprint", true);
        map.Enable();
    }

    private void OnEnable()  => SetCursorLocked(true);
    private void OnDisable() => SetCursorLocked(false);

    private void Update()
    {
        // Look around
        Vector2 look = lookAction.ReadValue<Vector2>() * mouseSensitivity;
        transform.Rotate(0f, look.x, 0f);
        pitch = Mathf.Clamp(pitch - look.y, -maxLookAngle, maxLookAngle);
        cameraHolder.localEulerAngles = new Vector3(pitch, 0f, 0f);

        // Walk
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 direction = transform.right * input.x + transform.forward * input.y;
        float speed = sprintAction.IsPressed() ? sprintSpeed : walkSpeed;

        if (controller.isGrounded && yVelocity < 0f) yVelocity = -2f;
        yVelocity += gravity * Time.deltaTime;

        controller.Move((direction * speed + Vector3.up * yVelocity) * Time.deltaTime);
    }

    public static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}