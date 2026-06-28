using System;
using Fusion;
using UnityEngine;

// Ponte entre o lado do Kofi (palavras) e o lado do Aldric (alavancas).
// Quando uma palavra acende, registra no glossário e pede validação ao PhraseManager.
// Também gerencia a câmera do espelho visual (opcional).
public class MirrorWallController : NetworkBehaviour
{
    public static MirrorWallController Instance { get; private set; }

    // Disparado em todos os clientes — LeverPanel pode usar para efeitos visuais locais
    public static event Action<string> OnPalavraEspelhada;

    [Header("Espelho visual (opcional)")]
    [SerializeField] private Camera cameraMirror;
    [SerializeField] private RenderTexture texturaMirror;

    public override void Spawned()
    {
        Instance = this;
        PalavraParede.OnPalavraAcendida += HandlePalavraAcendida;

        if (cameraMirror != null && texturaMirror != null)
            cameraMirror.targetTexture = texturaMirror;
    }

    private void OnDestroy()
    {
        PalavraParede.OnPalavraAcendida -= HandlePalavraAcendida;
    }

    private void HandlePalavraAcendida(string palavraId)
    {
        OnPalavraEspelhada?.Invoke(palavraId);
        Debug.Log($"[MirrorWall] Palavra acendida: {palavraId} — enviando para PhraseManager");
        PhraseManager.Instance?.RPC_ValidarPalavra(palavraId);
    }
}
