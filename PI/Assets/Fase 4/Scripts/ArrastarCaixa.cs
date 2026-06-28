using UnityEngine;
using UnityEngine.EventSystems;
using Fusion;

// Deixa o jogador arrastar a caixa com o toque (ou clique no editor).
// O movimento é feito no FixedUpdateNetwork pra funcionar certo na rede.
// A caixa precisa de Box Collider, Rigidbody, NetworkObject e NetworkTransform.
public class ArrastarCaixa : NetworkBehaviour
{
    private bool arrastando = false;
    private bool querSoltar = false;     // guarda que o jogador soltou pra tratar no tick
    private Camera cameraJogador;
    private Rigidbody corpo;
    private float profundidade;          // distância da caixa até a câmera quando peguei
    private float alturaTravada;         // mantém o Y pra caixa não subir nem descer
    private Vector2 posicaoDedo;         // última posição do dedo/mouse na tela

    public override void Spawned()
    {
        corpo = GetComponent<Rigidbody>();

        if (corpo != null)
        {
            corpo.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            corpo.interpolation = RigidbodyInterpolation.Interpolate;
            corpo.freezeRotation = true;
        }
    }

    // O Update só lê o toque. Quem move a caixa é o FixedUpdateNetwork.
    private void Update()
    {
        if (cameraJogador == null)
        {
            cameraJogador = Player.LocalCamera != null ? Player.LocalCamera : Camera.main;
            if (cameraJogador == null) return;
        }

        // Lê o toque ou o mouse (o que estiver disponível)
        bool pressionou, segurando, soltou;
        Vector2 posicao;
        LerToque(out pressionou, out segurando, out soltou, out posicao);

        if (!arrastando)
        {
            // Começa a arrastar se o toque acertou a caixa (e não foi na UI)
            if (pressionou && !TocandoNaUI() && RaioAcertouCaixa(posicao))
                ComecarArraste(posicao);
        }
        else
        {
            if (segurando) posicaoDedo = posicao;   // o tick usa essa posição
            if (soltou) querSoltar = true;
        }
    }

    // Movimento da caixa no tick da rede
    public override void FixedUpdateNetwork()
    {
        if (!arrastando && !querSoltar) return;

        // Na rede, só quem tem a autoridade pode mover a caixa.
        // Enquanto não tenho, fico pedindo e não mexo na posição.
        if (!HasStateAuthority)
        {
            Object.RequestStateAuthority();
            return;
        }

        if (querSoltar)
        {
            querSoltar = false;
            arrastando = false;
            if (corpo != null) corpo.isKinematic = false;   // liga a física de novo
            return;
        }

        if (cameraJogador == null) return;

        // Transforma a posição do dedo na tela em uma posição no mundo
        Vector3 pontoNaTela = new Vector3(posicaoDedo.x, posicaoDedo.y, profundidade);
        Vector3 destino = cameraJogador.ScreenToWorldPoint(pontoNaTela);
        destino.y = alturaTravada;

        transform.position = destino;
    }

    private void ComecarArraste(Vector2 posicao)
    {
        arrastando = true;
        posicaoDedo = posicao;
        alturaTravada = transform.position.y;
        profundidade = cameraJogador.WorldToScreenPoint(transform.position).z;

        // Pede a autoridade pra conseguir mover a caixa na rede
        if (!HasStateAuthority) Object.RequestStateAuthority();

        if (corpo != null)
        {
            corpo.linearVelocity = Vector3.zero;
            corpo.angularVelocity = Vector3.zero;
            corpo.isKinematic = true;
        }
    }

    // Lê a entrada: usa o toque se tiver dedo na tela, senão usa o mouse.
    private void LerToque(out bool pressionou, out bool segurando, out bool soltou, out Vector2 posicao)
    {
        pressionou = segurando = soltou = false;
        posicao = Vector2.zero;

        if (Input.touchCount > 0)
        {
            Touch dedo = Input.GetTouch(0);
            posicao = dedo.position;
            pressionou = dedo.phase == TouchPhase.Began;
            segurando = dedo.phase == TouchPhase.Began
                     || dedo.phase == TouchPhase.Moved
                     || dedo.phase == TouchPhase.Stationary;
            soltou = dedo.phase == TouchPhase.Ended || dedo.phase == TouchPhase.Canceled;
            return;
        }

        posicao = Input.mousePosition;
        pressionou = Input.GetMouseButtonDown(0);
        segurando = Input.GetMouseButton(0);
        soltou = Input.GetMouseButtonUp(0);
    }

    private bool TocandoNaUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    // Dispara um raio da câmera e vê se acertou esta caixa.
    private bool RaioAcertouCaixa(Vector2 posicaoNaTela)
    {
        if (cameraJogador == null) return false;

        // Ignora a camada "LocalPlayer" pra o raio não bater no corpo do
        // próprio jogador (a câmera fica em primeira pessoa, dentro dele).
        Ray raio = cameraJogador.ScreenPointToRay(posicaoNaTela);
        int camadaJogador = LayerMask.NameToLayer("LocalPlayer");
        int mascara = camadaJogador >= 0 ? ~(1 << camadaJogador) : Physics.DefaultRaycastLayers;
        RaycastHit[] batidas = Physics.RaycastAll(raio, 50f, mascara);
        System.Array.Sort(batidas, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit batida in batidas)
        {
            if (batida.collider.isTrigger) continue;
            if (batida.collider.CompareTag("Player")) continue;

            bool acertou = batida.collider.transform == transform
                        || batida.collider.transform.IsChildOf(transform);
            if (acertou) return true;
            break;
        }
        return false;
    }

    private void OnDisable()
    {
        if (arrastando)
        {
            arrastando = false;
            if (corpo != null) corpo.isKinematic = false;
        }
    }
}
