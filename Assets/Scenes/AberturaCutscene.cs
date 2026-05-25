using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CutsceneAbertura : MonoBehaviour
{
    [System.Serializable]
    public class Slide
    {
        [TextArea(4, 12)]
        public string texto;
    }

    [SerializeField]
    private Slide[] slides = new Slide[]
    {
        new Slide { texto = "Por gerações, dois reinos dividiram o mesmo continente." },
        new Slide { texto = "Umbra, de tradição comunitária, guiada por anciãos e pelo conhecimento transmitido pela fala. Auris, um império hierárquico, com instituições centralizadas e o hábito de registrar tudo que considerava relevante." },
        new Slide { texto = "Entre os dois, a guerra. Auris venceu. A terra de Umbra foi dividida. Metade permaneceu. A outra metade passou ao Império, junto com tudo que havia nela." },
        new Slide { texto = "A paz chegou no papel. O ressentimento ficou." },
        new Slide { texto = "Décadas depois, uma Torre surgiu entre os dois reinos. Ninguém sabe quem a construiu. Ninguém viu o início da obra. Um dia ela simplesmente estava lá." },
        new Slide { texto = "Um mensageiro chegou aos dois reinos com a mesma proposta. Qualquer candidato que chegasse ao topo poderia negociar benefícios para o próprio povo." },
        new Slide { texto = "Umbra escolheu Kofi. Auris escolheu Aldric. Os dois cruzaram a porta da Torre carregando o peso da história dos próprios povos." },
        new Slide { texto = "'Candidatos. Meu nome não importa, mas podem me chamar de Arquiteto.' 'Fui eu quem trouxe vocês até aqui.'" },
        new Slide { texto = "'E serei eu quem vai acompanhar cada passo de vocês até o topo. Se é que chegarem lá.'" },
    };

    [SerializeField] private GameObject painelCutscene;
    [SerializeField] private TextMeshProUGUI textoNarrador;
    [SerializeField] private Button botaoProximo;
    [SerializeField] private TextMeshProUGUI textoBotaoProximo;
    [SerializeField] private Camera cameraCutscene;
    [SerializeField] private string nomeCenaDestino = "Escolha Jogador";
    [SerializeField][Range(0.01f, 0.1f)] private float velocidadeDigitacao = 0.03f;

    private int slideAtual = 0;
    private Coroutine _coroutineDigitacao;
    private bool podeAvancar = false; // só avança quando o texto terminou de digitar

    public static bool Ativa { get; private set; } = false;

    private void Start()
    {
        Ativa = true;
        painelCutscene.SetActive(true);

        if (botaoProximo != null)
            botaoProximo.gameObject.SetActive(false);

        if (cameraCutscene != null)
            cameraCutscene.gameObject.SetActive(true);

        MostrarSlide(slideAtual);
    }

    private void Update()
    {
        // Toque ou clique em qualquer lugar da tela avança o slide
        if (podeAvancar && (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)))
        {
            podeAvancar = false;
            AvancarSlide();
        }
    }

    void MostrarSlide(int index)
    {
        podeAvancar = false;

        if (_coroutineDigitacao != null)
            StopCoroutine(_coroutineDigitacao);

        if (botaoProximo != null)
            botaoProximo.gameObject.SetActive(false);

        textoNarrador.text = "";

        if (index < slides.Length)
            _coroutineDigitacao = StartCoroutine(Digitar(slides[index].texto, index));
    }

    IEnumerator Digitar(string texto, int indexSlide)
    {
        for (int i = 0; i < texto.Length; i++)
        {
            textoNarrador.text += texto[i];
            yield return new WaitForSeconds(velocidadeDigitacao);
        }

        bool ultimo = indexSlide >= slides.Length - 1;

        if (textoBotaoProximo != null)
            textoBotaoProximo.text = ultimo ? "Começar" : "Próximo >";

        if (botaoProximo != null)
            botaoProximo.gameObject.SetActive(true);

        // Libera o avanço por toque após o texto terminar
        podeAvancar = true;
    }

    public void AvancarSlide()
    {
        if (botaoProximo != null)
            botaoProximo.gameObject.SetActive(false);

        int proximo = slideAtual + 1;

        if (proximo >= slides.Length)
        {
            TerminarCutscene();
        }
        else
        {
            slideAtual = proximo;
            MostrarSlide(slideAtual);
        }
    }

    void TerminarCutscene()
    {
        Ativa = false;
        painelCutscene.SetActive(false);

        if (cameraCutscene != null)
            cameraCutscene.gameObject.SetActive(false);

        SceneManager.LoadScene(nomeCenaDestino);
    }

    private void OnDestroy()
    {
        Ativa = false;
    }
}