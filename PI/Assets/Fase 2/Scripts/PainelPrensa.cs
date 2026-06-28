using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// Painel touchscreen pra escolher escala, prensar e enviar pela esteira.
// Agora em rede: prensar e enviar viram RPC, entao os dois jogadores veem a
// prensa descer/subir e o objeto mudar de tamanho. Qualquer jogador (Kofi ou
// Aldric) pode operar o painel.
public class PainelPrensa : NetworkBehaviour
{
    [SerializeField] private Prensa prensa;                       // Referencia a prensa controlada

    [SerializeField] private GameObject painelUI;                 // UI que aparece ao tocar no painel
    [SerializeField] private TextMeshProUGUI textoEscalaSelecionada; // Mostra a escala escolhida
    [SerializeField] private Button botaoConfirmar;               // Botao para acionar a prensa
    [SerializeField] private Button botaoEnviar;                  // Botao para mandar o objeto na esteira

    [SerializeField] private float velocidadeEsteira = 4f;        // Velocidade do empurrao na esteira
    [SerializeField] private float tempoAntesDeEnviar = 1.5f;     // Espera a prensa subir antes de soltar
    [SerializeField] private Transform destino;                   // Para onde o objeto e lancado

    [SerializeField] private Vector3 escalaPequena = new Vector3(0.5f, 0.5f, 0.5f); // Escala "P"
    [SerializeField] private Vector3 escalaMedia = new Vector3(1f, 1f, 1f);         // Escala "M"
    [SerializeField] private Vector3 escalaGrande = new Vector3(1.5f, 1.5f, 1.5f);  // Escala "G"

    private Vector3 escalaEscolhida;  // Escala atualmente selecionada
    private bool uiAberta = false;    // Se o painel UI esta aberto
    private Camera cam;               // Camera usada para o raycast do toque

    void Start()
    {
        // Comeca com a escala media selecionada e o painel fechado
        escalaEscolhida = escalaMedia;
        painelUI.SetActive(false);
        AtualizarTexto();
    }

    void Update()
    {
        // Se a UI ja esta aberta, so atualiza o estado dos botoes
        if (uiAberta)
        {
            // Os botoes so funcionam se tiver um objeto na prensa
            bool temObjeto = prensa != null && ObjetoNaPrensa();
            botaoConfirmar.interactable = temObjeto;
            botaoEnviar.interactable = temObjeto;
            return;
        }

        // Garante que temos referencia da camera do jogador
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

        // Faz um raio do toque ate o mundo.
        // Ignora a layer "LocalPlayer" para o raio nao bater no corpo invisivel
        // do jogador local (camera em primeira pessoa fica dentro do corpo).
        Ray ray = cam.ScreenPointToRay(posicao);
        int localPlayerLayer = LayerMask.NameToLayer("LocalPlayer");
        int mascara = localPlayerLayer >= 0 ? ~(1 << localPlayerLayer) : Physics.DefaultRaycastLayers;
        RaycastHit[] hits = Physics.RaycastAll(ray, 30f, mascara);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // Verifica se o primeiro objeto solido atingido foi o painel
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            // Ignora triggers e o proprio jogador
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
        // Liberado pro Kofi e pro Aldric usarem o painel.
        // Pra restringir a um personagem, e so checar BasicSpawner.PersonagemLocal aqui.
        uiAberta = true;
        painelUI.SetActive(true);
    }

    // Fecha o painel da UI (chamado por botao)
    public void FecharUI()
    {
        uiAberta = false;
        painelUI.SetActive(false);
    }

    // Botoes da UI para escolher a escala
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

    // Botao Confirmar: manda todos prensarem com a escala escolhida
    public void Confirmar()
    {
        if (prensa == null) return;
        if (!ObjetoNaPrensa()) return;

        RPC_Prensar(escalaEscolhida);
        FecharUI();
    }

    // RPC: a prensa desce e escala o objeto em todos os clientes
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_Prensar(Vector3 escala)
    {
        if (prensa != null) prensa.Ativar(escala);
    }

    // Botao Enviar: levanta a prensa nos dois e o host empurra o objeto
    public void Enviar()
    {
        if (prensa == null || prensa.PontoDoObjeto == null) return;
        if (!prensa.PontoDoObjeto.Ocupado) return;

        StartCoroutine(SequenciaEnviar());
        FecharUI();
    }

    // Sequencia: levanta a prensa (visual nos dois) e o host solta + empurra
    IEnumerator SequenciaEnviar()
    {
        // Todos sobem a prensa
        RPC_Levantar();

        // Espera a prensa subir antes de soltar o objeto
        yield return new WaitForSeconds(tempoAntesDeEnviar);

        Transform obj = prensa.PontoDoObjeto.ObjetoEncaixado;
        if (obj == null) yield break;
        if (destino == null) yield break;

        ArrastarItem item = obj.GetComponent<ArrastarItem>();
        if (item == null) yield break;

        // Direcao horizontal do objeto ate o destino
        Vector3 dir = destino.position - obj.position;
        dir.y = 0f;
        dir = dir.normalized;

        // So eu (quem operou) mando o host soltar e empurrar, pra nao empurrar duas vezes
        item.RPC_Enviar(dir * velocidadeEsteira);
    }

    // RPC: a prensa sobe de volta em todos os clientes
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_Levantar()
    {
        if (prensa != null) StartCoroutine(prensa.Levantar());
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
        PontoDeEncaixe ponto = prensa.PontoDoObjeto;
        if (ponto == null) return false;
        if (ponto.Object == null) return false; // ainda nao entrou na rede
        return ponto.Ocupado;
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
