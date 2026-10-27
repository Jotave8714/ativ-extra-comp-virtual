using UnityEngine;

/// <summary>
/// Plataforma VERDE: sobe e desce dentro da altura do cilindro.
/// - Jogador na plataforma amarela  -> desce
/// - Caso contrário                 -> sobe e fica na altura máxima
///
/// Posicione a plataforma na altura MÁXIMA no editor (topo do cilindro).
/// Adicione um Rigidbody (Is Kinematic = true) e um Collider.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ElevatorPlatform : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private PlatformSensor yellowPlatform;
    [Tooltip("Opcional: se atribuído, a altura mínima é calculada pela base do cilindro.")]
    [SerializeField] private Renderer cylinder;

    [Header("Movimento")]
    [SerializeField] private float speed = 3f;
    [Tooltip("Usado apenas se o cilindro não for atribuído.")]
    [SerializeField] private float travelDistance = 5f;

    private Rigidbody rb;
    private float topY;
    private float bottomY;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        topY = transform.position.y;

        if (cylinder != null)
        {
            // Base do cilindro + metade da espessura da plataforma
            float halfThickness = 0f;
            Collider col = GetComponent<Collider>();
            if (col != null) halfThickness = col.bounds.extents.y;

            bottomY = cylinder.bounds.min.y + halfThickness;
        }
        else
        {
            bottomY = topY - travelDistance;
        }
    }

    private void FixedUpdate()
    {
        bool descer = yellowPlatform != null && yellowPlatform.PlayerOnTop;
        float alvoY = descer ? bottomY : topY;

        Vector3 pos = rb.position;
        pos.y = Mathf.MoveTowards(pos.y, alvoY, speed * Time.fixedDeltaTime);
        rb.MovePosition(pos);
    }

    // --- Faz o jogador acompanhar a plataforma ---
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            collision.transform.SetParent(transform, true);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            collision.transform.SetParent(null, true);
    }
}
