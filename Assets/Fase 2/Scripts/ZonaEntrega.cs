using Fusion;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ZonaEntrega : NetworkBehaviour
{
    [Header("Requisitos (tamanho certo)")]
    [SerializeField] private int necessarioPequeno = 2;
    [SerializeField] private int necessarioMedio   = 3;
    [SerializeField] private int necessarioGrande  = 1;

    [Header("Limites de escala")]
    [SerializeField] private float limiteMaxPequeno = 0.75f;
    [SerializeField] private float limiteMaxMedio   = 1.25f;

    [Header("Porta")]
    [SerializeField] private GameObject porta;
    [SerializeField] private float posicaoZPortaAberta = -9.5f;

    [Header("UI")]
    [SerializeField] private Image barraPreenchimento;
    [SerializeField] private TextMeshProUGUI textoPequeno;
    [SerializeField] private TextMeshProUGUI textoMedio;
    [SerializeField] private TextMeshProUGUI textoGrande;

    [Networked] private int entregasPequeno { get; set; }
    [Networked] private int entregasMedio   { get; set; }
    [Networked] private int entregasGrande  { get; set; }
    [Networked] private bool portaAberta    { get; set; }

    private int TotalEntregues =>
        Mathf.Min(entregasPequeno, necessarioPequeno) +
        Mathf.Min(entregasMedio,   necessarioMedio)   +
        Mathf.Min(entregasGrande,  necessarioGrande);

    private int Total => necessarioPequeno + necessarioMedio + necessarioGrande;

    public override void Render()
    {
        if (barraPreenchimento != null)
            barraPreenchimento.fillAmount = Total > 0 ? (float)TotalEntregues / Total : 0f;

        if (textoPequeno != null)
            textoPequeno.text = $"P: {Mathf.Min(entregasPequeno, necessarioPequeno)}/{necessarioPequeno}";
        if (textoMedio != null)
            textoMedio.text = $"M: {Mathf.Min(entregasMedio, necessarioMedio)}/{necessarioMedio}";
        if (textoGrande != null)
            textoGrande.text = $"G: {Mathf.Min(entregasGrande, necessarioGrande)}/{necessarioGrande}";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Pegavel")) return;

        Destroy(other.gameObject);

        if (!HasStateAuthority) return;

        float escala = other.transform.localScale.x;
        if (escala <= limiteMaxPequeno)
            entregasPequeno++;
        else if (escala <= limiteMaxMedio)
            entregasMedio++;
        else
            entregasGrande++;

        Debug.Log($"[ZonaEntrega] escala={escala:0.##} | P={entregasPequeno} M={entregasMedio} G={entregasGrande}");

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
        {
            Vector3 pos = porta.transform.position;
            pos.z = posicaoZPortaAberta;
            porta.transform.position = pos;
        }
    }
}
