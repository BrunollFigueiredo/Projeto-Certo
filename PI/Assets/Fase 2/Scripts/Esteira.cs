using UnityEngine;

// Empurra objetos que tocam na esteira para a direção Z negativa
public class Esteira : MonoBehaviour
{
    [SerializeField] private float velocidade = 3f; // Velocidade que a esteira aplica nos objetos

    // Chamado quando algo bate na esteira
    private void OnCollisionEnter(Collision collision)
    {
        Aplicar(collision.rigidbody);
    }

    // Continua aplicando enquanto o objeto estiver em cima
    private void OnCollisionStay(Collision collision)
    {
        Aplicar(collision.rigidbody);
    }

    // Aplica a velocidade no objeto se ele puder ser movido
    private void Aplicar(Rigidbody rb)
    {
        // Ignora se não tem Rigidbody ou se é estático
        if (rb == null) return;
        if (rb.isKinematic) return;
        // Só age em objetos com a tag "Pegavel"
        if (!rb.gameObject.CompareTag("Pegavel")) return;

        // Substitui só a velocidade no eixo Z, mantém X e Y
        Vector3 vel = rb.linearVelocity;
        vel.z = -velocidade;
        rb.linearVelocity = vel;
    }
}
