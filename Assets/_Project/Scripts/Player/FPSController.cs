using UnityEngine;

public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;

    [Header("Mouse Look")]
    public float lookSpeed = 3f;

    [Header("Camera")]
    [SerializeField] private Camera playerCamera;

    private CharacterController cc;
    private float rotationX; // pitch
    private float rotationY; // yaw
    private float verticalVelocity;

    void Start()
    {
        cc = GetComponent<CharacterController>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        rotationY = transform.eulerAngles.y;

        if (playerCamera != null)
        {
            rotationX = playerCamera.transform.localEulerAngles.x;
            if (rotationX > 180f) rotationX -= 360f;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Cek pause
        if (Time.timeScale == 0f || Cursor.lockState != CursorLockMode.Locked)
            return;

        HandleMouseLook();
    }

    void FixedUpdate()
    {
        if (Time.timeScale == 0f || Cursor.lockState != CursorLockMode.Locked)
            return;

        HandleMovement();
    }

    void HandleMouseLook()
    {
        // Pakai GetAxisRaw untuk konsistensi — langsung kalikan sensitivity
        float mouseX = Input.GetAxisRaw("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxisRaw("Mouse Y") * lookSpeed;

        rotationY += mouseX;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);

        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = (transform.right * h + transform.forward * v).normalized;
        move *= speed;

        if (cc.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
        else
            verticalVelocity += Physics.gravity.y * Time.fixedDeltaTime;

        move.y = verticalVelocity;
        cc.Move(move * Time.fixedDeltaTime);
    }

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
