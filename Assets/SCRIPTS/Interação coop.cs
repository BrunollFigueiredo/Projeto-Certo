using UnityEngine;
using Fusion;
using static Unity.Collections.Unicode;
public class Interaçãocoop : NetworkBehaviour
{
    [Header("Configurações")]
    [SerializeField] private GameObject floatingBalloonUI;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float heightOffset = 1.5f;
    // --- VARIÁVEIS SINCRONIZADAS ---

    [Networked, Capacity(2)]
    public NetworkDictionary<PlayerRef, NetworkBool> PlayersInRange => default;

    [Networked, Capacity(2)]
    public NetworkDictionary<PlayerRef, NetworkBool> PlayersReadyToPick => default;

    [Networked] public PlayerInteractor HolderA { get; set; }
    [Networked] public PlayerInteractor HolderB { get; set; }

    public override void Render()
    {
        if (floatingBalloonUI != null)
            floatingBalloonUI.SetActive(PlayersInRange.Count > 0);
    }

    public override void FixedUpdateNetwork()
    {
        if (HolderA != null && HolderB != null)
        {
            // Desliga a gravidade e colisões para TODOS (Host e Cliente)
            rb.isKinematic = true;

            // Mas SOMENTE O HOST move o cubo de fato.
            // O componente "NetworkTransform" vai levar essa posição até o Cliente automaticamente.
            if (HasStateAuthority)
            {
                Vector3 targetPos = (HolderA.HoldPoint.position + HolderB.HoldPoint.position) / 2;
                transform.position = Vector3.Lerp(transform.position, targetPos + (Vector3.up * heightOffset), Runner.DeltaTime * 10f);
                transform.rotation = Quaternion.Slerp(transform.rotation, HolderA.transform.rotation, Runner.DeltaTime * 5f);
            }
        }
        else
        {
            // Quando soltarem, a física volta para todo mundo
            if (rb.isKinematic) rb.isKinematic = false;
        }
    }

    // --- MÉTODOS DE REDE ---

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_UpdatePresence(PlayerRef player, bool isIn)
    {
        if (isIn)
        {
            if (!PlayersInRange.ContainsKey(player)) PlayersInRange.Add(player, true);
        }
        else
        {
            PlayersInRange.Remove(player);
            PlayersReadyToPick.Remove(player);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ToggleReady(PlayerRef player)
    {
        if (PlayersReadyToPick.ContainsKey(player))
            PlayersReadyToPick.Remove(player);
        else
            PlayersReadyToPick.Add(player, true);

        if (PlayersReadyToPick.Count >= 2 && PlayersInRange.Count >= 2)
        {
            int count = 0;
            // CORREÇÃO 2: Iterar sobre quem votou (PlayersReadyToPick) e não sobre todos na área
            foreach (var kvp in PlayersReadyToPick)
            {
                NetworkObject playerObj = Runner.GetPlayerObject(kvp.Key);

                if (playerObj != null)
                {
                    if (count == 0) HolderA = playerObj.GetComponent<PlayerInteractor>();
                    else if (count == 1) HolderB = playerObj.GetComponent<PlayerInteractor>();
                    count++;
                }

                if (count >= 2) break;
            }
        }
    }

    public bool CanShowPickUp() => PlayersInRange.Count >= 2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
