using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerClimbController : MonoBehaviour
{
    [Header("Movimentação")]
    public float walkSpeed = 6.0f;
    public float rotationSpeed = 12.0f;

    [Header("Pulo e Gravidade")]
    public float jumpHeight = 2.6f;
    public float gravity = -24.0f;
    public float maxFallSpeed = -35.0f; // Limita a velocidade máxima ao despencar de muito alto
    public float coyoteTime = 0.15f;

    [Header("Detecção de Piso")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.25f;

    [Header("Métricas da Subida (Somente Leitura)")]
    public float currentAltitude;
    public float maxAltitudeReached;

    private CharacterController controller;
    private Vector3 velocity;
    private float coyoteTimer;
    private BaseMovingPlatform currentPlatform;
    private float startY;
    private Vector3 initialSpawnPoint;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        startY = transform.position.y;
        initialSpawnPoint = transform.position;
    }

    void Update()
    {
        DetectGroundAndPlatform();
        HandleMovementAndJump();
        UpdateAltitudeMetrics();
    }

    void LateUpdate()
    {
        // Mantém o cubo firme em cima das plataformas verdes (P3) e laranjas (P5)
        if (currentPlatform != null)
        {
            controller.Move(currentPlatform.DeltaPosition);
        }
    }

    private void DetectGroundAndPlatform()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        bool isGrounded = Physics.SphereCast(
            rayOrigin,
            controller.radius * 0.8f,
            Vector3.down,
            out RaycastHit hit,
            groundCheckDistance + 0.1f,
            groundLayer
        );

        if (isGrounded && velocity.y <= 0f)
        {
            coyoteTimer = coyoteTime;
            velocity.y = -2f;
            currentPlatform = hit.collider.GetComponentInParent<BaseMovingPlatform>();
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
            currentPlatform = null;
        }
    }

    private void HandleMovementAndJump()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Transform cam = Camera.main.transform;
        Vector3 forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;

        Vector3 moveDir = (forward * v + right * h).normalized;

        if (moveDir.magnitude >= 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            controller.Move(moveDir * walkSpeed * Time.deltaTime);
        }

        if (Input.GetButtonDown("Jump") && coyoteTimer > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            coyoteTimer = 0f;
            currentPlatform = null;
        }

        velocity.y = Mathf.Max(velocity.y + gravity * Time.deltaTime, maxFallSpeed);
        controller.Move(velocity * Time.deltaTime);
    }

    private void UpdateAltitudeMetrics()
    {
        currentAltitude = Mathf.Max(0f, transform.position.y - startY);
        if (currentAltitude > maxAltitudeReached)
        {
            maxAltitudeReached = currentAltitude;
        }

        // Segurança única: se cair abaixo do chão inicial (P1), volta para a pedra P1 na base
        if (transform.position.y < startY - 15f)
        {
            controller.enabled = false;
            transform.position = initialSpawnPoint + Vector3.up * 1f;
            velocity = Vector3.zero;
            controller.enabled = true;
        }
    }
}