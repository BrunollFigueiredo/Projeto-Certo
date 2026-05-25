using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class CutsceneFase1 : NetworkBehaviour
{
    [System.Serializable]
    public class Slide
    {
        [TextArea(4, 12)]
        public string texto;
    }

    [Header("Conteúdo")]
    [SerializeField] private Slide[] slides = new Slide[]
    {
        new Slide { texto = "— Boas-vindas —\n\nCandidatos.\n\nSejam bem-vindos à Torre." },
        new Slide { texto = "Vocês foram escolhidos por seus respectivos reinos para estar aqui. Essa escolha não foi acidente. Foi julgamento. E o julgamento, como tudo nesta Torre, seguirá critérios precisos." },
        new Slide { texto = "A recompensa aguarda no topo. Para chegar até ela, será necessário atravessar cada andar. Um por um. Juntos." },
        new Slide { texto = "Esta Torre foi construída para ser vencida pela cooperação. O que um não alcança, o outro alcança. O que um não enxerga, o outro enxerga. Guardem isso." },
        new Slide { texto = "O cronômetro não espera. Vamos começar." },
        new Slide { texto = "— Fase 1: Sala das Sequências —\nÀ sua frente, um portão. Ele não abrirá pela força. Não abrirá pelo tempo. Abrirá apenas pela sequência correta." },
        new Slide { texto = "Nesta sala há dois elementos. Uma biblioteca, com tomos que registram a história deste continente. E um painel, com botões que aguardam acionamento na ordem certa." },
        new Slide { texto = "As pistas estão nos tomos. A ação está no painel. Nenhum de vocês pode fazer os dois." },
        new Slide { texto = "Encontrem a sequência. Acionem o painel. O portão se abrirá.\n\nO tempo começa agora." },
    };

    [Header("UI")]
    [SerializeField] private GameObject painelCutscene;
    [SerializeField] private TextMeshProUGUI textoNarrador;
    [SerializeField] private Button botaoProximo;
    [SerializeField] private TextMeshProUGUI textoBotaoProximo;

    [Header("Câmera")]
    [SerializeField] private Camera cameraCutscene;

    [Header("Controles (desativados durante a cutscene)")]
    [SerializeField] private GameObject[] controlesMobile;

    [Header("Configuração")]
    [SerializeField] [Range(0.01f, 0.1f)] private float velocidadeDigitacao = 0.03f;

    [Networked] private int slideAtual { get; set; }

    public static bool Ativa { get; private set; } = false;

    private ChangeDetector _changes;
    private Coroutine _coroutineDigitacao;
    private int _ultimoSlideExibido = -1;

    private void Awake()
    {
        Ativa = true;
    }

    public override void Spawned()
    {
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (HasStateAuthority)
            slideAtual = 0;

        painelCutscene.SetActive(true);
        botaoProximo.gameObject.SetActive(false);

        foreach (var controle in controlesMobile)
            if (controle != null) controle.SetActive(false);

        // Desliga a câmera principal da cena para não renderizar junto com a cutscene
        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam != cameraCutscene)
            mainCam.gameObject.SetActive(false);

        if (cameraCutscene != null)
            cameraCutscene.gameObject.SetActive(true);

        MostrarSlideLocal(slideAtual);
    }

    public override void Render()
    {
        foreach (var change in _changes.DetectChanges(this))
        {
            if (change == nameof(slideAtual))
                MostrarSlideLocal(slideAtual);
        }
    }

    void MostrarSlideLocal(int index)
    {
        if (index == _ultimoSlideExibido) return;
        _ultimoSlideExibido = index;

        if (_coroutineDigitacao != null)
            StopCoroutine(_coroutineDigitacao);

        botaoProximo.gameObject.SetActive(false);
        textoNarrador.text = "";

        if (index < slides.Length)
            _coroutineDigitacao = StartCoroutine(Digitar(slides[index].texto, index));
    }

    IEnumerator Digitar(string texto, int indexSlide)
    {
        foreach (char c in texto)
        {
            textoNarrador.text += c;
            yield return new WaitForSeconds(velocidadeDigitacao);
        }

        bool ultimo = indexSlide >= slides.Length - 1;
        if (textoBotaoProximo != null)
            textoBotaoProximo.text = ultimo ? "Começar" : "Próximo >";

        botaoProximo.gameObject.SetActive(true);
    }

    public void AvancarSlide()
    {
        botaoProximo.gameObject.SetActive(false);

        if (HasStateAuthority)
            ProcessarAvanco();
        else
            RPC_PedirAvanco();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_PedirAvanco() => ProcessarAvanco();

    void ProcessarAvanco()
    {
        int proximo = slideAtual + 1;
        if (proximo >= slides.Length)
            RPC_TerminarCutscene();
        else
            slideAtual = proximo;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_TerminarCutscene()
    {
        TerminarCutsceneLocal();
    }

    void TerminarCutsceneLocal()
    {
        Ativa = false;
        painelCutscene.SetActive(false);

        if (cameraCutscene != null)
            cameraCutscene.gameObject.SetActive(false);

        Debug.Log($"[Cutscene] Reativando {controlesMobile.Length} controles.");
        foreach (var controle in controlesMobile)
        {
            if (controle != null)
            {
                controle.SetActive(true);
                Debug.Log($"[Cutscene] Reativou: {controle.name}");
            }
        }

        Player.AtivarCamerasJogadores();

        // TODO: Inicie o timer aqui quando ele existir
        // Exemplo: GerenciadorTempo.Instance?.IniciarTimer();
    }

    private void OnDestroy()
    {
        Ativa = false;
    }
}
