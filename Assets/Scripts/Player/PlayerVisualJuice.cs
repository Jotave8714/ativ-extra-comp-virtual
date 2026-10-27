using UnityEngine;

public class PlayerVisualJuice : MonoBehaviour
{
    [Header("Referência da Malha 3D (Filho do Player)")]
    public Transform visualMesh;

    [Header("Efeito Squash & Stretch (Pulo e Pouso)")]
    public Vector3 jumpStretchScale = new Vector3(0.8f, 1.25f, 0.8f);
    public Vector3 landSquashScale = new Vector3(1.25f, 0.75f, 1.25f);
    public float scaleReturnSpeed = 10f;

    [Header("Inclinação ao Correr (Tilt)")]
    public float maxTiltAngle = 12f;
    public float tiltSpeed = 10f;

    private PlayerMotor motor;
    private Vector3 originalScale;

    void Awake()
    {
        motor = GetComponent<PlayerMotor>();
        if (visualMesh != null)
            originalScale = visualMesh.localScale;
    }

    void OnEnable()
    {
        if (motor != null)
        {
            motor.OnJumpExecuted += ApplyJumpStretch;
            motor.OnLanded += ApplyLandSquash;
        }
    }

    void OnDisable()
    {
        if (motor != null)
        {
            motor.OnJumpExecuted -= ApplyJumpStretch;
            motor.OnLanded -= ApplyLandSquash;
        }
    }

    void Update()
    {
        if (visualMesh == null) return;

        // Retorna suavemente a escala da malha para o tamanho original (1, 1, 1)
        visualMesh.localScale = Vector3.Lerp(visualMesh.localScale, originalScale, scaleReturnSpeed * Time.deltaTime);

        // Inclina o cubo levemente para frente quando está em movimento
        Vector3 horizontalVel = new Vector3(motor.CurrentVelocity.x, 0f, motor.CurrentVelocity.z);
        float speedPercent = Mathf.Clamp01(horizontalVel.magnitude / motor.moveSpeed);
        float targetTilt = speedPercent * maxTiltAngle;

        Quaternion targetLocalRot = Quaternion.Euler(targetTilt, 0f, 0f);
        visualMesh.localRotation = Quaternion.Slerp(visualMesh.localRotation, targetLocalRot, tiltSpeed * Time.deltaTime);
    }

    private void ApplyJumpStretch()
    {
        if (visualMesh != null)
            visualMesh.localScale = Vector3.Scale(originalScale, jumpStretchScale);
    }

    private void ApplyLandSquash(float impactSpeed)
    {
        if (visualMesh != null && impactSpeed > 5f)
            visualMesh.localScale = Vector3.Scale(originalScale, landSquashScale);
    }
}