using Fusion;
using UnityEngine;

// Gerencia o conjunto de alavancas do lado do Aldric.
// Ativa a alavanca correspondente à palavra validada; desativa as demais.
public class LeverPanel : NetworkBehaviour
{
    public static LeverPanel Instance { get; private set; }

    [SerializeField] private Lever[] alavancas;

    public override void Spawned() => Instance = this;

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_AtivarPorPalavra(string palavraId)
    {
        foreach (var alavanca in alavancas)
        {
            if (alavanca.palavraAssociada == palavraId)
                alavanca.RPC_Ativar();
            else if (alavanca.EstadoAtual != Lever.Estado.Completa)
                alavanca.RPC_Desativar();
        }
    }

    public void ResetarTodas()
    {
        foreach (var alavanca in alavancas)
            alavanca.RPC_Resetar();
    }
}
