using UnityEngine;

public class Forno : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("ItemEsteira")) return;

        if (BarraProgresso.Instance != null)
        {
            BarraProgresso.Instance.AdicionarCarvao();
        }

        Destroy(other.gameObject);
    }
}
