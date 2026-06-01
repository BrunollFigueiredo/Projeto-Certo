using Fusion;
using UnityEngine;
using UnityEngine.EventSystems;

// Drag & drop: toca e arrasta enquanto segura o dedo.
// Coloque nas caixas junto com NetworkObject + NetworkTransform.
// Box Collider (nao MeshCollider) + Rigidbody obrigatorio.
public class ArrastarCaixa : NetworkBehaviour
{
    private int _dedoId = -1;
    private bool _arrastando = false;
    private Camera _cam;
    private Rigidbody _rb;
    private NetworkObject _netObj;
    private float _profundidadeTela; // distancia do objeto a camera ao iniciar o drag
    private float _alturaFixa;       // Y do objeto nao muda durante o drag

    private void Start()
    {
        _rb  = GetComponent<Rigidbody>();
        _netObj = GetComponent<NetworkObject>();

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
            if (_cam == null)
            {
                Debug.LogWarning("[ArrastarCaixa] camera nula! LocalCamera=" + Player.LocalCamera + " main=" + Camera.main);
                return;
            }
            Debug.Log("[ArrastarCaixa] camera encontrada: " + _cam.name);
        }

#if UNITY_EDITOR
        AtualizarMouse();
        AtualizarToques();
#else
        AtualizarToques();
#endif
    }

    // ─── Touch ──────────────────────────────────────────────────────────────

    private void AtualizarToques()
    {
        if (Input.touchCount > 0)
            Debug.Log("[ArrastarCaixa] touches=" + Input.touchCount + " dedoId=" + _dedoId);

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);

            if (t.phase == TouchPhase.Began && _dedoId == -1)
            {
                bool sobreUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId);
                bool acertou = RaycastAcertouEste(t.position);
                Debug.Log("[ArrastarCaixa] Toque! sobreUI=" + sobreUI + " acertou=" + acertou + " pos=" + t.position);
                if (sobreUI) continue;
                if (acertou) IniciarArrastar(t.fingerId);
            }

            if (t.fingerId == _dedoId)
            {
                if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
                    ContinuarArrastar(t.position);
                else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                    SoltarArrastar();
            }
        }
    }

    // ─── Mouse (Editor) ─────────────────────────────────────────────────────

#if UNITY_EDITOR
    private void AtualizarMouse()
    {
        if (Input.GetMouseButtonDown(0) && _dedoId == -1)
        {
            bool acertou = RaycastAcertouEste(Input.mousePosition);
            Debug.Log("[ArrastarCaixa] MouseDown! acertou=" + acertou + " pos=" + Input.mousePosition);
            if (acertou) IniciarArrastar(-99);
        }
        if (_dedoId == -99)
        {
            if (Input.GetMouseButton(0))
                ContinuarArrastar(Input.mousePosition);
            else if (Input.GetMouseButtonUp(0))
                SoltarArrastar();
        }
    }
#endif

    // ─── Raycast ignora collider do jogador ─────────────────────────────────

    private bool RaycastAcertouEste(Vector2 posicaoTela)
    {
        if (_cam == null) return false;
        Ray ray = _cam.ScreenPointToRay(posicaoTela);
        RaycastHit[] hits = Physics.RaycastAll(ray, 50f);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        if (hits.Length == 0) { Debug.Log("[ArrastarCaixa] Raycast: nenhum hit"); return false; }

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.isTrigger) continue;
            if (hit.collider.CompareTag("Player")) continue;

            bool acertou = hit.collider.transform == transform
                        || hit.collider.transform.IsChildOf(transform);
            Debug.Log("[ArrastarCaixa] Primeiro solido: " + hit.collider.name + " acertou=" + acertou);
            if (acertou) return true;
            break;
        }
        return false;
    }

    // ─── Drag ───────────────────────────────────────────────────────────────

    private void IniciarArrastar(int dedoId)
    {
        Debug.Log("[ArrastarCaixa] IniciarArrastar! authority=" + (_netObj != null ? _netObj.HasStateAuthority.ToString() : "sem NetworkObject"));
        _dedoId   = dedoId;
        _arrastando = true;
        _alturaFixa = transform.position.y;

        // Profundidade do objeto na tela: usada no drag em screen-space
        _profundidadeTela = _cam.WorldToScreenPoint(transform.position).z;

        if (_netObj != null && !_netObj.HasStateAuthority)
            _netObj.RequestStateAuthority();

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
        _dedoId     = -1;
        _arrastando = false;

        if (_rb != null)
            _rb.isKinematic = false;
    }

    private void OnDisable()
    {
        if (_arrastando) SoltarArrastar();
    }
}
