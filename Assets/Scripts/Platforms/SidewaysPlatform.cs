using UnityEngine;

/// <summary>
/// Plataforma LARANJA: movimenta-se de um lado para o outro (vai e volta).
/// Adicione um Rigidbody (Is Kinematic = true) e um Collider.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SidewaysPlatform : MonoBehaviour
{
    [Header("Movimento")]
    [Tooltip("Eixo do movimento no espaço do mundo (X = lados).")]
    [SerializeField] private Vector3 direction = Vector3.right;
    [Tooltip("Distância percorrida para cada lado a partir da posição inicial.")]
    [SerializeField] private float distance = 3f;
    [SerializeField] private float speed = 2f;

    private Rigidbody rb;
    private Vector3 startPos;
    private Vector3 dir;
    private float t;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        startPos = transform.position;
        dir = direction.normalized;
    }

    private void FixedUpdate()
    {
        // PingPong suave: vai de -distance até +distance
        t += Time.fixedDeltaTime * speed;
        float offset = Mathf.Sin(t) * distance;
        rb.MovePosition(startPos + dir * offset);
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
