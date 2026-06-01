using Fusion;
using UnityEngine;

public class PortaFinal : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return;
        if (!other.CompareTag("Player")) return;

        GerenciadorFase4.Instance.RPC_MudarJogadoresFinal(1);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!HasStateAuthority) return;
        if (!other.CompareTag("Player")) return;

        GerenciadorFase4.Instance.RPC_MudarJogadoresFinal(-1);
    }
}
