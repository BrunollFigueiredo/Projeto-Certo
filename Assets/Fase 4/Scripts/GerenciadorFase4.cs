using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GerenciadorFase4 : NetworkBehaviour
{
    public static GerenciadorFase4 Instance { get; private set; }

    [Header("Porta")]
    [SerializeField] private Transform porta;                       // Porta unica que abre ao completar as cores
    [SerializeField] private Vector3 offsetAberta = new Vector3(0f, 5f, 0f);

    [Header("Fim de fase")]
    [SerializeField] private string cenaFinal = "CenaFinal";        // Cutscene final (precisa estar no Build Settings)
    [SerializeField] private float delayFim = 2f;                   // Tempo mostrando a porta aberta antes de trocar de cena

    [Header("Timer")]
    [SerializeField] private float tempoTotal = 300f;
    [SerializeField] private TextMeshProUGUI textoTimer;

    [Header("Spawn")]
    public Transform SpawnInicio;

    // Estado networked
    [Networked] private float TempoRestante { get; set; }
    [Networked] private int ItensColocados { get; set; }
    [Networked] private NetworkBool PortaAberta { get; set; }

    // Posicao original da porta (salva no Spawned)
    private Vector3 _posOriginalPorta;
    private bool _terminou; // trava local para nao disparar a transicao duas vezes

    private void Awake()
    {
        Instance = this;
    }

    public override void Spawned()
    {
        if (porta != null) _posOriginalPorta = porta.position;

        if (HasStateAuthority)
        {
            TempoRestante = tempoTotal;
            ItensColocados = 0;
            PortaAberta = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        if (PortaAberta) return; // fase vencida: o timer para de contar

        TempoRestante -= Runner.DeltaTime;
        if (TempoRestante <= 0f)
        {
            TempoRestante = 0f;
            RPC_ReiniciarFase();
        }
    }

    public override void Render()
    {
        // Atualiza posicao da porta baseado no estado networked
        if (porta != null)
            porta.position = PortaAberta
                ? _posOriginalPorta + offsetAberta
                : _posOriginalPorta;

        // Atualiza UI do timer localmente
        if (textoTimer != null)
        {
            int minutos = Mathf.FloorToInt(TempoRestante / 60f);
            int segundos = Mathf.FloorToInt(TempoRestante % 60f);
            textoTimer.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    // Chamado pelos sensores de cor (ItemAzul/ItemRoxo/ItemVermelho):
    // delta = +1 ao colocar a cor certa, -1 ao remover.
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_MudarItens(int delta)
    {
        if (PortaAberta) return;

        ItensColocados = Mathf.Clamp(ItensColocados + delta, 0, 3);

        // As 3 cores no lugar = vitoria: abre a porta e termina o jogo.
        if (ItensColocados >= 3)
        {
            PortaAberta = true;
            RPC_TerminarFase();
        }
    }

    // Vitoria: a porta ja abre via PortaAberta no Render; aqui levamos todos
    // os jogadores ao cutscene final apos um pequeno delay.
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_TerminarFase()
    {
        if (_terminou) return;
        _terminou = true;

        FeedbackUI.Mostrar("Sequencia completa! A porta se abriu...");
        TransicaoFase.Ir(Runner, cenaFinal, delayFim);
    }

    // Timer zerou: reinicia a fase recarregando a cena atual em todos.
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ReiniciarFase()
    {
        FeedbackUI.Mostrar("Tempo esgotado! Reiniciando a fase...");
        TransicaoFase.Ir(Runner, SceneManager.GetActiveScene().name, 1.5f);
    }
}
