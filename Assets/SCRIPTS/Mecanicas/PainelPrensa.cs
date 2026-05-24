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
    [SerializeField] private float velocidadeEsteira = 3f;
    [SerializeField] private float aceleracaoEsteira = 2f;

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
        if (uiAberta) return;

        if (cam == null)
        {
            cam = Player.LocalCamera != null ? Player.LocalCamera : Camera.main;
            if (cam == null) return;
        }

        if (!DetectouToque(out Vector2 posicao)) return;

        Ray ray = cam.ScreenPointToRay(posicao);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == gameObject)
            {
                AbrirUI();
                break;
            }
        }
    }

    void AbrirUI()
    {
        uiAberta = true;
        painelUI.SetActive(true);

        bool temObjeto = prensa != null && ObjetoNaPrensa();
        botaoConfirmar.interactable = temObjeto;
        botaoEnviar.interactable = temObjeto;
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

        Transform obj = prensa.PontoDoObjeto.LiberarObjeto();
        if (obj == null) { Debug.LogWarning("[Enviar] LiberarObjeto retornou null."); return; }

        Debug.Log($"[Enviar] Enviando objeto: {obj.name}");
        StartCoroutine(AcelerarParaEsteira(obj));
        FecharUI();
    }

    IEnumerator AcelerarParaEsteira(Transform obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null) { Debug.LogWarning("[Enviar] Objeto não tem Rigidbody."); yield break; }

        rb.isKinematic = false;
        Debug.Log($"[Enviar] Acelerando {obj.name} para Z={velocidadeEsteira}");

        while (rb.linearVelocity.z < velocidadeEsteira)
        {
            float vz = Mathf.MoveTowards(rb.linearVelocity.z, velocidadeEsteira, aceleracaoEsteira * Time.deltaTime);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, vz);
            yield return null;
        }

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, velocidadeEsteira);
        Debug.Log("[Enviar] Velocidade final atingida.");
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
