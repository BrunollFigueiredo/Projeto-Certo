using UnityEngine;

public class Esteira : MonoBehaviour
{
    [SerializeField] private float velocidade = 3f;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("ItemEsteira")) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, velocidade);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("ItemEsteira")) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, 0f);
    }
}
