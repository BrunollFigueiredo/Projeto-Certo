using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class PainelPrensa : MonoBehaviour
{
    [Header("Prensa")]
    [SerializeField] private Prensa prensa;

    [Header("UI")]
    [SerializeField] private GameObject painelUI;
    [SerializeField] private TextMeshProUGUI textoEscalaSelecionada;
    [SerializeField] private Button botaoConfirmar;
    [SerializeField] private Button botaoEnviar;

    [Header("Esteira")]
    [SerializeField] private float velocidadeEsteira = 4f;
    [SerializeField] private Transform destino;

    [Header("Opções de Escala")]
    [SerializeField] private Vector3 escalaPequena = new Vector3(0.5f, 0.5f, 0.5f);
    [SerializeField] private Vector3 escalaMedia  = new Vector3(1f, 1f, 1f);
    [SerializeField] private Vector3 escalaGrande = new Vector3(1.5f, 1.5f, 1.5f);

    private Vector3 escalaEscolhida;
    private bool uiAberta = false;
    private Camera cam;

    void Start()
    {
        escalaEscolhida = escalaMedia;
        painelUI.SetActive(false);
        AtualizarTexto();
    }

    void Update()
    {
        if (uiAberta)
        {
            bool temObjeto = prensa != null && ObjetoNaPrensa();
            botaoConfirmar.interactable = temObjeto;
            botaoEnviar.interactable = temObjeto;
            return;
        }

        if (cam == null)
        {
            cam = Player.LocalCamera != null ? Player.LocalCamera : Camera.main;
            if (cam == null) return;
        }

        if (!DetectouToque(out Vector2 posicao)) return;

        Ray ray = cam.ScreenPointToRay(posicao);
        RaycastHit[] hits = Physics.RaycastAll(ray, 30f);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.isTrigger) continue;
            if (hit.collider.CompareTag("Player")) continue;

            bool acertouPainel = hit.collider.transform == transform
                              || hit.collider.transform.IsChildOf(transform);
            if (acertouPainel)
                AbrirUI();
            break;
        }
    }

    void AbrirUI()
    {
        uiAberta = true;
        painelUI.SetActive(true);
    }

    public void FecharUI()
    {
        uiAberta = false;
        painelUI.SetActive(false);
    }

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

    public void Confirmar()
    {
        prensa.Ativar(escalaEscolhida);
        FecharUI();
    }

    public void Enviar()
    {
        Debug.Log("[Enviar] Botão clicado.");

        if (prensa == null) { Debug.LogWarning("[Enviar] Prensa não referenciada."); return; }
        if (prensa.PontoDoObjeto == null) { Debug.LogWarning("[Enviar] PontoDoObjeto não referenciado."); return; }
        if (!prensa.PontoDoObjeto.Ocupado) { Debug.LogWarning("[Enviar] Nenhum objeto no ponto."); return; }

        StartCoroutine(SequenciaEnviar());
        FecharUI();
    }

    IEnumerator SequenciaEnviar()
    {
        yield return prensa.Levantar();

        Transform obj = prensa.PontoDoObjeto.LiberarObjeto();
        if (obj == null) { Debug.LogWarning("[Enviar] LiberarObjeto retornou null."); yield break; }

        if (destino == null) { Debug.LogWarning("[Enviar] Destino não definido no Inspector."); yield break; }

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null) { Debug.LogWarning("[Enviar] Objeto sem Rigidbody."); yield break; }

        rb.isKinematic = false;
        yield return new WaitForFixedUpdate(); // aguarda physics aceitar o estado não-kinematic

        Vector3 dir = destino.position - obj.position;
        dir.y = 0f;
        dir = dir.normalized;

        rb.linearVelocity = dir * velocidadeEsteira;
        Debug.Log($"[Enviar] {obj.name} lançado na direção {dir} com velocidade {velocidadeEsteira}");
    }

    void AtualizarTexto()
    {
        if (textoEscalaSelecionada != null)
            textoEscalaSelecionada.text = $"Escala: {escalaEscolhida.x:0.#}x";
    }

    bool ObjetoNaPrensa()
    {
        return prensa.PontoDoObjeto != null && prensa.PontoDoObjeto.Ocupado;
    }

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
}
