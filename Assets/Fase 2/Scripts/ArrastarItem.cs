using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// Pega, segura na mão e solta objetos (encaixando em pontos ou largando).
// Versão local: escreve a posição no Update, então os objetos Pegáveis NÃO
// devem ter NetworkTransform (senão a rede briga com a escrita e o objeto trava).
public class ArrastarItem : MonoBehaviour
{
    // Garante que só um objeto seja segurado por vez
    private static ArrastarItem objetoSendoSeguro = null;

    private Transform jogador;          // jogador local
    private Transform pontoMao;         // ponto onde o objeto fica preso (mão)
    private Camera cam;                 // câmera usada no raycast
    private bool segurandoEsteObjeto = false; // se está nas minhas mãos
    private Rigidbody rb;
    private float tempoCooldown = 4f;   // tempo até poder pegar de novo

    public float maxTimeBetweenTaps = 0.3f; // janela entre os dois toques pra soltar
    private int tapCount = 0;
    private float lastTapTime = 0;
    public UnityEvent onDoubleTap;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        // Corrige escala negativa que quebra o collider
        Vector3 s = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
    }

    void Update()
    {
        // Conta o cooldown pra baixo
        if (tempoCooldown > 0f)
        {
            tempoCooldown -= Time.deltaTime;
        }

        // Acha as referências do jogador local na primeira vez
        if (jogador == null)
        {
            jogador = Player.LocalTransform;
            if (jogador == null) return;

            pontoMao = Player.LocalPontoMao != null ? Player.LocalPontoMao : jogador;
            cam = Player.LocalCamera != null ? Player.LocalCamera : Camera.main;
        }

        // Segurando: gruda na mão e espera dois toques pra soltar
        if (segurandoEsteObjeto)
        {
            AtualizarPosicaoNaMao();

            Vector2 toque;
            if (DetectouToque(out toque))
            {
                // Dois toques rápidos (double-tap) soltam o objeto
                float agora = Time.time;
                if (agora - lastTapTime <= maxTimeBetweenTaps)
                    tapCount++;
                else
                    tapCount = 1;

                lastTapTime = agora;

                if (tapCount >= 2)
                {
                    tapCount = 0;
                    onDoubleTap?.Invoke();
                    SoltarObjeto();
                }
            }
            return;
        }

        // Não tenta pegar em cooldown, se já tem alguém segurando, ou se a tag não bate
        if (tempoCooldown > 0f) return;
        if (objetoSendoSeguro != null) return;
        if (!CompareTag("Pegavel")) return;

        // Pega o objeto se tocar nele
        Vector2 posicaoToque;
        if (DetectouToque(out posicaoToque))
        {
            TentarPegarObjeto(posicaoToque);
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

    // Vê se o toque acertou este objeto e o pega
    void TentarPegarObjeto(Vector2 posicaoToque)
    {
        if (cam == null) return;

        // Raio da câmera ignorando a camada do jogador (câmera fica dentro do corpo)
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

            // O primeiro sólido tem que ser este objeto
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

    // Faz o objeto acompanhar a mão do jogador
    void AtualizarPosicaoNaMao()
    {
        if (pontoMao == null) return;
        transform.position = pontoMao.position;
        transform.rotation = pontoMao.rotation;
    }

    // Pega o objeto: marca como segurando e desliga a física
    void PegarObjeto()
    {
        // Apenas Kofi pode carregar objetos nesta fase
        if (BasicSpawner.PersonagemLocal == Personagem.Aldric)
        {
            FeedbackUI.Mostrar("Apenas Kofi pode carregar objetos.");
            return;
        }

        segurandoEsteObjeto = true;
        objetoSendoSeguro = this;

        // Zera o contador pro double-tap começar limpo
        tapCount = 0;
        lastTapTime = 0f;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    // Solta o objeto: tenta encaixar no ponto mais próximo, senão religa a física
    void SoltarObjeto()
    {
        segurandoEsteObjeto = false;
        objetoSendoSeguro = null;

        PontoDeEncaixe[] pontos = FindObjectsByType<PontoDeEncaixe>(FindObjectsSortMode.None);

        // Considera a posição do objeto e a do jogador, e usa a menor distância
        Vector3 posObjeto = transform.position;
        Vector3 posJogador = jogador != null ? jogador.position : posObjeto;

        // Escolhe o ponto aceito mais próximo
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

        if (maisProximo != null && distMaisProximo <= raioMaisProximo)
        {
            maisProximo.EncaixarObjeto(transform);
            return;
        }

        // Não encaixou: religa a gravidade e dá um cooldown
        tempoCooldown = 0.4f;

        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }

    // Se o objeto for desativado segurando, solta automaticamente
    void OnDisable()
    {
        if (segurandoEsteObjeto)
        {
            SoltarObjeto();
        }
    }
}
