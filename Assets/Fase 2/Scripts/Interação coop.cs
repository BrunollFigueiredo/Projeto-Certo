using UnityEngine;
using Fusion;

// Objeto que precisa de dois jogadores juntos para pegar e carregar
public class Interaçãocoop : NetworkBehaviour
{
    [SerializeField] private GameObject floatingBalloonUI; // Balão de UI flutuante mostrado quando alguém está perto
    [SerializeField] private Rigidbody rb;                 // Rigidbody do objeto
    [SerializeField] private float heightOffset = 1.5f;    // Altura acima da mão dos jogadores

    // Lista sincronizada dos jogadores que estão na área do objeto
    [Networked, Capacity(2)]
    public NetworkDictionary<PlayerRef, NetworkBool> PlayersInRange => default;

    // Lista sincronizada dos jogadores que apertaram "pegar"
    [Networked, Capacity(2)]
    public NetworkDictionary<PlayerRef, NetworkBool> PlayersReadyToPick => default;

    // Referências aos dois jogadores que estão segurando o objeto
    [Networked] public PlayerInteractor HolderA { get; set; }
    [Networked] public PlayerInteractor HolderB { get; set; }

    public override void Render()
    {
        // Mostra ou esconde o balão dependendo de ter alguém perto
        if (floatingBalloonUI != null)
        {
            floatingBalloonUI.SetActive(PlayersInRange.Count > 0);
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Se os dois jogadores estão segurando, levita o objeto entre eles
        if (HolderA != null && HolderB != null)
        {
            // Desliga a física para o objeto não cair durante o transporte
            rb.isKinematic = true;

            // Só o host calcula a posição (o NetworkTransform sincroniza nos clientes)
            if (HasStateAuthority)
            {
                // Posição = média entre as mãos dos dois jogadores, com offset para cima
                Vector3 targetPos = (HolderA.HoldPoint.position + HolderB.HoldPoint.position) / 2;
                transform.position = Vector3.Lerp(transform.position, targetPos + (Vector3.up * heightOffset), Runner.DeltaTime * 10f);
                // Acompanha a rotação do jogador A
                transform.rotation = Quaternion.Slerp(transform.rotation, HolderA.transform.rotation, Runner.DeltaTime * 5f);
            }
        }
        else
        {
            // Quando soltarem, religa a física para o objeto voltar a cair
            if (rb.isKinematic)
            {
                rb.isKinematic = false;
            }
        }
    }

    // RPC: atualiza a presença de um jogador na área (entrou ou saiu)
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_UpdatePresence(PlayerRef player, bool isIn)
    {
        if (isIn)
        {
            // Adiciona o jogador na lista se ainda não está
            if (!PlayersInRange.ContainsKey(player))
            {
                PlayersInRange.Add(player, true);
            }
        }
        else
        {
            // Remove de ambas as listas se saiu
            PlayersInRange.Remove(player);
            PlayersReadyToPick.Remove(player);
        }
    }

    // RPC: alterna o status "pronto para pegar" de um jogador
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ToggleReady(PlayerRef player)
    {
        // Liga/desliga o estado de pronto desse jogador
        if (PlayersReadyToPick.ContainsKey(player))
        {
            PlayersReadyToPick.Remove(player);
        }
        else
        {
            PlayersReadyToPick.Add(player, true);
        }

        // Se os dois apertaram e ambos estão na área, define quem segura o objeto
        if (PlayersReadyToPick.Count >= 2 && PlayersInRange.Count >= 2)
        {
            int count = 0;

            foreach (var kvp in PlayersReadyToPick)
            {
                NetworkObject playerObj = Runner.GetPlayerObject(kvp.Key);

                if (playerObj != null)
                {
                    // O primeiro vira HolderA, o segundo vira HolderB
                    if (count == 0)
                    {
                        HolderA = playerObj.GetComponent<PlayerInteractor>();
                    }
                    else if (count == 1)
                    {
                        HolderB = playerObj.GetComponent<PlayerInteractor>();
                    }
                    count++;
                }

                if (count >= 2) break;
            }
        }
    }

    // Verifica se já tem 2 jogadores na área (usado para mostrar o botão de pegar)
    public bool CanShowPickUp()
    {
        return PlayersInRange.Count >= 2;
    }

    void Start()
    {

    }

    void Update()
    {

    }
}
