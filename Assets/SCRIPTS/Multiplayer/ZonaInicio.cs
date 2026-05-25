using Fusion;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ZonaInicio : NetworkBehaviour
{
    [SerializeField] private string cenaFase1 = "Fase1";
    [SerializeField] private int jogadoresNecessarios = 1;
    [SerializeField] private TextMeshProUGUI textoStatus;

    [Networked] private int jogadoresDentro { get; set; }

    private HashSet<GameObject> jogadoresNaArea = new HashSet<GameObject>();
    private bool transicionando = false;

    public override void Render()
    {
        if (textoStatus != null)
            textoStatus.text = $"{jogadoresDentro}/{jogadoresNecessarios} prontos";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return;

        GameObject raiz = other.transform.root.gameObject;
        if (!raiz.CompareTag("Player")) return;

        if (jogadoresNaArea.Add(raiz))
            jogadoresDentro = jogadoresNaArea.Count;

        if (!transicionando && jogadoresDentro >= jogadoresNecessarios)
        {
            transicionando = true;
            RPC_IrParaFase1();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!HasStateAuthority) return;

        GameObject raiz = other.transform.root.gameObject;
        if (!raiz.CompareTag("Player")) return;

        if (jogadoresNaArea.Remove(raiz))
            jogadoresDentro = jogadoresNaArea.Count;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_IrParaFase1()
    {
        TransicaoFase.Ir(Runner, cenaFase1);
    }
}
