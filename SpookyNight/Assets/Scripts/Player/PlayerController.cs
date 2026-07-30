using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("이동")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float mouseSensitivity = 2f;

    [Header("점프")]
    public float jumpForce = 5f;
    public float gravity = -20f;

    [Header("대쉬")]
    public float dashSpeed = 15f;
    public float dashTime = 0.2f;
    public float dashCooldown = 1f;

    private CharacterController controller;
    private Camera cam;
    private float verticalRotation = 0f;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Move();
        Look();
        ApplyGravity();

    }

    void Move()
    {
        isGrounded = controller.isGrounded;

        // 땅에 있을 때 아래로 밀리는 velocity 초기화
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Shift 누르면 달리기
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        // 점프 (Space, 땅에 있을 때만)
        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = jumpForce;
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);

        cam.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}