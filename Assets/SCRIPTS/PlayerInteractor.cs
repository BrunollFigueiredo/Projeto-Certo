using Fusion;
using UnityEngine;

public class PlayerInteractor : NetworkBehaviour
{
    public Transform HoldPoint;
    public Animator animator;
    private Interaçãocoop currentCube;

    public override void Spawned()
    {
        // CORREÇÃO 1: Quem registra o objeto do jogador para a rede é o Host (State Authority)
        if (HasStateAuthority)
        {
            Runner.SetPlayerObject(Object.InputAuthority, Object);
        }

        // O celular do jogador (Input Authority) registra a UI localmente
        if (HasInputAuthority)
        {
            MobileUIManager.Instance.RegisterLocalPlayer(this);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority || currentCube == null) return;

        // Atualiza a visibilidade do botão "Pegar" na UI do celular
        bool isAlreadyHolding = (currentCube.HolderA == this || currentCube.HolderB == this);

        if (!isAlreadyHolding)
        {
            // Só mostra o botão de pegar se houver 2 players perto
            MobileUIManager.Instance.ShowPickUpButton(currentCube.CanShowPickUp());
        }
        else
        {
            // Se já estiver segurando, garante que o botão de "Largar" apareça
            MobileUIManager.Instance.ToggleHoldState(true);
        }
    }

    // --- EVENTOS DA UI MOBILE ---

    public void OnPickUpButtonPressed()
    {
        if (currentCube != null)
        {
            // Opcional: Se quiser a animação, basta descomentar aqui
            // animator.SetTrigger("PickUp");
            currentCube.RPC_ToggleReady(Object.InputAuthority);
        }
    }

    public void OnDropButtonPressed()
    {
        if (currentCube != null)
        {
            // animator.SetTrigger("Drop");
            RPC_RequestCoopDrop(currentCube);
            MobileUIManager.Instance.ToggleHoldState(false);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestCoopDrop(Interaçãocoop cube)
    {
        // Limpa o estado cooperativo no servidor
        cube.HolderA = null;
        cube.HolderB = null;
        cube.PlayersReadyToPick.Clear();
    }

    // --- DETECÇÃO ---

    private void OnTriggerEnter(Collider other)
    {
        if (HasInputAuthority && other.TryGetComponent(out Interaçãocoop cube))
        {
            currentCube = cube;
            cube.RPC_UpdatePresence(Object.InputAuthority, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (HasInputAuthority && other.TryGetComponent(out Interaçãocoop cube))
        {
            // PROTEÇÃO: Se eu estiver atualmente segurando este cubo, eu ignoro a saída do trigger!
            // Isso evita que a UI suma quando o cubo for levantado acima da minha cabeça.
            if (cube.HolderA == this || cube.HolderB == this) return;

            cube.RPC_UpdatePresence(Object.InputAuthority, false);

            if (currentCube == cube)
            {
                currentCube = null;
                MobileUIManager.Instance.ShowPickUpButton(false);
                MobileUIManager.Instance.ToggleHoldState(false);
            }
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
