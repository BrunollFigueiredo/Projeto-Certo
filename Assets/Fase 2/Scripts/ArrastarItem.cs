using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// Permite pegar, segurar na mão e soltar objetos (encaixando ou jogando)
public class ArrastarItem : MonoBehaviour
{
    // Referência estática para garantir que só um objeto seja segurado por vez
    private static ArrastarItem objetoSendoSeguro = null;

    private Transform jogador;          // Transform do jogador local
    private Transform pontoMao;         // Ponto onde o objeto fica preso (mão)
    private Camera cam;                 // Câmera usada no raycast
    private bool segurandoEsteObjeto = false; // Se este objeto está nas minhas mãos
    private Rigidbody rb;               // Rigidbody do objeto
    private float tempoCooldown = 4f;   // Tempo até poder pegar de novo após soltar
    public float maxTimeBetweenTaps = 0.3f;
    private int tapCount = 0;
    private float lastTapTime = 0;
    public UnityEvent onDoubleTap;
    void Start()
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

        // Se já estou segurando este objeto: gruda na mão e espera novo toque para soltar
        if (segurandoEsteObjeto)
        {
            AtualizarPosicaoNaMao();
        }
        if (Input.touchCount > 0)
        {

            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {

                if (Time.time - lastTapTime < maxTimeBetweenTaps)
                {

                    tapCount++;
                    if (tapCount == 2)
                    {

                        //onDoubleTap.Invoke(); // Dispara o evento

                        Debug.Log("Toque duplo");
                        SoltarObjeto();
                        tapCount = 0; // Reseta o contador

                    }

                }

                else
                {

                    tapCount = 1; // Primeiro toque

                }

                lastTapTime = Time.time;

            }

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

        // Dispara um raio da câmera passando pelo toque
        Ray ray = cam.ScreenPointToRay(posicaoToque);
        RaycastHit[] hits = Physics.RaycastAll(ray, 30f);
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
        transform.position = pontoMao.position;
        transform.rotation = pontoMao.rotation;
    }

    // Pega o objeto: marca como segurando e desliga a física
    void PegarObjeto()
    {
        segurandoEsteObjeto = true;
        objetoSendoSeguro = this;

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

        for (int i = 0; i < pontos.Length; i++)
        {
            PontoDeEncaixe ponto = pontos[i];

            // Verifica se o ponto aceita este objeto e se está perto o suficiente
            if (!ponto.AceitaObjeto(gameObject)) continue;

            float distancia = Vector3.Distance(transform.position, ponto.transform.position);
            if (distancia <= ponto.RaioDeSnap)
            {
                ponto.EncaixarObjeto(transform);
                return;
            }
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
