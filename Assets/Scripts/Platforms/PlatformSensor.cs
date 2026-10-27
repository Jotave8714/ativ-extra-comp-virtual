using UnityEngine;

/// <summary>
/// Colocar na plataforma AMARELA (precisa de Collider; o Player precisa ter a tag "Player").
/// Informa se o jogador está em cima da plataforma.
/// </summary>
public class PlatformSensor : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private int contatos;

    public bool PlayerOnTop => contatos > 0;

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag(playerTag)) return;

        // Só conta se o contato for por cima da plataforma (e não pela lateral)
        foreach (ContactPoint contato in collision.contacts)
        {
            if (contato.normal.y < -0.5f)
            {
                contatos++;
                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!collision.gameObject.CompareTag(playerTag)) return;
        contatos = Mathf.Max(0, contatos - 1);
    }
}
