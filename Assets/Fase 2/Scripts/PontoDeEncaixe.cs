using UnityEngine;

// Ponto onde um objeto pode ser encaixado (ex: dentro da prensa)
public class PontoDeEncaixe : MonoBehaviour
{
    [SerializeField] private float raioDeSnap = 1.5f;      // Distância máxima para o objeto encaixar
    [SerializeField] private string tagAceita = "Pegavel"; // Tag dos objetos aceitos

    private bool ocupado = false;            // Se já tem objeto encaixado
    private Transform objetoEncaixado = null; // Referência ao objeto encaixado

    // Propriedades públicas para outros scripts consultarem
    public bool Ocupado
    {
        get { return ocupado; }
    }

    public float RaioDeSnap
    {
        get { return raioDeSnap; }
    }

    public Transform ObjetoEncaixado
    {
        get { return objetoEncaixado; }
    }

    // Verifica se este ponto aceita o objeto
    public bool AceitaObjeto(GameObject objeto)
    {
        // Não aceita se já tem algo encaixado
        if (ocupado) return false;

        // Verifica se a tag bate
        if (tagAceita != "" && !objeto.CompareTag(tagAceita))
        {
            return false;
        }

        return true;
    }

    // Encaixa o objeto no ponto (trava posição e desliga física)
    public void EncaixarObjeto(Transform objeto)
    {
        ocupado = true;
        objetoEncaixado = objeto;

        // Posiciona o objeto exatamente no ponto
        objeto.SetParent(null);
        objeto.position = transform.position;
        objeto.rotation = transform.rotation;

        // Desliga a física do Rigidbody para o objeto ficar parado
        Rigidbody rb = objeto.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Zera a velocidade ANTES de virar kinematic (zerar depois dá warning)
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Desativa o script de arrastar para o jogador não pegar de novo
        ArrastarItem pegavel = objeto.GetComponent<ArrastarItem>();
        if (pegavel != null)
        {
            pegavel.enabled = false;
        }
    }

    // Libera o objeto encaixado e o devolve para quem chamou
    public Transform LiberarObjeto()
    {
        if (!ocupado) return null;
        if (objetoEncaixado == null) return null;

        Transform obj = objetoEncaixado;
        ocupado = false;
        objetoEncaixado = null;
        return obj;
    }

    // Desenha uma esfera no editor mostrando o raio de encaixe
    void OnDrawGizmosSelected()
    {
        // Vermelho se ocupado, verde se vazio
        if (ocupado)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.green;
        }

        Gizmos.DrawWireSphere(transform.position, raioDeSnap);
    }
}
