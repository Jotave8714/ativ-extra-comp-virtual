using UnityEngine;
using System;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    [Header("Movimentação Horizontal")]
    public float moveSpeed = 6.5f;
    public float acceleration = 15f;
    public float rotationSpeed = 14f;

    [Header("Pulo e Gravidade")]
    public float jumpHeight = 2.6f;
    public float gravityUp = -22f;      // Gravidade enquanto está subindo no pulo
    public float gravityDown = -35f;    // Gravidade mais forte na queda (dá precisão)
    public float maxFallSpeed = -40f;

    [Header("Assistência de Precisão (Game Feel)")]
    public float coyoteTime = 0.15f;    // Tolerância ao sair da borda da plataforma
    public float jumpBufferTime = 0.15f;// Tolerância ao apertar pulo antes de tocar o chão

    [Header("Detecção de Chão")]
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.35f;
    public float groundCheckOffset = 0.1f;

    // Eventos para comunicar com o script de efeitos visuais/som
    public event Action OnJumpExecuted;
    public event Action<float> OnLanded;

    public bool IsGrounded { get; private set; }
    public Vector3 CurrentVelocity => currentHorizontalVelocity + Vector3.up * verticalVelocity;
    public Transform CurrentGroundTransform { get; private set; }

    private CharacterController controller;
    private Transform cameraTransform;
    private Vector3 currentHorizontalVelocity;
    private float verticalVelocity;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private bool wasGroundedLastFrame;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        CheckGround();
        HandleJumpInput();
        HandleHorizontalMovement();
        ApplyGravityAndJump();
        MoveCharacter();
    }

    private void CheckGround()
    {
        Vector3 spherePos = transform.position + Vector3.up * (groundCheckRadius - groundCheckOffset);
        
        if (Physics.SphereCast(spherePos, groundCheckRadius, Vector3.down, out RaycastHit hit, groundCheckOffset + 0.15f, groundLayer, QueryTriggerInteraction.Ignore))
        {
            IsGrounded = true;
            CurrentGroundTransform = hit.transform;
            coyoteTimer = coyoteTime;

            if (!wasGroundedLastFrame && verticalVelocity <= 0f)
            {
                OnLanded?.Invoke(Mathf.Abs(verticalVelocity));
            }
        }
        else
        {
            IsGrounded = false;
            CurrentGroundTransform = null;
            coyoteTimer -= Time.deltaTime;
        }

        wasGroundedLastFrame = IsGrounded;
    }

    private void HandleJumpInput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }
    }

    private void HandleHorizontalMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = new Vector3(h, 0f, v).normalized;
        Vector3 targetVelocity = Vector3.zero;

        if (inputDir.magnitude >= 0.1f)
        {
            Vector3 camForward = cameraTransform != null ? Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized : Vector3.forward;
            Vector3 camRight = cameraTransform != null ? Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized : Vector3.right;

            Vector3 moveDir = (camForward * inputDir.z + camRight * inputDir.x).normalized;
            targetVelocity = moveDir * moveSpeed;

            // Rotaciona suavemente o cubo para olhar na direção em que está andando
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        currentHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, targetVelocity, acceleration * Time.deltaTime);
    }

    private void ApplyGravityAndJump()
    {
        // Executa o pulo se o buffer estiver ativo e o Coyote Time permitir
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravityUp);
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            IsGrounded = false;
            CurrentGroundTransform = null;

            OnJumpExecuted?.Invoke();
        }
        else if (IsGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f; // Mantém o player firme no chão
        }
        else
        {
            // Usa gravidade mais pesada quando está caindo ou quando solta o botão de pulo cedo
            float currentGravity = (verticalVelocity > 0f && Input.GetButton("Jump")) ? gravityUp : gravityDown;
            verticalVelocity = Mathf.Max(verticalVelocity + currentGravity * Time.deltaTime, maxFallSpeed);
        }
    }

    private void MoveCharacter()
    {
        Vector3 finalDisplacement = (currentHorizontalVelocity + Vector3.up * verticalVelocity) * Time.deltaTime;
        controller.Move(finalDisplacement);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Vector3 spherePos = transform.position + Vector3.up * (groundCheckRadius - groundCheckOffset);
        Gizmos.DrawWireSphere(spherePos + Vector3.down * (groundCheckOffset + 0.05f), groundCheckRadius);
    }
}