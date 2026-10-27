using UnityEngine;

public class PlayerCameraFollow : MonoBehaviour
{
    [Header("Alvo")]
    public Transform target;
    public Vector3 targetOffset = new Vector3(0f, 1.4f, 0f);

    [Header("Distância e Ângulo")]
    public float distance = 7.0f;
    public Vector2 pitchLimits = new Vector2(-15f, 65f); // Permite olhar para o alto na subida
    public float mouseSensitivity = 3.0f;

    [Header("Suavização")]
    public float positionSmoothTime = 0.08f;
    public float rotationSmoothTime = 0.05f;

    [Header("Anticolisão de Câmera")]
    public LayerMask collisionLayers;
    public float cameraRadius = 0.25f;

    private float yaw;
    private float pitch = 20f;
    private Vector3 currentRotation;
    private Vector3 rotationSmoothVelocity;
    private Vector3 followVelocity;
    private Vector3 smoothedFocusPoint;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (target != null)
        {
            smoothedFocusPoint = target.position + targetOffset;
            yaw = target.eulerAngles.y;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Leitura do Mouse para girar ao redor do cubo
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);

        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
        Quaternion camRotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0f);

        // 2. Ponto de foco suavizado no eixo Y para não dar trancos nos pulos
        Vector3 targetFocus = target.position + targetOffset;
        smoothedFocusPoint = Vector3.SmoothDamp(smoothedFocusPoint, targetFocus, ref followVelocity, positionSmoothTime);

        // 3. Cálculo da posição final e verificação se há parede bloqueando a visão
        Vector3 camDirection = -(camRotation * Vector3.forward);
        float finalDistance = distance;

        if (Physics.SphereCast(smoothedFocusPoint, cameraRadius, camDirection, out RaycastHit hit, distance, collisionLayers, QueryTriggerInteraction.Ignore))
        {
            finalDistance = Mathf.Clamp(hit.distance - 0.1f, 1.0f, distance);
        }

        transform.position = smoothedFocusPoint + camDirection * finalDistance;
        transform.rotation = camRotation;
    }
}