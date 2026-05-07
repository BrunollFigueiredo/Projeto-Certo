using UnityEngine;

public class BarraProgresso : MonoBehaviour
{
    public static BarraProgresso Instance;

    [SerializeField] private RectTransform barraPreenchimento;
    [SerializeField] private int totalCarvaoNecessario = 5;

    private int carvaoAtual = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        AtualizarBarra();
    }

    public void AdicionarCarvao()
    {
        if (carvaoAtual >= totalCarvaoNecessario) return;

        carvaoAtual = carvaoAtual + 1;
        AtualizarBarra();
    }

    void AtualizarBarra()
    {
        if (barraPreenchimento == null) return;

        float progresso = (float)carvaoAtual / (float)totalCarvaoNecessario;
        barraPreenchimento.localScale = new Vector3(progresso, 1f, 1f);
    }

    public bool EstaCheia()
    {
        return carvaoAtual >= totalCarvaoNecessario;
    }
}
