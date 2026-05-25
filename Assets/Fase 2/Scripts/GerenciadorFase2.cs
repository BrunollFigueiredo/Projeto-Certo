using Fusion;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Tamanhos possíveis de objetos entregues
public enum TamanhoObjeto { Pequeno, Medio, Grande }

// Gerencia o progresso da Fase 2 e abre a porta no fim
public class GerenciadorFase2 : NetworkBehaviour
{
    [SerializeField] private int necessarioPequeno = 2; // Pequenos que precisam ser entregues
    [SerializeField] private int necessarioMedio = 3;   // Médios que precisam ser entregues
    [SerializeField] private int necessarioGrande = 1;  // Grandes que precisam ser entregues

    [SerializeField] private GameObject porta;          // Porta a abrir no fim
    [SerializeField] private float distanciaPorta = 4f; // Quanto a porta se move ao abrir

    [SerializeField] private Slider barraProgresso;     // Barra de progresso da UI
    [SerializeField] private TextMeshProUGUI textoPequeno; // Texto contador dos pequenos
    [SerializeField] private TextMeshProUGUI textoMedio;   // Texto contador dos médios
    [SerializeField] private TextMeshProUGUI textoGrande;  // Texto contador dos grandes

    // Contadores sincronizados em rede
    [Networked] private int entregasPequeno { get; set; }
    [Networked] private int entregasMedio { get; set; }
    [Networked] private int entregasGrande { get; set; }
    [Networked] private bool portaAberta { get; set; }

    // Atualiza a UI todo frame
    public override void Render()
    {
        // Calcula o total entregue sem ultrapassar o necessário
        int total = necessarioPequeno + necessarioMedio + necessarioGrande;
        int totalEntregues = Mathf.Min(entregasPequeno, necessarioPequeno)
                           + Mathf.Min(entregasMedio, necessarioMedio)
                           + Mathf.Min(entregasGrande, necessarioGrande);

        // Atualiza a barra de progresso
        if (barraProgresso != null)
        {
            if (total > 0)
            {
                barraProgresso.value = (float)totalEntregues / total;
            }
            else
            {
                barraProgresso.value = 0f;
            }
        }

        // Atualiza os contadores de texto
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

    // Chamado por outros scripts quando um objeto é entregue
    public void RegistrarEntrega(TamanhoObjeto tamanho)
    {
        // Só o host atualiza os contadores
        if (!HasStateAuthority) return;

        // Soma no contador certo dependendo do tamanho
        if (tamanho == TamanhoObjeto.Pequeno)
        {
            entregasPequeno++;
        }
        else if (tamanho == TamanhoObjeto.Medio)
        {
            entregasMedio++;
        }
        else if (tamanho == TamanhoObjeto.Grande)
        {
            entregasGrande++;
        }

        // Se completou tudo, abre a porta
        if (!portaAberta
            && entregasPequeno >= necessarioPequeno
            && entregasMedio >= necessarioMedio
            && entregasGrande >= necessarioGrande)
        {
            portaAberta = true;
            RPC_AbrirPorta();
        }
    }

    // RPC: move a porta em todos os clientes
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_AbrirPorta()
    {
        if (porta != null)
        {
            porta.transform.Translate(0, 0, -distanciaPorta, Space.World);
        }
    }
}
