using Fusion;
using UnityEngine;
using UnityEngine.EventSystems;

// Objeto que o Kofi carrega na mao e encaixa na prensa.
// Agora em rede: quem manda na posicao e o host (StateAuthority). Ele escreve a
// posicao e o NetworkRigidbody espelha pro outro jogador, entao o Aldric ve o
// objeto se mexer e ir pra prensa.
public class ArrastarItem : NetworkBehaviour
{
    // So deixo o jogador local segurar um objeto por vez (controle local).
    private static ArrastarItem objetoLocalSeguro = null;

    // Quem esta segurando (null = ninguem). Sincronizado.
    [Networked] public Player Dono { get; set; }
    // Se esta travado no ponto de encaixe (parado na prensa). Sincronizado.
    [Networked] public NetworkBool Encaixado { get; set; }
    // Ponto onde foi encaixado, pra liberar depois. Sincronizado.
    [Networked] public PontoDeEncaixe PontoAtual { get; set; }

    [SerializeField] private float velocidadeMao = 15f; // suavidade ao seguir a mao

    public float maxTimeBetweenTaps = 0.3f; // janela entre os dois toques pra soltar
    private int tapCount = 0;
    private float lastTapTime = 0;

    private Rigidbody rb;
    private Camera cam;
    private bool euQueSeguro = false; // verdadeiro so na maquina de quem pegou
    private bool entrouNaRede = false; // so leio variaveis [Networked] depois do Spawned

    public override void Spawned()
    {
        entrouNaRede = true;
        rb = GetComponent<Rigidbody>();

        // Corrige escala negativa que quebra o collider
        Vector3 s = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
    }

    void Update()
    {
        // Antes de entrar na rede nao da pra ler Dono/Encaixado (variaveis [Networked])
        if (!entrouNaRede) return;

        // Pega a camera do jogador local pro raycast do toque
        if (cam == null)
            cam = Player.LocalCamera != null ? Player.LocalCamera : Camera.main;

        // Se sou EU que estou segurando, espero o double-tap pra soltar
        if (euQueSeguro)
        {
            Vector2 toque;
            if (DetectouToque(out toque))
            {
                float agora = Time.time;
                if (agora - lastTapTime <= maxTimeBetweenTaps) tapCount++;
                else tapCount = 1;

                lastTapTime = agora;

                if (tapCount >= 2)
                {
                    tapCount = 0;
                    euQueSeguro = false;
                    objetoLocalSeguro = null;
                    RPC_Soltar();
                }
            }
            return;
        }

        // Nao tento pegar se ja tem dono, se esta encaixado, ou se ja seguro outro
        if (Dono != null) return;
        if (Encaixado) return;
        if (objetoLocalSeguro != null) return;
        if (!CompareTag("Pegavel")) return;
        if (Player.LocalTransform == null) return;

        Vector2 posicaoToque;
        if (DetectouToque(out posicaoToque))
            TentarPegarObjeto(posicaoToque);
    }

    // O host escreve a posicao do objeto; o NetworkRigidbody espelha pros dois.
    public override void FixedUpdateNetwork()
    {
        // So o dono mexe; o NetworkRigidbody 3D cuida de sincronizar nos outros.
        if (!HasStateAuthority) return;
        if (Encaixado) return; // encaixado fica parado no ponto

        if (Dono != null && Dono.PontoMao != null)
        {
            if (!rb.isKinematic) rb.isKinematic = true;
            transform.position = Vector3.Lerp(transform.position, Dono.PontoMao.position, Runner.DeltaTime * velocidadeMao);
            transform.rotation = Quaternion.Lerp(transform.rotation, Dono.PontoMao.rotation, Runner.DeltaTime * velocidadeMao);
        }
        else
        {
            if (rb.isKinematic) rb.isKinematic = false; // solto: volta a cair
        }
    }

    // Detecta o toque no celular ou o clique do mouse no editor
    bool DetectouToque(out Vector2 posicao)
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            posicao = Input.mousePosition;
            return true;
        }
#else
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

    // Ve se o toque acertou este objeto e pede pra pegar
    void TentarPegarObjeto(Vector2 posicaoToque)
    {
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(posicaoToque);
        int camadaJogador = LayerMask.NameToLayer("LocalPlayer");
        int mascara = camadaJogador >= 0 ? ~(1 << camadaJogador) : Physics.DefaultRaycastLayers;
        RaycastHit[] hits = Physics.RaycastAll(ray, 30f, mascara);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            if (hit.collider.isTrigger) continue;
            if (hit.collider.CompareTag("Player")) continue;

            bool acertouEste = hit.collider.transform == transform
                            || hit.collider.transform.IsChildOf(transform);

            if (acertouEste)
                PegarObjeto();

            break;
        }
    }

    // Marca como segurando e avisa a rede
    void PegarObjeto()
    {
        // Apenas Kofi pode carregar objetos nesta fase
        if (BasicSpawner.PersonagemLocal == Personagem.Aldric)
        {
            FeedbackUI.Mostrar("Apenas Kofi pode carregar objetos.");
            return;
        }
        if (Player.Local == null) return;

        // Trava local imediata pra resposta rapida; a rede confirma o Dono
        euQueSeguro = true;
        objetoLocalSeguro = this;
        tapCount = 0;
        lastTapTime = 0f;

        RPC_Pegar(Player.Local);
    }

    // Avisa o host que peguei o objeto
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_Pegar(Player quem)
    {
        if (Dono != null || Encaixado) return; // ja esta com alguem
        Dono = quem;
    }

    // Avisa o host que soltei; ele decide se encaixa ou cai
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_Soltar()
    {
        Dono = null;
        TentarEncaixar();
    }

    // Roda no host: procura o ponto de encaixe mais perto e trava o objeto nele
    void TentarEncaixar()
    {
        PontoDeEncaixe[] pontos = FindObjectsByType<PontoDeEncaixe>(FindObjectsSortMode.None);

        PontoDeEncaixe maisProximo = null;
        float distMaisProximo = float.MaxValue;
        float raioMaisProximo = 0f;

        for (int i = 0; i < pontos.Length; i++)
        {
            PontoDeEncaixe ponto = pontos[i];
            if (!ponto.AceitaObjeto(gameObject)) continue;

            float dist = Vector3.Distance(transform.position, ponto.transform.position);
            if (dist < distMaisProximo)
            {
                distMaisProximo = dist;
                maisProximo = ponto;
                raioMaisProximo = ponto.RaioDeSnap;
            }
        }

        if (maisProximo != null && distMaisProximo <= raioMaisProximo)
        {
            // Trava no ponto. Posicao e estado sincronizam pelos [Networked].
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            transform.position = maisProximo.transform.position;
            transform.rotation = maisProximo.transform.rotation;

            Encaixado = true;
            PontoAtual = maisProximo;
            maisProximo.Ocupar(Object);
        }
        else
        {
            // Nao encaixou: cai no chao
            if (rb != null) rb.isKinematic = false;
        }
    }

    // Chamado pelo painel da prensa: solta da prensa e empurra pra esteira.
    // Vai pro host, que e quem manda na fisica do objeto.
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_Enviar(Vector3 velocidade)
    {
        if (PontoAtual != null)
        {
            PontoAtual.Liberar();
            PontoAtual = null;
        }

        Encaixado = false;
        Dono = null;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = velocidade;
        }
    }
}
