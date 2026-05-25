using UnityEngine;
using UnityEngine.EventSystems;

public class ArrastarItem : MonoBehaviour
{
    private static ArrastarItem objetoSendoSeguro = null;

    private Transform jogador;
    private Transform pontoMao;
    private Camera cam;
    private bool segurandoEsteObjeto = false;
    private Rigidbody rb;
    private float tempoCooldown = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        // Corrige escala negativa que quebra o BoxCollider
        Vector3 s = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
    }

    void Update()
    {
        if (tempoCooldown > 0f)
            tempoCooldown -= Time.deltaTime;

        if (jogador == null)
        {
            jogador = Player.LocalTransform;
            if (jogador == null) return;
            pontoMao = Player.LocalPontoMao != null ? Player.LocalPontoMao : jogador;
            cam = Player.LocalCamera != null ? Player.LocalCamera : Camera.main;
            Debug.Log($"[ArrastarItem] Jogador encontrado. PontoMao={pontoMao.name} Camera={cam?.name}");
        }

        if (segurandoEsteObjeto)
        {
            AtualizarPosicaoNaMao();
            if (DetectouToque(out _))
                SoltarObjeto();
            return;
        }

        if (tempoCooldown > 0f) return;
        if (objetoSendoSeguro != null) return;
        if (!CompareTag("Pegavel")) return;

        if (DetectouToque(out Vector2 posicaoToque))
            TentarPegarObjeto(posicaoToque);
    }

    // Abstrai touch (mobile) e mouse (editor) num único retorno
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

    void TentarPegarObjeto(Vector2 posicaoToque)
    {
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(posicaoToque);
        RaycastHit[] hits = Physics.RaycastAll(ray, 30f);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.isTrigger) continue;
            if (hit.collider.CompareTag("Player")) continue;

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

    void AtualizarPosicaoNaMao()
    {
        transform.position = pontoMao.position;
        transform.rotation = pontoMao.rotation;
    }

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

    void SoltarObjeto()
    {
        segurandoEsteObjeto = false;
        objetoSendoSeguro = null;

        PontoDeEncaixe[] pontos = FindObjectsByType<PontoDeEncaixe>(FindObjectsSortMode.None);
        foreach (PontoDeEncaixe ponto in pontos)
        {
            if (!ponto.AceitaObjeto(gameObject)) continue;

            float distancia = Vector3.Distance(transform.position, ponto.transform.position);
            if (distancia <= ponto.RaioDeSnap)
            {
                ponto.EncaixarObjeto(transform);
                return;
            }
        }

        tempoCooldown = 0.4f;

        if (rb != null)
            rb.isKinematic = false;
    }

    void OnDisable()
    {
        if (segurandoEsteObjeto)
            SoltarObjeto();
    }
}
