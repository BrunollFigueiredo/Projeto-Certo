using Fusion;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Recebe os objetos prensados e conta quantos de cada tamanho foram entregues
public class ZonaEntrega : NetworkBehaviour
{
    [SerializeField] private int necessarioPequeno = 2; // Quantos pequenos precisam ser entregues
    [SerializeField] private int necessarioMedio = 3;   // Quantos médios precisam ser entregues
    [SerializeField] private int necessarioGrande = 1;  // Quantos grandes precisam ser entregues

    [SerializeField] private float limiteMaxPequeno = 0.75f; // Tamanho máximo para ser considerado pequeno
    [SerializeField] private float limiteMaxMedio = 1.25f;   // Tamanho máximo para ser considerado médio

    [SerializeField] private GameObject porta;                // Porta que abre quando tudo é entregue
    [SerializeField] private float posicaoZPortaAberta = -9.5f; // Posição Z final da porta

    [SerializeField] private Image barraPreenchimento;        // Barra de progresso da UI
    [SerializeField] private TextMeshProUGUI textoPequeno;    // Texto contador dos pequenos
    [SerializeField] private TextMeshProUGUI textoMedio;      // Texto contador dos médios
    [SerializeField] private TextMeshProUGUI textoGrande;     // Texto contador dos grandes

    // Contadores sincronizados em rede
    [Networked] private int entregasPequeno { get; set; }
    [Networked] private int entregasMedio { get; set; }
    [Networked] private int entregasGrande { get; set; }
    [Networked] private bool portaAberta { get; set; }

    // Atualiza a UI todo frame
    public override void Render()
    {
        // Calcula o total de entregas válidas (sem passar do necessário)
        int total = necessarioPequeno + necessarioMedio + necessarioGrande;
        int totalEntregues = Mathf.Min(entregasPequeno, necessarioPequeno)
                           + Mathf.Min(entregasMedio, necessarioMedio)
                           + Mathf.Min(entregasGrande, necessarioGrande);

        // Atualiza a barra de preenchimento
        if (barraPreenchimento != null)
        {
            if (total > 0)
            {
                barraPreenchimento.fillAmount = (float)totalEntregues / total;
            }
            else
            {
                barraPreenchimento.fillAmount = 0f;
            }
        }

        // Atualiza os textos com o progresso de cada tamanho
        if (textoPequeno != null)
        {
            textoPequeno.text = "P: " + Mathf.Min(entregasPequeno, necessarioPequeno) + "/" + necessarioPequeno;
        }
        if (textoMedio != null)
        {
            textoMedio.text = "M: " + Mathf.Min(entregasMedio, necessarioMedio) + "/" + necessarioMedio;
        }
        if (textoGrande != null)
        {
            textoGrande.text = "G: " + Mathf.Min(entregasGrande, necessarioGrande) + "/" + necessarioGrande;
        }
    }

    // Chamado quando um objeto entra na zona de entrega
    private void OnTriggerEnter(Collider other)
    {
        // Só aceita objetos com a tag "Pegavel"
        if (!other.CompareTag("Pegavel")) return;

        // Ignora objetos que estão SEGURADOS ou ENCAIXADOS na prensa: nesses
        // estados o Rigidbody é kinematic. A entrega só vale para objetos que
        // chegam pela física (soltos na esteira). Sem isso, o objeto sumia ao ser
        // colocado na prensa, porque encostava no trigger da zona e era despawnado.
        Rigidbody rbOutro = other.GetComponent<Rigidbody>();
        if (rbOutro != null && rbOutro.isKinematic) return;

        // Só o host processa a entrega e remove o objeto. Remover um
        // NetworkObject fora do dono causa erro no Fusion, por isso só
        // o StateAuthority age aqui — o Despawn sincroniza para todos.
        if (!HasStateAuthority) return;

        // Classifica pelo tamanho da escala
        float escala = other.transform.localScale.x;

        if (escala <= limiteMaxPequeno)
        {
            entregasPequeno++;
        }
        else if (escala <= limiteMaxMedio)
        {
            entregasMedio++;
        }
        else
        {
            entregasGrande++;
        }

        // Remove o objeto entregue de forma sincronizada
        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            Runner.Despawn(netObj);
        }
        else
        {
            Destroy(other.gameObject);
        }

        // Se já entregou tudo, abre a porta
        if (!portaAberta
            && entregasPequeno >= necessarioPequeno
            && entregasMedio >= necessarioMedio
            && entregasGrande >= necessarioGrande)
        {
            portaAberta = true;
            RPC_AbrirPorta();
        }
    }

    // RPC: move a porta para a posição aberta em todos os clientes
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_AbrirPorta()
    {
        if (porta != null)
        {
            porta.transform.Translate(0, 15, 0);
        }
    }
}
