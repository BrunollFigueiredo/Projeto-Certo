using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// Painel touchscreen para escolher escala, confirmar prensagem e enviar pela esteira
public class PainelPrensa : MonoBehaviour
{
    [SerializeField] private Prensa prensa;                       // Referência à prensa controlada

    [SerializeField] private GameObject painelUI;                 // UI que aparece ao tocar no painel
    [SerializeField] private TextMeshProUGUI textoEscalaSelecionada; // Mostra a escala escolhida
    [SerializeField] private Button botaoConfirmar;               // Botão para acionar a prensa
    [SerializeField] private Button botaoEnviar;                  // Botão para mandar o objeto na esteira

    [SerializeField] private float velocidadeEsteira = 4f;        // Velocidade do empurrão na esteira
    [SerializeField] private Transform destino;                   // Para onde o objeto é lançado

    [SerializeField] private Vector3 escalaPequena = new Vector3(0.5f, 0.5f, 0.5f); // Escala "P"
    [SerializeField] private Vector3 escalaMedia = new Vector3(1f, 1f, 1f);         // Escala "M"
    [SerializeField] private Vector3 escalaGrande = new Vector3(1.5f, 1.5f, 1.5f);  // Escala "G"

    private Vector3 escalaEscolhida;  // Escala atualmente selecionada
    private bool uiAberta = false;    // Se o painel UI está aberto
    private Camera cam;               // Câmera usada para o raycast do toque

    void Start()
    {
        // Começa com a escala média selecionada e o painel fechado
        escalaEscolhida = escalaMedia;
        painelUI.SetActive(false);
        AtualizarTexto();
    }

    void Update()
    {
        // Se a UI já está aberta, só atualiza o estado dos botões
        if (uiAberta)
        {
            // Os botões só funcionam se tiver um objeto na prensa
            bool temObjeto = prensa != null && ObjetoNaPrensa();
            botaoConfirmar.interactable = temObjeto;
            botaoEnviar.interactable = temObjeto;
            return;
        }

        // Garante que temos referência da câmera do jogador
        if (cam == null)
        {
            if (Player.LocalCamera != null)
            {
                cam = Player.LocalCamera;
            }
            else
            {
                cam = Camera.main;
            }

            if (cam == null) return;
        }

        // Detecta toque na tela (mouse no editor)
        Vector2 posicao;
        if (!DetectouToque(out posicao)) return;

        // Faz um raio do toque até o mundo
        Ray ray = cam.ScreenPointToRay(posicao);
        RaycastHit[] hits = Physics.RaycastAll(ray, 30f);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // Verifica se o primeiro objeto sólido atingido foi o painel
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            // Ignora triggers e o próprio jogador
            if (hit.collider.isTrigger) continue;
            if (hit.collider.CompareTag("Player")) continue;

            // Se acertou no painel (ou em algum filho dele), abre a UI
            bool acertouPainel = hit.collider.transform == transform
                              || hit.collider.transform.IsChildOf(transform);

            if (acertouPainel)
            {
                AbrirUI();
            }

            break;
        }
    }

    // Abre o painel da UI
    void AbrirUI()
    {
        uiAberta = true;
        painelUI.SetActive(true);
    }

    // Fecha o painel da UI (chamado por botão)
    public void FecharUI()
    {
        uiAberta = false;
        painelUI.SetActive(false);
    }

    // Botões da UI para escolher a escala
    public void SelecionarPequeno()
    {
        escalaEscolhida = escalaPequena;
        AtualizarTexto();
    }

    public void SelecionarMedio()
    {
        escalaEscolhida = escalaMedia;
        AtualizarTexto();
    }

    public void SelecionarGrande()
    {
        escalaEscolhida = escalaGrande;
        AtualizarTexto();
    }

    // Botão Confirmar: aciona a prensa com a escala escolhida
    public void Confirmar()
    {
        prensa.Ativar(escalaEscolhida);
        FecharUI();
    }

    // Botão Enviar: levanta a prensa e empurra o objeto pela esteira
    public void Enviar()
    {
        // Confere se tem como enviar
        if (prensa == null) return;
        if (prensa.PontoDoObjeto == null) return;
        if (!prensa.PontoDoObjeto.Ocupado) return;

        StartCoroutine(SequenciaEnviar());
        FecharUI();
    }

    // Sequência completa: levanta prensa, libera objeto e aplica velocidade
    IEnumerator SequenciaEnviar()
    {
        // Espera a prensa subir antes de soltar o objeto
        yield return prensa.Levantar();

        Transform obj = prensa.PontoDoObjeto.LiberarObjeto();
        if (obj == null) yield break;
        if (destino == null) yield break;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null) yield break;

        // Religa a física para o objeto poder ser movido
        rb.isKinematic = false;
        yield return new WaitForFixedUpdate();

        // Calcula a direção horizontal do objeto até o destino
        Vector3 dir = destino.position - obj.position;
        dir.y = 0f;
        dir = dir.normalized;

        // Aplica a velocidade no objeto para ele andar até a esteira
        rb.linearVelocity = dir * velocidadeEsteira;
    }

    // Atualiza o texto da UI mostrando a escala selecionada
    void AtualizarTexto()
    {
        if (textoEscalaSelecionada != null)
        {
            textoEscalaSelecionada.text = "Escala: " + escalaEscolhida.x.ToString("0.#") + "x";
        }
    }

    // Verifica se tem objeto encaixado na prensa
    bool ObjetoNaPrensa()
    {
        if (prensa.PontoDoObjeto == null) return false;
        return prensa.PontoDoObjeto.Ocupado;
    }

    // Detecta toque na tela (mobile) ou clique do mouse (editor)
    bool DetectouToque(out Vector2 posicao)
    {
#if UNITY_EDITOR
        // No editor: usa o clique do mouse
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            posicao = Input.mousePosition;
            return true;
        }
#else
        // No celular: percorre os toques ativos procurando um novo
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            if (t.phase != TouchPhase.Began) continue;
            if (EventSystem.current.IsPointerOverGameObject(t.fingerId)) continue;

            posicao = t.position;
            return true;
        }
#endif
        posicao = Vector2.zero;
        return false;
    }
}
