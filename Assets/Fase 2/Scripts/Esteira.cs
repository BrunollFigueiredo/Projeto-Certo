using UnityEngine;

public class Esteira : MonoBehaviour
{
    [SerializeField] private float velocidade = 3f;

    private void OnCollisionEnter(Collision collision)
    {
        Aplicar(collision.rigidbody);
    }

    private void OnCollisionStay(Collision collision)
    {
        Aplicar(collision.rigidbody);
    }

    private void Aplicar(Rigidbody rb)
    {
        if (rb == null || rb.isKinematic) return;
        if (!rb.gameObject.CompareTag("Pegavel")) return;

        Vector3 vel = rb.linearVelocity;
        vel.z = -velocidade;
        rb.linearVelocity = vel;
    }
}
