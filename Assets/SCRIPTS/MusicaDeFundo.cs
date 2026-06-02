using UnityEngine;
using UnityEngine.SceneManagement;

// Música de fundo que continua tocando entre as cenas, sem reiniciar.
// Mas fica em SILÊNCIO durante as fases (a música é só pra menus/cutscenes).
// Coloque este componente em UM GameObject que tenha um AudioSource.
[RequireComponent(typeof(AudioSource))]
public class MusicaDeFundo : MonoBehaviour
{
    // Nomes das cenas onde a música NÃO deve tocar (as fases).
    [SerializeField]
    private string[] cenasSemMusica = { "Fase1", "Fase2", "Fase3", "Fase4" };

    // Guarda a única música ativa. Se nascer outra cópia, ela se apaga.
    private static MusicaDeFundo instancia;

    private AudioSource audioSource;

    private void Awake()
    {
        // Se já tem uma música tocando, destrói esta cópia pra não reiniciar.
        if (instancia != null)
        {
            Destroy(gameObject);
            return;
        }

        // Esta vira a música oficial e sobrevive à troca de cenas.
        instancia = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;

        // Aplica já pra cena atual (toca ou fica em silêncio)
        AplicarParaCena(SceneManager.GetActiveScene().name);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += AoCarregarCena;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AoCarregarCena;
    }

    // Chamado toda vez que uma cena nova carrega
    private void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        AplicarParaCena(cena.name);
    }

    // Pausa a música nas fases, toca nas outras cenas
    private void AplicarParaCena(string nomeDaCena)
    {
        if (EhCenaDeFase(nomeDaCena))
        {
            // Numa fase: pausa (guarda o ponto pra voltar depois)
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }
        else
        {
            // Menu/cutscene: continua de onde parou
            if (!audioSource.isPlaying)
            {
                audioSource.UnPause();
                if (!audioSource.isPlaying) audioSource.Play();
            }
        }
    }

    // Confere se a cena está na lista das que ficam sem música
    private bool EhCenaDeFase(string nomeDaCena)
    {
        for (int i = 0; i < cenasSemMusica.Length; i++)
        {
            if (cenasSemMusica[i] == nomeDaCena) return true;
        }
        return false;
    }

    // Troca a faixa que está tocando (ex.: música do menu -> outra música).
    public static void TrocarMusica(AudioClip nova)
    {
        if (instancia == null || nova == null) return;
        if (instancia.audioSource.clip == nova) return;

        instancia.audioSource.clip = nova;
        instancia.audioSource.Play();
    }

    // Ajusta o volume da música (0 a 1).
    public static void DefinirVolume(float volume)
    {
        if (instancia == null) return;
        instancia.audioSource.volume = Mathf.Clamp01(volume);
    }
}
