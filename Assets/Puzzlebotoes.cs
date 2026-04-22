using Fusion;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Puzzlebotoes : NetworkBehaviour
{
    public int[] sequenciaCorreta = { 2, 1, 3, 4 };

    public PainelBotao[] listaDeBotoes;

    private List<int> sequenciaJogador = new List<int>();
    private bool processando = false;

    public void TenteiPressionar(int idDoBotaoClicado)
    {
        if (HasStateAuthority == false) return;
        if (processando) return;

        // Ignora se este botão já foi pressionado nesta rodada
        if (sequenciaJogador.Contains(idDoBotaoClicado)) return;

        Debug.Log("<color=blue>Toque detectado no Botão ID: </color>" + idDoBotaoClicado);

        RPC_DescerBotao(idDoBotaoClicado);
        sequenciaJogador.Add(idDoBotaoClicado);

        if (sequenciaJogador.Count >= sequenciaCorreta.Length)
        {
            bool acertou = VerificarSequencia();
            processando = true;
            sequenciaJogador.Clear();

            if (acertou)
                Debug.Log("<color=gold>PUZZLE RESOLVIDO!</color>");
            else
                Debug.Log("<color=red>SEQUÊNCIA ERRADA! Resetando...</color>");

            StartCoroutine(FinalizarComDelay(acertou));
        }
    }

    private bool VerificarSequencia()
    {
        for (int i = 0; i < sequenciaCorreta.Length; i++)
        {
            if (sequenciaJogador[i] != sequenciaCorreta[i])
                return false;
        }
        return true;
    }

    IEnumerator FinalizarComDelay(bool acertou)
    {
        yield return new WaitForSeconds(0.5f);
        RPC_FinalizarTodos(acertou);
        processando = false;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_DescerBotao(int id)
    {
        foreach (var btn in listaDeBotoes)
        {
            if (btn != null && btn.idBotao == id)
            {
                btn.Descer();
                break;
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_FinalizarTodos(bool acertou)
    {
        Color cor = acertou ? Color.green : Color.red;
        bool resetarCor = !acertou;

        foreach (var btn in listaDeBotoes)
        {
            if (btn != null)
                btn.SubirComCor(cor, resetarCor);
        }
    }
}
