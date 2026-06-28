using Fusion;
using UnityEngine;

// Ponto onde um objeto pode ser encaixado (ex: dentro da prensa).
// Agora em rede: o estado "ocupado" sincroniza, entao os dois jogadores sabem
// que tem objeto na prensa (o painel do Aldric usa isso pra liberar os botoes).
public class PontoDeEncaixe : NetworkBehaviour
{
    [SerializeField] private float raioDeSnap = 1.5f;      // Distancia maxima para o objeto encaixar
    [SerializeField] private string tagAceita = "Pegavel"; // Tag dos objetos aceitos

    // Estado sincronizado
    [Networked] public NetworkBool Ocupado { get; set; }
    [Networked] public NetworkObject ObjetoEncaixadoNet { get; set; }

    public float RaioDeSnap => raioDeSnap;

    // Transform do objeto encaixado (resolvido a partir do NetworkObject sincronizado)
    public Transform ObjetoEncaixado =>
        ObjetoEncaixadoNet != null ? ObjetoEncaixadoNet.transform : null;

    // Verifica se este ponto aceita o objeto
    public bool AceitaObjeto(GameObject objeto)
    {
        if (Ocupado) return false;
        if (tagAceita != "" && !objeto.CompareTag(tagAceita)) return false;
        return true;
    }

    // Chamado pelo host quando um objeto encaixa
    public void Ocupar(NetworkObject obj)
    {
        Ocupado = true;
        ObjetoEncaixadoNet = obj;
    }

    // Chamado pelo host quando o objeto sai (enviado pela esteira)
    public Transform Liberar()
    {
        if (!Ocupado) return null;

        Transform obj = ObjetoEncaixado;
        Ocupado = false;
        ObjetoEncaixadoNet = null;
        return obj;
    }

    // Desenha uma esfera no editor mostrando o raio de encaixe
    void OnDrawGizmosSelected()
    {
        // So le o estado de rede depois do objeto existir na rede (antes disso da erro)
        bool ocupadoAgora = Application.isPlaying && Object != null && Ocupado;
        Gizmos.color = ocupadoAgora ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, raioDeSnap);
    }
}
