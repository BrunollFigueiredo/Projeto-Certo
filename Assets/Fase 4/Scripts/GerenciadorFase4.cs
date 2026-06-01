using Fusion;
using TMPro;
using UnityEngine;

public class GerenciadorFase4 : NetworkBehaviour
{
    public static GerenciadorFase4 Instance { get; private set; }

    [Header("Portas")]
    [SerializeField] private Transform portao1;
    [SerializeField] private Transform portao2;
    [SerializeField] private Transform portao3;
    [SerializeField] private Vector3 offsetAberta = new Vector3(0f, 5f, 0f);

    [Header("Timer")]
    [SerializeField] private float tempoTotal = 300f;
    [SerializeField] private TextMeshProUGUI textoTimer;

    [Header("Spawn")]
    public Transform SpawnInicio;

    // Estado networked
    [Networked] private float TempoRestante { get; set; }
    [Networked] private int BoxesNaArea { get; set; }
    [Networked] private int ItensColocados { get; set; }
    [Networked] private int JogadoresNaFinal { get; set; }
    [Networked] private NetworkBool Portao1Aberto { get; set; }
    [Networked] private NetworkBool Portao2Aberto { get; set; }
    [Networked] private NetworkBool Portao3Aberto { get; set; }

    // Posições originais das portas (salvas no Spawned)
    private Vector3 _posOriginalPortao1;
    private Vector3 _posOriginalPortao2;
    private Vector3 _posOriginalPortao3;

    private void Awake()
    {
        Instance = this;
    }

    public override void Spawned()
    {
        if (portao1 != null) _posOriginalPortao1 = portao1.position;
        if (portao2 != null) _posOriginalPortao2 = portao2.position;
        if (portao3 != null) _posOriginalPortao3 = portao3.position;

        if (HasStateAuthority)
        {
            TempoRestante = tempoTotal;
            BoxesNaArea = 0;
            ItensColocados = 0;
            JogadoresNaFinal = 0;
            Portao1Aberto = false;
            Portao2Aberto = false;
            Portao3Aberto = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        TempoRestante -= Runner.DeltaTime;
        if (TempoRestante <= 0f)
        {
            TempoRestante = tempoTotal;
            RPC_TeleportarTodos();
        }
    }

    public override void Render()
    {
        // Atualiza posição das portas baseado no estado networked
        if (portao1 != null)
            portao1.position = Portao1Aberto
                ? _posOriginalPortao1 + offsetAberta
                : _posOriginalPortao1;

        if (portao2 != null)
            portao2.position = Portao2Aberto
                ? _posOriginalPortao2 + offsetAberta
                : _posOriginalPortao2;

        if (portao3 != null)
            portao3.position = Portao3Aberto
                ? _posOriginalPortao3 + offsetAberta
                : _posOriginalPortao3;

        // Atualiza UI do timer localmente
        if (textoTimer != null)
        {
            int minutos = Mathf.FloorToInt(TempoRestante / 60f);
            int segundos = Mathf.FloorToInt(TempoRestante % 60f);
            textoTimer.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    // Chamado por AreaLimpa: delta = +1 ao entrar, -1 ao sair
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_MudarBoxes(int delta)
    {
        BoxesNaArea = Mathf.Clamp(BoxesNaArea + delta, 0, 99);

        if (BoxesNaArea == 0 && !Portao1Aberto)
            Portao1Aberto = true;
    }

    // Chamado pelos sensores de item: delta = +1 ao colocar, -1 ao remover
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_MudarItens(int delta)
    {
        ItensColocados = Mathf.Clamp(ItensColocados + delta, 0, 3);

        if (ItensColocados >= 3 && !Portao2Aberto)
            Portao2Aberto = true;
    }

    // Chamado por PortaFinal: delta = +1 ao entrar, -1 ao sair
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_MudarJogadoresFinal(int delta)
    {
        JogadoresNaFinal = Mathf.Clamp(JogadoresNaFinal + delta, 0, 2);

        if (JogadoresNaFinal >= 2 && !Portao3Aberto)
            Portao3Aberto = true;
    }

    // Disparado pela StateAuthority para todos quando o timer zera
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_TeleportarTodos()
    {
        Transform spawn = SpawnInicio != null
            ? SpawnInicio
            : BasicSpawner.PontoDeSpawn(BasicSpawner.PersonagemLocal);

        if (spawn == null || Player.LocalTransform == null) return;

        NetworkCharacterController ncc = Player.LocalTransform.GetComponent<NetworkCharacterController>();
        if (ncc != null)
            ncc.Teleport(spawn.position, spawn.rotation);
        else
            Player.LocalTransform.SetPositionAndRotation(spawn.position, spawn.rotation);

        FeedbackUI.Mostrar("Tempo esgotado! Voltando ao início...");
    }
}
