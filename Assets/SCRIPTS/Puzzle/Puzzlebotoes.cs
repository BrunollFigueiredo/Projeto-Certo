using Fusion;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Puzzlebotoes : NetworkBehaviour
{
    public int[] sequenciaCorreta = { 2, 1, 3, 4 };
    public GameObject PortaEsquerda;
    public GameObject PortaDireita;
    public PainelBotao[] listaDeBotoes;

    private List<int> sequenciaJogador = new List<int>();
    private bool processando = false;

    public void TenteiPressionar(int idDoBotaoClicado)
    {
        if (HasStateAuthority == false) return;
        if (processando) return;
        if (sequenciaJogador.Contains(idDoBotaoClicado)) return;

        RPC_DescerBotao(idDoBotaoClicado);
        sequenciaJogador.Add(idDoBotaoClicado);

        if (sequenciaJogador.Count >= sequenciaCorreta.Length)
        {
            bool acertou = VerificarSequencia();
            processando = true;
            sequenciaJogador.Clear();
            StartCoroutine(FinalizarComDelay(acertou));
        }
    }

    private bool VerificarSequencia()
    {
        for (int i = 0; i < sequenciaCorreta.Length; i++)
        {
            if (sequenciaJogador[i] != sequenciaCorreta[i])
            {
                return false;
            }
            else
            {
                PortaEsquerda.transform.Translate(11, 0, 0);
                PortaDireita.transform.Translate(-11, 0, 0);
            }
        }

        return true;
    }

    IEnumerator FinalizarComDelay(bool acertou)
    {
        yield return new WaitForSeconds(0.5f);
        RPC_FinalizarTodos(acertou);
        processando = false;

        if (acertou)
        {
            RPC_IrParaFase2();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_IrParaFase2()
    {
        TransicaoFase.Ir(Runner, "Fase2");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_DescerBotao(int id)
    {
        foreach (PainelBotao btn in listaDeBotoes)
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
        Color cor;
        bool resetarCor;

        if (acertou)
        {
            cor = Color.green;
            resetarCor = false;
        }
        else
        {
            cor = Color.red;
            resetarCor = true;
        }

        foreach (PainelBotao btn in listaDeBotoes)
        {
            if (btn != null)
            {
                btn.SubirComCor(cor, resetarCor);
            }
        }
    }
}
