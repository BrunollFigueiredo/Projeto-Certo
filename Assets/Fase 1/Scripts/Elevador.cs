using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class Elevador : NetworkBehaviour
{
    [SerializeField] private string cenaFase2 = "Fase2";
    [SerializeField] private int jogadoresNecessarios = 1;

    private HashSet<GameObject> jogadoresNaArea = new HashSet<GameObject>();
    private bool transicionando = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return;

        GameObject raiz = other.transform.root.gameObject;
        if (!raiz.CompareTag("Player")) return;

        jogadoresNaArea.Add(raiz);

        if (!transicionando && jogadoresNaArea.Count >= jogadoresNecessarios)
        {
            transicionando = true;
            RPC_IrParaFase2();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!HasStateAuthority) return;

        GameObject raiz = other.transform.root.gameObject;
        if (!raiz.CompareTag("Player")) return;

        jogadoresNaArea.Remove(raiz);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_IrParaFase2()
    {
        TransicaoFase.Ir(Runner, cenaFase2);
    }
}
