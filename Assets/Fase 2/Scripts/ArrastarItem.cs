using Fusion;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// Permite pegar, segurar na mão e soltar objetos (encaixando ou jogando).
// IMPORTANTE: as escritas de posição (grudar na mão / encaixar) acontecem em
// FixedUpdateNetwork (no tick), NÃO no Update. Na autoridade, o NetworkTransform
// reaplica a posição do tick durante o Render; uma escrita feita no Update seria
// atropelada todo frame e o objeto ficava "bugado" sem ser pego de verdade.
// Requer: NetworkObject + NetworkTransform + Collider + Rigidbody.
public class ArrastarItem : NetworkBehaviour
{
    /// Referência estática para garantir que só um objeto seja segurado por vez
    private static ArrastarItem objetoSendoSeguro = null;

    private Transform jogador;          // Transform do jogador local
    private Transform pontoMao;         // Ponto onde o objeto fica preso (mão)
    private Camera cam;                 // Câmera usada no raycast
    private bool segurandoEsteObjeto = false; // Se este objeto está nas minhas mãos
    private bool pedidoSoltar = false;  // Input pediu soltar; aplicado no próximo tick
    private Rigidbody rb;               // Rigidbody do objeto
    private float tempoCooldown = 4f;   // Tempo até poder pegar de novo após soltar
    public float maxTimeBetweenTaps = 0.3f;
    private int tapCount = 0;
    private float lastTapTime = 0;
    public UnityEvent onDoubleTap;

    public override void Spawned()
    {
        // Configura o Rigidbody para evitar bugs de colisão e rotação
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        // Corrige escalas negativas que quebram colisores
        Vector3 s = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
    }

    // ─── Update SÓ lê o input. As escritas de posição vão pro tick. ───────────

    void Update()
    {
        // Diminui o cooldown a cada frame
        if (tempoCooldown > 0f)
        {
            tempoCooldown -= Time.deltaTime;
        }

        // Busca as referências do jogador local na primeira vez
        if (jogador == null)
        {
            jogador = Player.LocalTransform;
            if (jogador == null) return;

            if (Player.LocalPontoMao != null)
            {
                pontoMao = Player.LocalPontoMao;
            }
            else
            {
                pontoMao = jogador;
            }

            if (Player.LocalCamera != null)
            {
                cam = Player.LocalCamera;
            }
            else
            {
                cam = Camera.main;
            }
        }

        // Se já estou segurando este objeto: soltar exige DOIS toques rápidos
        // (double-tap). Dois toques dentro de maxTimeBetweenTaps marcam o pedido
        // de soltar — o tick processa. (Seguir a mão também é feito no tick, por
        // isso aqui não escrevemos posição.)
        if (segurandoEsteObjeto)
        {
            Vector2 _toqueSolta;
            if (DetectouToque(out _toqueSolta))
            {
                float agora = Time.time;

                // Se o toque anterior foi recente, conta como 2º toque; senão reinicia.
                if (agora - lastTapTime <= maxTimeBetweenTaps)
                    tapCount++;
                else
                    tapCount = 1;

                lastTapTime = agora;

                if (tapCount >= 2)
                {
                    pedidoSoltar = true;
                    tapCount = 0;
                    onDoubleTap?.Invoke();
                }
            }
            return; // enquanto seguro algo, não tento pegar outro objeto
        }

        // Não tenta pegar se está em cooldown, se já tem alguém segurando ou se a tag não bate
        if (tempoCooldown > 0f) return;
        if (objetoSendoSeguro != null) return;
        if (!CompareTag("Pegavel")) return;

        // Tenta pegar se houve toque na tela em cima do objeto
        Vector2 posicaoToque;
        if (DetectouToque(out posicaoToque))
        {
            TentarPegarObjeto(posicaoToque);
        }
    }

    // ─── Posição é aplicada no tick de rede ──────────────────────────────────

    public override void FixedUpdateNetwork()
    {
        if (!segurandoEsteObjeto && !pedidoSoltar) return;

        // Só a autoridade pode mexer no transform de um NetworkObject. Enquanto
        // ela não chega, seguimos pedindo e não escrevemos (NetworkTransform segura).
        if (!HasStateAuthority)
        {
            Object.RequestStateAuthority();
            return;
        }

        if (pedidoSoltar)
        {
            pedidoSoltar = false;
            SoltarObjeto();
            return;
        }

        if (segurandoEsteObjeto)
        {
            AtualizarPosicaoNaMao();
        }
    }

    // Render roda a cada frame visual (depois da interpolação). Para quem está
    // segurando, gruda o objeto exatamente no PontoMão sem o atraso da
    // interpolação do NetworkTransform — assim ele vai direto pra mão e fica lá.
    // A posição de rede para os outros jogadores continua vindo do tick.
    public override void Render()
    {
        if (segurandoEsteObjeto && HasStateAuthority)
        {
            AtualizarPosicaoNaMao();
        }
    }

    // Detecta toque no mobile ou clique do mouse no editor
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

    // Verifica se o toque acertou este objeto e o pega
    void TentarPegarObjeto(Vector2 posicaoToque)
    {
        if (cam == null) return;

        // Dispara um raio da câmera passando pelo toque.
        // Ignora a layer "LocalPlayer": o corpo do jogador local fica nela e,
        // como a câmera é em primeira pessoa (dentro do corpo), o raio bateria
        // primeiro no próprio corpo invisível e nunca chegaria no objeto.
        Ray ray = cam.ScreenPointToRay(posicaoToque);
        int localPlayerLayer = LayerMask.NameToLayer("LocalPlayer");
        int mascara = localPlayerLayer >= 0 ? ~(1 << localPlayerLayer) : Physics.DefaultRaycastLayers;
        RaycastHit[] hits = Physics.RaycastAll(ray, 30f, mascara);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            // Ignora triggers e o player
            if (hit.collider.isTrigger) continue;
            if (hit.collider.CompareTag("Player")) continue;

            // Confere se o primeiro objeto sólido é este
            bool acertouEste = hit.collider.transform == transform
                            || hit.collider.transform.IsChildOf(transform);

            if (acertouEste)
            {
                PegarObjeto();
                return;
            }

            break;
        }
    }

    // Atualiza a posição/rotação do objeto para acompanhar a mão do jogador
    void AtualizarPosicaoNaMao()
    {
        if (pontoMao == null) return;
        transform.position = pontoMao.position;
        transform.rotation = pontoMao.rotation;
    }

    // Pega o objeto: marca como segurando e desliga a física
    void PegarObjeto()
    {
        // Apenas Kofi pode carregar objetos nesta fase.
        if (BasicSpawner.PersonagemLocal == Personagem.Aldric)
        {
            FeedbackUI.Mostrar("Apenas Kofi pode carregar objetos.");
            return;
        }

        segurandoEsteObjeto = true;
        objetoSendoSeguro = this;

        // Zera o contador para o double-tap de soltar começar do zero.
        tapCount = 0;
        lastTapTime = 0f;

        // Pede a autoridade do objeto na rede para que mover a posição
        // sincronize para o outro jogador via NetworkTransform.
        if (!HasStateAuthority)
        {
            Object.RequestStateAuthority();
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    // Solta o objeto: tenta encaixar em algum ponto próximo, senão religa física
    void SoltarObjeto()
    {
        segurandoEsteObjeto = false;
        objetoSendoSeguro = null;

        // Procura por todos os pontos de encaixe da cena
        PontoDeEncaixe[] pontos = FindObjectsByType<PontoDeEncaixe>(FindObjectsSortMode.None);

        // Considera a posição do OBJETO (mão) e a do JOGADOR, e usa a MENOR
        // distância — assim encaixa se qualquer um dos dois estiver dentro do raio.
        Vector3 posObjeto = transform.position;
        Vector3 posJogador = jogador != null ? jogador.position : posObjeto;

        // Escolhe o ponto ACEITO mais próximo (não o primeiro da lista).
        PontoDeEncaixe maisProximo = null;
        float distMaisProximo = float.MaxValue;
        float raioMaisProximo = 0f;

        for (int i = 0; i < pontos.Length; i++)
        {
            PontoDeEncaixe ponto = pontos[i];
            if (!ponto.AceitaObjeto(gameObject)) continue;

            float dist = Mathf.Min(
                Vector3.Distance(posObjeto, ponto.transform.position),
                Vector3.Distance(posJogador, ponto.transform.position));

            if (dist < distMaisProximo)
            {
                distMaisProximo = dist;
                maisProximo = ponto;
                raioMaisProximo = ponto.RaioDeSnap;
            }
        }

        // DIAGNÓSTICO temporário: mostra a distância até o ponto aceito mais próximo.
        if (maisProximo == null)
            Debug.LogWarning("[ArrastarItem] Soltar -> NENHUM ponto de encaixe aceita este objeto (existe PontoDeEncaixe na cena? tag bate? está ocupado?)");
        else
            Debug.Log("[ArrastarItem] Soltar -> ponto '" + maisProximo.name + "' em " + maisProximo.transform.position
                + " | dist " + distMaisProximo.ToString("0.00") + " (raio " + raioMaisProximo.ToString("0.00") + ") -> "
                + (distMaisProximo <= raioMaisProximo ? "ENCAIXOU" : "longe demais, caiu"));

        if (maisProximo != null && distMaisProximo <= raioMaisProximo)
        {
            maisProximo.EncaixarObjeto(transform);
            return;
        }

        // Se não encaixou em ninguém, religa a gravidade e aplica cooldown
        tempoCooldown = 0.4f;

        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }

    // Se o objeto for desativado enquanto está sendo segurado, solta automaticamente
    void OnDisable()
    {
        if (segurandoEsteObjeto)
        {
            SoltarObjeto();
        }
    }
}
