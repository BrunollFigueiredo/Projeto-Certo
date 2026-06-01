using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

// Mostra a cutscene de introdução da Fase 1 sincronizada entre os jogadores
public class CutsceneFase1 : NetworkBehaviour
{
    // Representa um slide de texto da cutscene
    [System.Serializable]
    public class Slide
    {
        [TextArea(4, 12)]
        public string texto;
    }

    // Lista de slides que vão ser mostrados em ordem
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

    [SerializeField] private GameObject painelCutscene;          // Painel preto com o texto da narração
    [SerializeField] private TextMeshProUGUI textoNarrador;      // Texto que aparece letra por letra
    [SerializeField] private Button botaoProximo;                // Botão para avançar o slide
    [SerializeField] private TextMeshProUGUI textoBotaoProximo;  // Texto dentro do botão (Próximo/Começar)
    [SerializeField] private Camera cameraCutscene;              // Câmera fixa usada durante a cutscene
    [SerializeField] private GameObject[] controlesMobile;       // Joystick e botão de pulo (ficam ocultos)
    [SerializeField] [Range(0.01f, 0.1f)] private float velocidadeDigitacao = 0.03f; // Tempo entre cada letra

    [Networked] private int slideAtual { get; set; }  // Slide atual sincronizado entre os jogadores

    public static bool Ativa { get; private set; } = false; // Flag global para outros scripts saberem que a cutscene está rolando

    private ChangeDetector _changes;            // Detecta mudanças nas variáveis [Networked]
    private Coroutine _coroutineDigitacao;      // Referência da animação de digitar atual
    private int _ultimoSlideExibido = -1;       // Evita exibir o mesmo slide duas vezes

    private void Awake()
    {
        Ativa = true;
    }

    private void Start()
    {
        // Inicia a UI imediatamente sem esperar o Fusion spawnar o objeto
        painelCutscene.SetActive(true);
        botaoProximo.gameObject.SetActive(false);

        for (int i = 0; i < controlesMobile.Length; i++)
            if (controlesMobile[i] != null)
                controlesMobile[i].SetActive(false);

        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam != cameraCutscene)
            mainCam.gameObject.SetActive(false);

        if (cameraCutscene != null)
            cameraCutscene.gameObject.SetActive(true);

        MostrarSlideLocal(0);
    }

    public override void Spawned()
    {
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (HasStateAuthority)
            slideAtual = 0;

        // Só sincroniza se o cliente entrou tarde (já está em um slide além do 0)
        if (slideAtual > 0)
            MostrarSlideLocal(slideAtual);
    }

    public override void Render()
    {
        // Quando o slide muda no host, atualiza nos clientes
        foreach (var change in _changes.DetectChanges(this))
        {
            if (change == nameof(slideAtual))
            {
                MostrarSlideLocal(slideAtual);
            }
        }
    }

    // Mostra um slide específico na tela local
    void MostrarSlideLocal(int index)
    {
        // Não repete o mesmo slide
        if (index == _ultimoSlideExibido) return;
        _ultimoSlideExibido = index;

        // Cancela a animação anterior se ainda estiver rodando
        if (_coroutineDigitacao != null)
        {
            StopCoroutine(_coroutineDigitacao);
        }

        botaoProximo.gameObject.SetActive(false);
        textoNarrador.text = "";

        // Inicia a animação de digitar o texto do slide
        if (index < slides.Length)
        {
            _coroutineDigitacao = StartCoroutine(Digitar(slides[index].texto, index));
        }
    }

    // Animação que faz o texto aparecer letra por letra
    IEnumerator Digitar(string texto, int indexSlide)
    {
        for (int i = 0; i < texto.Length; i++)
        {
            textoNarrador.text += texto[i];
            yield return new WaitForSeconds(velocidadeDigitacao);
        }

        // Quando termina, mostra o botão de avançar com o texto certo
        bool ultimo = indexSlide >= slides.Length - 1;

        if (textoBotaoProximo != null)
        {
            if (ultimo)
            {
                textoBotaoProximo.text = "Começar";
            }
            else
            {
                textoBotaoProximo.text = "Próximo >";
            }
        }

        botaoProximo.gameObject.SetActive(true);
    }

    // Chamado pelo botão Próximo/Começar
    public void AvancarSlide()
    {
        botaoProximo.gameObject.SetActive(false);

        // O host avança direto, os clientes pedem via RPC
        if (HasStateAuthority)
        {
            ProcessarAvanco();
        }
        else
        {
            RPC_PedirAvanco();
        }
    }

    // RPC: cliente pede ao host para avançar o slide
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_PedirAvanco()
    {
        ProcessarAvanco();
    }

    // Avança o slide ou termina a cutscene se for o último
    void ProcessarAvanco()
    {
        int proximo = slideAtual + 1;

        if (proximo >= slides.Length)
        {
            RPC_TerminarCutscene();
        }
        else
        {
            slideAtual = proximo;
        }
    }

    // RPC: avisa todos para encerrar a cutscene
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_TerminarCutscene()
    {
        TerminarCutsceneLocal();
    }

    // Encerra a cutscene na tela local (esconde painel, religa controles e câmera do jogador)
    void TerminarCutsceneLocal()
    {
        Ativa = false;
        painelCutscene.SetActive(false);

        // Desliga a câmera da cutscene
        if (cameraCutscene != null)
        {
            cameraCutscene.gameObject.SetActive(false);
        }

        // Religa os controles mobile
        for (int i = 0; i < controlesMobile.Length; i++)
        {
            if (controlesMobile[i] != null)
            {
                controlesMobile[i].SetActive(true);
            }
        }

        // Ativa as câmeras dos personagens
        Player.AtivarCamerasJogadores();
    }

    private void OnDestroy()
    {
        // Garante que a flag fique falsa quando a cena terminar
        Ativa = false;
    }
}
