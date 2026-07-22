using UnityEngine;

public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float jumpHeight = 3f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 40f;
    public Transform playerCamera;

    private CharacterController cc;
    private float xRotation;
    private float verticalVelocity;
    private bool jumpRequested;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        if (playerCamera == null) playerCamera = transform.Find("Main Camera");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;
        Look();

        if (cc.isGrounded && Input.GetButtonDown("Jump"))
            jumpRequested = true;
    }

    void FixedUpdate()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;
        Move();
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation  = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        move *= speed;

        if (cc.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        else verticalVelocity += Physics.gravity.y * Time.fixedDeltaTime;

        if (jumpRequested)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            jumpRequested = false;
        }

        move.y = verticalVelocity;
        cc.Move(move * Time.fixedDeltaTime);
    }
}
