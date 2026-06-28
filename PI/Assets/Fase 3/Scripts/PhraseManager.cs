using System;
using Fusion;
using UnityEngine;

[Serializable]
public class FraseAlvo
{
    public string[] palavras; // sequência esperada, em ordem
}

// Valida a montagem das frases palavra por palavra.
// Kofi toca a palavra → validação aqui → se correta, ativa a alavanca do Aldric.
// Se errada, dispara OnErro. Quando Aldric completa a alavanca, avança a frase.
public class PhraseManager : NetworkBehaviour
{
    public static PhraseManager Instance { get; private set; }

    [SerializeField] private FraseAlvo[] frases;

    [Networked] private int fraseAtual { get; set; }
    [Networked] private int palavraAtual { get; set; }
    [Networked] private NetworkBool _aguardandoAlavanca { get; set; }

    // Leitura pública para PalavraParede bloquear toque duplo
    public bool AguardandoAlavanca => _aguardandoAlavanca;

    public static event Action OnFraseCompleta;
    public static event Action OnErro;

    public override void Spawned()
    {
        Instance = this;
        Lever.OnCompleta += HandleAlavancaCompleta;
    }

    private void OnDestroy()
    {
        Lever.OnCompleta -= HandleAlavancaCompleta;
    }

    // Chamado pelo MirrorWallController quando uma palavra acende
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ValidarPalavra(string palavraId)
    {
        if (_aguardandoAlavanca || fraseAtual >= frases.Length) return;

        string esperada = frases[fraseAtual].palavras[palavraAtual];

        Debug.Log($"[PhraseManager] Validando: recebido={palavraId} esperado={esperada}");
        if (palavraId == esperada)
        {
            Debug.Log($"[PhraseManager] CORRETO — ativando alavanca {palavraId}");
            _aguardandoAlavanca = true;
            LeverPanel.Instance?.RPC_AtivarPorPalavra(palavraId);
        }
        else
        {
            Debug.Log($"[PhraseManager] ERRADO — disparando erro");
            RPC_DispararErro();
        }
    }

    // Chamado pelo evento Lever.OnCompleta (dispara em todos os clientes)
    private void HandleAlavancaCompleta(string palavraId)
    {
        if (!HasStateAuthority || !_aguardandoAlavanca) return;
        if (fraseAtual >= frases.Length) return;

        string esperada = frases[fraseAtual].palavras[palavraAtual];
        if (palavraId != esperada) return;

        palavraAtual++;
        _aguardandoAlavanca = false;

        if (palavraAtual >= frases[fraseAtual].palavras.Length)
        {
            fraseAtual++;
            palavraAtual = 0;
            RPC_DispararFraseCompleta();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ResetarFrase()
    {
        palavraAtual = 0;
        _aguardandoAlavanca = false;
        // fraseAtual não reseta — progresso de frases já completas é mantido
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_DispararFraseCompleta() => OnFraseCompleta?.Invoke();

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_DispararErro() => OnErro?.Invoke();
}
