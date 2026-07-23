using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float mouseSensitivity = 2f;
    public float gravity = -20f; // Gravitasi yang lebih stabil

    private CharacterController controller;
    private float xRotation = 0f;
    private Camera playerCamera;
    private Vector3 velocity; // Variabel untuk menyimpan kecepatan jatuh

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 1. Gerakan Badan (WASD)
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * walkSpeed * Time.deltaTime);

        // 2. Gerakan Kepala (Mouse)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 3. Gravitasi yang Diperbaiki (Tidak akan tembus lantai)
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Menekan ke bawah sedikit biar tetap nempel tanah
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime); // Pakai variabel velocity
    }
}