using Fusion;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum TamanhoObjeto { Pequeno, Medio, Grande }

public class GerenciadorFase2 : NetworkBehaviour
{
    [Header("Requisitos")]
    [SerializeField] private int necessarioPequeno = 2;
    [SerializeField] private int necessarioMedio   = 3;
    [SerializeField] private int necessarioGrande  = 1;

    [Header("Porta")]
    [SerializeField] private GameObject porta;
    [SerializeField] private float distanciaPorta = 4f;

    [Header("UI")]
    [SerializeField] private Slider barraProgresso;
    [SerializeField] private TextMeshProUGUI textoPequeno;
    [SerializeField] private TextMeshProUGUI textoMedio;
    [SerializeField] private TextMeshProUGUI textoGrande;

    [Networked] private int entregasPequeno { get; set; }
    [Networked] private int entregasMedio   { get; set; }
    [Networked] private int entregasGrande  { get; set; }
    [Networked] private bool portaAberta    { get; set; }

    private int Total         => necessarioPequeno + necessarioMedio + necessarioGrande;
    private int TotalEntregues => Mathf.Min(entregasPequeno, necessarioPequeno)
                                + Mathf.Min(entregasMedio,   necessarioMedio)
                                + Mathf.Min(entregasGrande,  necessarioGrande);

    public override void Render()
    {
        if (barraProgresso != null)
            barraProgresso.value = Total > 0 ? (float)TotalEntregues / Total : 0f;

        if (textoPequeno != null) textoPequeno.text = $"P: {Mathf.Min(entregasPequeno, necessarioPequeno)}/{necessarioPequeno}";
        if (textoMedio   != null) textoMedio.text   = $"M: {Mathf.Min(entregasMedio,   necessarioMedio)}/{necessarioMedio}";
        if (textoGrande  != null) textoGrande.text  = $"G: {Mathf.Min(entregasGrande,  necessarioGrande)}/{necessarioGrande}";
    }

    public void RegistrarEntrega(TamanhoObjeto tamanho)
    {
        if (!HasStateAuthority) return;

        switch (tamanho)
        {
            case TamanhoObjeto.Pequeno: entregasPequeno++; break;
            case TamanhoObjeto.Medio:   entregasMedio++;   break;
            case TamanhoObjeto.Grande:  entregasGrande++;  break;
        }

        if (!portaAberta
            && entregasPequeno >= necessarioPequeno
            && entregasMedio   >= necessarioMedio
            && entregasGrande  >= necessarioGrande)
        {
            portaAberta = true;
            RPC_AbrirPorta();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_AbrirPorta()
    {
        if (porta != null)
            porta.transform.Translate(0, 0, -distanciaPorta, Space.World);
    }
}
