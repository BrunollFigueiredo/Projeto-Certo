using UnityEngine;

// Agrupa todas as PalavraParede da parede do Kofi.
// ErrorFeedback chama ResetarTudo() quando uma frase quebra.
public class WordWall : MonoBehaviour
{
    public static WordWall Instance { get; private set; }

    [SerializeField] private PalavraParede[] palavras;

    private void Awake() => Instance = this;

    public void ResetarTudo()
    {
        foreach (var p in palavras)
            p.RPC_SolicitarReset();
    }
}
