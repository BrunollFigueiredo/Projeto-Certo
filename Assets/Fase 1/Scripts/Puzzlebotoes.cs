using Fusion;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Controla o puzzle de sequência de botões e abre a porta quando acertam
public class Puzzlebotoes : NetworkBehaviour
{
    public int[] sequenciaCorreta = { 2, 1, 3, 4 }; // Ordem correta dos botões
    public GameObject Porta;                         // A porta que vai abrir ao acertar
    public PainelBotao[] listaDeBotoes;              // Todos os botões do painel

    private List<int> sequenciaJogador = new List<int>(); // O que o jogador apertou até agora
    private bool processando = false;                     // Trava enquanto verifica o resultado

    // Chamado quando um botão é pressionado pelo jogador
    public void TenteiPressionar(int idDoBotaoClicado)
    {
        // Só o jogador com autoridade processa a lógica
        if (!HasStateAuthority) return;
        if (processando) return;
        // Ignora se já apertou esse botão antes
        if (sequenciaJogador.Contains(idDoBotaoClicado)) return;

        // Manda o botão descer em todos os clientes e guarda na lista
        RPC_DescerBotao(idDoBotaoClicado);
        sequenciaJogador.Add(idDoBotaoClicado);

        // Quando completou a sequência, verifica se acertou
        if (sequenciaJogador.Count >= sequenciaCorreta.Length)
        {
            bool acertou = VerificarSequencia();
            processando = true;
            sequenciaJogador.Clear();
            StartCoroutine(FinalizarComDelay(acertou));
        }
    }

    // Confere se a sequência do jogador é igual à correta
    private bool VerificarSequencia()
    {
        for (int i = 0; i < sequenciaCorreta.Length; i++)
        {
            if (sequenciaJogador[i] != sequenciaCorreta[i])
            {
                return false;
            }
        }

        return true;
    }

    // Espera meio segundo antes de mostrar o resultado nos botões
    IEnumerator FinalizarComDelay(bool acertou)
    {
        yield return new WaitForSeconds(0.5f);
        RPC_FinalizarTodos(acertou);
        processando = false;
    }

    // RPC: faz o botão descer em todos os clientes
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_DescerBotao(int id)
    {
        // Procura o botão com o id certo e manda descer
        for (int i = 0; i < listaDeBotoes.Length; i++)
        {
            PainelBotao btn = listaDeBotoes[i];
            if (btn != null && btn.idBotao == id)
            {
                btn.Descer();
                break;
            }
        }
    }

    // RPC: aplica o resultado em todos os clientes (sobe botões e abre porta se acertou)
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_FinalizarTodos(bool acertou)
    {
        Color cor;
        bool resetarCor;

        if (acertou)
        {
            // Verde e abre a porta movendo para Z = -9.5
            cor = Color.green;
            resetarCor = false;

            if (Porta != null)
            {
                Vector3 pos = Porta.transform.position;
                pos.y = 27f;
                Porta.transform.position = pos;
            }
        }
        else
        {
            // Vermelho e depois volta para a cor original
            cor = Color.red;
            resetarCor = true;
        }

        // Faz todos os botões subirem com a cor escolhida
        for (int i = 0; i < listaDeBotoes.Length; i++)
        {
            PainelBotao btn = listaDeBotoes[i];
            if (btn != null)
            {
                btn.SubirComCor(cor, resetarCor);
            }
        }
    }
}
