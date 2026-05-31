using System;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

// Alavanca do lado do Aldric.
// Aldric pressiona e segura — a barra enche em tempoParaCompletar segundos.
// Se soltar antes, a barra reseta. Quando completa, notifica o PhraseManager.
public class Lever : NetworkBehaviour
{
    public enum Estado { Inativa, Ativa, SendoSegura, Completa }

    [SerializeField] public string palavraAssociada;
    [SerializeField] private float tempoParaCompletar = 2f;

    [Header("Visual")]
    [SerializeField] private Renderer rendererAlavanca;
    [SerializeField] private Color corInativa = Color.gray;
    [SerializeField] private Color corAtiva = Color.yellow;
    [SerializeField] private Color corCompleta = Color.green;

    [Header("Animação — engrenagem")]
    [SerializeField] private Transform engrenagem;
    [SerializeField] private float velocidadeEngrenagem = 360f;

    [Header("UI — barra de progresso")]
    [SerializeField] private Image barraProgresso;

    [Networked] public Estado EstadoAtual { get; private set; }
    [Networked] private float Progresso { get; set; }

    // Dispara em todos os clientes quando a alavanca é completada
    public static event Action<string> OnCompleta;

    private ChangeDetector _changes;

    public override void Spawned()
    {
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
        AtualizarVisual();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || EstadoAtual != Estado.SendoSegura) return;

        Progresso = Mathf.Min(1f, Progresso + Runner.DeltaTime / tempoParaCompletar);
        if (Progresso >= 1f)
        {
            EstadoAtual = Estado.Completa;
            RPC_NotificarCompleta();
        }
    }

    public override void Render()
    {
        if (barraProgresso != null)
            barraProgresso.fillAmount = Progresso;

        if (engrenagem != null && EstadoAtual == Estado.SendoSegura)
            engrenagem.Rotate(velocidadeEngrenagem * Time.deltaTime, 0f, 0f);

        foreach (var change in _changes.DetectChanges(this))
        {
            if (change == nameof(EstadoAtual))
                AtualizarVisual();
        }
    }

    private void OnMouseDown()
    {
        if (EstadoAtual == Estado.Ativa)
            RPC_IniciarSegura();
    }

    private void OnMouseUp()
    {
        if (EstadoAtual == Estado.SendoSegura)
            RPC_Soltar();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_Ativar()
    {
        if (EstadoAtual == Estado.Inativa)
        {
            EstadoAtual = Estado.Ativa;
            Progresso = 0f;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_Desativar()
    {
        if (EstadoAtual != Estado.Completa)
        {
            EstadoAtual = Estado.Inativa;
            Progresso = 0f;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_Resetar()
    {
        EstadoAtual = Estado.Inativa;
        Progresso = 0f;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_IniciarSegura()
    {
        if (EstadoAtual == Estado.Ativa)
            EstadoAtual = Estado.SendoSegura;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_Soltar()
    {
        if (EstadoAtual == Estado.SendoSegura)
        {
            EstadoAtual = Estado.Ativa;
            Progresso = 0f;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_NotificarCompleta() => OnCompleta?.Invoke(palavraAssociada);

    private void AtualizarVisual()
    {
        if (rendererAlavanca == null) return;
        rendererAlavanca.material.color = EstadoAtual switch
        {
            Estado.Ativa or Estado.SendoSegura => corAtiva,
            Estado.Completa                    => corCompleta,
            _                                  => corInativa
        };
    }
}
