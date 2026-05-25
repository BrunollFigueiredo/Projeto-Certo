using Fusion;
using UnityEngine;
using System.Collections.Generic;

// Trigger no fim da fase que leva os jogadores para a próxima cena
public class Elevador : NetworkBehaviour
{
    [SerializeField] private string cenaFase2 = "Fase2";       // Nome da cena que vai carregar
    [SerializeField] private int jogadoresNecessarios = 1;     // Quantos jogadores precisam estar dentro

    private List<GameObject> jogadoresNaArea = new List<GameObject>(); // Lista de jogadores no trigger
    private bool transicionando = false;                              // Trava para não chamar duas vezes

    // Chamado quando um collider entra na área
    private void OnTriggerEnter(Collider other)
    {
        // Só o host controla a transição
        if (!HasStateAuthority) return;

        // Pega o objeto raiz e confere se é um Player
        GameObject raiz = other.transform.root.gameObject;
        if (!raiz.CompareTag("Player")) return;

        // Adiciona o jogador à lista se ainda não estiver
        if (!jogadoresNaArea.Contains(raiz))
        {
            jogadoresNaArea.Add(raiz);
        }

        // Se já tem jogadores suficientes, troca de cena
        if (!transicionando && jogadoresNaArea.Count >= jogadoresNecessarios)
        {
            transicionando = true;
            RPC_IrParaFase2();
        }
    }

    // Chamado quando um collider sai da área
    private void OnTriggerExit(Collider other)
    {
        if (!HasStateAuthority) return;

        GameObject raiz = other.transform.root.gameObject;
        if (!raiz.CompareTag("Player")) return;

        // Remove o jogador da lista
        if (jogadoresNaArea.Contains(raiz))
        {
            jogadoresNaArea.Remove(raiz);
        }
    }

    // RPC: manda todos os clientes carregarem a próxima cena
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_IrParaFase2()
    {
        TransicaoFase.Ir(Runner, cenaFase2);
    }
}
