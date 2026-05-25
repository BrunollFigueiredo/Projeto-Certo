using UnityEngine;

public class PontoDeEncaixe : MonoBehaviour
{
    [SerializeField] private float raioDeSnap = 1.5f;
    [SerializeField] private string tagAceita = "Pegavel"; // deixe vazio para aceitar qualquer objeto

    private bool ocupado = false;
    private Transform objetoEncaixado = null;

    public bool Ocupado => ocupado;
    public float RaioDeSnap => raioDeSnap;
    public Transform ObjetoEncaixado => objetoEncaixado;

    public bool AceitaObjeto(GameObject objeto)
    {
        if (ocupado) return false;
        if (!string.IsNullOrEmpty(tagAceita) && !objeto.CompareTag(tagAceita)) return false;
        return true;
    }

    public void EncaixarObjeto(Transform objeto)
    {
        ocupado = true;
        objetoEncaixado = objeto;

        objeto.SetParent(null);
        objeto.position = transform.position;
        objeto.rotation = transform.rotation;

        Rigidbody rb = objeto.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Desativa o componente para o objeto não poder ser pego novamente
        ArrastarItem pegavel = objeto.GetComponent<ArrastarItem>();
        if (pegavel != null) pegavel.enabled = false;
    }

    public Transform LiberarObjeto()
    {
        if (!ocupado || objetoEncaixado == null) return null;

        Transform obj = objetoEncaixado;
        ocupado = false;
        objetoEncaixado = null;
        return obj;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = ocupado ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, raioDeSnap);
    }
}
