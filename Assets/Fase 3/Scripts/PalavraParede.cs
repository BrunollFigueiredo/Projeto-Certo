using System;
using Fusion;
using TMPro;
using UnityEngine;

public class PalavraParede : NetworkBehaviour
{
    [Header("Visual")]
    [SerializeField] private TextMeshPro texto;
    [SerializeField] private Color corApagada = new Color(1f, 1f, 1f, 0.25f);
    [SerializeField] private Color corAcesa = new Color(1f, 0.85f, 0.1f);
    [SerializeField] private Color corOutlineAcesa = new Color(1f, 0.5f, 0f);
    [SerializeField] [Range(0f, 1f)] private float espessuraOutline = 0.3f;

    [Header("Luz pontual (opcional)")]
    [SerializeField] private Light luzPontual;

    [Header("Sistema de Frases")]
    [SerializeField] public string palavraId;

    // Disparado em todos os clientes quando a palavra acende
    public static event Action<string> OnPalavraAcendida;

    [Networked] private NetworkBool Acesa { get; set; }

    private ChangeDetector _changes;
    private bool _playerJaSpawnou = false;

    public override void Spawned()
    {
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
        AtualizarVisual();
    }

    public override void Render()
    {
        foreach (var change in _changes.DetectChanges(this))
        {
            if (change == nameof(Acesa))
            {
                Debug.Log($"[PalavraParede] Acesa mudou para {Acesa} na palavra {palavraId}");
                AtualizarVisual();
                if (Acesa)
                {
                    Debug.Log($"[PalavraParede] Disparando OnPalavraAcendida — subscribers: {OnPalavraAcendida?.GetInvocationList().Length ?? 0}");
                    OnPalavraAcendida?.Invoke(palavraId);
                }
            }
        }

        if (Player.LocalSpawnou && !_playerJaSpawnou)
        {
            _playerJaSpawnou = true;
            AtualizarVisual();
        }
    }

    private void OnMouseDown()
    {
        if (Acesa) return;

        if (BasicSpawner.PersonagemLocal != Personagem.Kofi)
        {
            FeedbackUI.Mostrar("Você não consegue entender essa escrita.");
            return;
        }

        // Bloqueia novo toque enquanto Aldric ainda não confirmou a palavra atual
        if (PhraseManager.Instance != null && PhraseManager.Instance.AguardandoAlavanca)
        {
            FeedbackUI.Mostrar("Aguarde Aldric confirmar a palavra atual.");
            return;
        }

        Debug.Log($"[PalavraParede] Clicou na palavra id={palavraId}");
        RPC_Acender();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_Acender()
    {
        if (!Acesa)
            Acesa = true;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SolicitarReset()
    {
        Acesa = false;
    }

    private void AtualizarVisual()
    {
        if (texto != null)
        {
            if (Acesa)
            {
                texto.color = corAcesa;
                texto.outlineWidth = espessuraOutline;
                texto.outlineColor = corOutlineAcesa;
            }
            else if (BasicSpawner.PersonagemLocal == Personagem.Kofi)
            {
                texto.color = corApagada;
                texto.outlineWidth = 0f;
                texto.outlineColor = Color.clear;
            }
            else
            {
                texto.color = Color.clear;
                texto.outlineWidth = 0f;
                texto.outlineColor = Color.clear;
            }
        }

        if (luzPontual != null)
        {
            luzPontual.enabled = Acesa;
            luzPontual.color = corAcesa;
        }
    }
}
