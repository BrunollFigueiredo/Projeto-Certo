using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// Cronômetro regressivo sincronizado. Muda de cor conforme o tempo diminui.
public class PhaseTimer : NetworkBehaviour
{
    [SerializeField] private float tempoTotal = 180f;
    [SerializeField] private TextMeshProUGUI textoTimer;
    [SerializeField] private UnityEvent OnGameOver;

    [Networked] private float tempoRestante { get; set; }
    [Networked] private NetworkBool rodando { get; set; }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            tempoRestante = tempoTotal;
            rodando = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        // Começa automaticamente quando a cutscene terminar
        if (!rodando && !CutsceneFase1.Ativa && tempoRestante > 0f)
        {
            rodando = true;
        }

        if (!rodando) return;

        tempoRestante -= Runner.DeltaTime;
        if (tempoRestante <= 0f)
        {
            tempoRestante = 0f;
            rodando = false;
            RPC_GameOver();
        }
    }

    public override void Render() => AtualizarTexto();

    public void Parar()
    {
        if (HasStateAuthority) rodando = false;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_GameOver() => OnGameOver?.Invoke();

    private void AtualizarTexto()
    {
        if (textoTimer == null) return;

        int s = Mathf.CeilToInt(tempoRestante);
        textoTimer.text = $"{s / 60:00}:{s % 60:00}";

        float t = tempoRestante / tempoTotal;
        textoTimer.color = t > 0.5f ? Color.green :
                           t > 0.25f ? Color.yellow : Color.red;
    }
}
