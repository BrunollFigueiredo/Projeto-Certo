using System.Collections;
using Fusion;
using UnityEngine;

// Reage ao erro do PhraseManager: mostra aviso vermelho no lado do Aldric,
// aguarda um delay e então reseta palavras, alavancas e o progresso da frase atual.
public class ErrorFeedback : NetworkBehaviour
{
    [SerializeField] private GameObject painelErroAldric;
    [SerializeField] private float delayReset = 3f;

    private bool _processando = false;

    public override void Spawned()
    {
        PhraseManager.OnErro += HandleErro;
        if (painelErroAldric != null)
            painelErroAldric.SetActive(false);
    }

    private void OnDestroy() => PhraseManager.OnErro -= HandleErro;

    private void HandleErro()
    {
        if (_processando) return;
        StartCoroutine(SequenciaErro());
    }

    private IEnumerator SequenciaErro()
    {
        _processando = true;

        // Painel de erro: visível só para Aldric localmente
        if (painelErroAldric != null && BasicSpawner.PersonagemLocal == Personagem.Aldric)
            painelErroAldric.SetActive(true);

        yield return new WaitForSeconds(delayReset);

        // Reset de estado de rede: só StateAuthority envia os RPCs
        if (HasStateAuthority)
        {
            WordWall.Instance?.ResetarTudo();
            LeverPanel.Instance?.ResetarTodas();
            PhraseManager.Instance?.RPC_ResetarFrase();
        }

        if (painelErroAldric != null)
            painelErroAldric.SetActive(false);

        _processando = false;
    }
}
