using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// Conta frases completas. Quando bate o número alvo, dispara OnVitoria.
public class PhaseProgressTracker : NetworkBehaviour
{
    [SerializeField] private int frasesParaVencer = 2;
    [SerializeField] private TextMeshProUGUI textoProgresso;
    [SerializeField] private UnityEvent OnVitoria;

    [Networked] private int frasesCompletas { get; set; }

    private ChangeDetector _changes;

    public override void Spawned()
    {
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
        PhraseManager.OnFraseCompleta += HandleFraseCompleta;
        AtualizarTexto();
    }

    private void OnDestroy() => PhraseManager.OnFraseCompleta -= HandleFraseCompleta;

    private void HandleFraseCompleta()
    {
        if (!HasStateAuthority) return;
        frasesCompletas++;
        if (frasesCompletas >= frasesParaVencer)
            RPC_Vitoria();
    }

    public override void Render()
    {
        foreach (var change in _changes.DetectChanges(this))
            if (change == nameof(frasesCompletas))
                AtualizarTexto();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_Vitoria()
    {
        Debug.Log("[PhaseProgressTracker] VITÓRIA — todas as frases completas!");
        OnVitoria?.Invoke();
    }

    private void AtualizarTexto()
    {
        if (textoProgresso != null)
            textoProgresso.text = $"{frasesCompletas}/{frasesParaVencer}";
    }
}
