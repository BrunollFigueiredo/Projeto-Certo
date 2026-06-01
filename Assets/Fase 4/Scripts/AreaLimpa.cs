using Fusion;
using UnityEngine;

public class AreaLimpa : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return;
        if (!other.CompareTag("Pegavel")) return;

        GerenciadorFase4.Instance.RPC_MudarBoxes(1);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!HasStateAuthority) return;
        if (!other.CompareTag("Pegavel")) return;

        GerenciadorFase4.Instance.RPC_MudarBoxes(-1);
    }
}
