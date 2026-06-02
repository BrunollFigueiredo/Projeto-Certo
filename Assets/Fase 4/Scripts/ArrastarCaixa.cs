using UnityEngine;
using UnityEngine.EventSystems;

// Drag & drop LOCAL (sem rede): toca e arrasta enquanto segura o dedo.
// Versao de teste, sem Fusion — o NetworkObject/NetworkTransform estava
// brigando com a escrita direta de transform.position e travava o arraste.
// Box Collider (nao MeshCollider) + Rigidbody obrigatorio.
public class ArrastarCaixa : MonoBehaviour
{
    private bool _arrastando = false;
    private Camera _cam;
    private Rigidbody _rb;
    private float _profundidadeTela; // distancia do objeto a camera ao iniciar o drag
    private float _alturaFixa;       // Y do objeto nao muda durante o drag

    private void Start()
    {
        _rb  = GetComponent<Rigidbody>();

        if (_rb != null)
        {
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.freezeRotation = true;
        }
    }

    private void Update()
    {
        if (_cam == null)
        {
            _cam = Player.LocalCamera != null ? Player.LocalCamera : Camera.main;
            if (_cam == null) return;
        }

        // Le o ponteiro de forma unificada: a MESMA fonte (toque ou mouse) que
        // comeca o arraste tambem o continua e o finaliza. No Device Simulator o
        // gesto segurado e reportado como TOQUE (nao como mouse mantido), entao
        // misturar os dois caminhos travava o drag no meio. Prioriza toque quando
        // existe; cai no mouse so quando nao ha toque (editor sem simulador).
        bool down, held, up;
        Vector2 pos;
        LerPonteiro(out down, out held, out up, out pos);

        if (!_arrastando)
        {
            if (down && !SobreUI() && RaycastAcertouEste(pos))
                IniciarArrastar();
        }
        else
        {
            if (held) ContinuarArrastar(pos);
            if (up)   SoltarArrastar();
        }
    }

    // ─── Leitura unificada do ponteiro (toque ou mouse) ──────────────────────

    private void LerPonteiro(out bool down, out bool held, out bool up, out Vector2 pos)
    {
        down = held = up = false;
        pos = Vector2.zero;

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            pos  = t.position;
            down = t.phase == TouchPhase.Began;
            held = t.phase == TouchPhase.Began
                || t.phase == TouchPhase.Moved
                || t.phase == TouchPhase.Stationary;
            up   = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
            return;
        }

        pos  = Input.mousePosition;
        down = Input.GetMouseButtonDown(0);
        held = Input.GetMouseButton(0);
        up   = Input.GetMouseButtonUp(0);
    }

    private bool SobreUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    // ─── Raycast ignora collider do jogador ─────────────────────────────────

    private bool RaycastAcertouEste(Vector2 posicaoTela)
    {
        if (_cam == null) return false;
        // Ignora a layer "LocalPlayer": o corpo do jogador local fica nela e,
        // como a camera e em primeira pessoa, o raio bateria primeiro no proprio
        // corpo invisivel e nunca chegaria na caixa.
        Ray ray = _cam.ScreenPointToRay(posicaoTela);
        int localPlayerLayer = LayerMask.NameToLayer("LocalPlayer");
        int mascara = localPlayerLayer >= 0 ? ~(1 << localPlayerLayer) : Physics.DefaultRaycastLayers;
        RaycastHit[] hits = Physics.RaycastAll(ray, 50f, mascara);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.isTrigger) continue;
            if (hit.collider.CompareTag("Player")) continue;

            bool acertou = hit.collider.transform == transform
                        || hit.collider.transform.IsChildOf(transform);
            if (acertou) return true;
            break;
        }
        return false;
    }

    // ─── Drag ───────────────────────────────────────────────────────────────

    private void IniciarArrastar()
    {
        _arrastando = true;
        _alturaFixa = transform.position.y;

        // Profundidade do objeto na tela: usada no drag em screen-space
        _profundidadeTela = _cam.WorldToScreenPoint(transform.position).z;

        if (_rb != null)
        {
            _rb.linearVelocity  = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic     = true;
        }
    }

    private void ContinuarArrastar(Vector2 posicaoTela)
    {
        if (!_arrastando || _cam == null) return;

        // Converte posicao da tela para mundo mantendo a mesma profundidade
        // Isso funciona bem com qualquer angulo de camera
        Vector3 screenPos = new Vector3(posicaoTela.x, posicaoTela.y, _profundidadeTela);
        Vector3 destino   = _cam.ScreenToWorldPoint(screenPos);
        destino.y         = _alturaFixa; // trava altura

        transform.position = destino;
    }

    private void SoltarArrastar()
    {
        _arrastando = false;

        if (_rb != null)
            _rb.isKinematic = false;
    }

    private void OnDisable()
    {
        if (_arrastando) SoltarArrastar();
    }
}
