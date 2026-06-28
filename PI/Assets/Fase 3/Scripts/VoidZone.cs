using Fusion;
using UnityEngine;

// Coloque num objeto com Trigger bem abaixo do mapa.
// Se o jogador cair no void, ele é teleportado de volta ao spawn.
public class VoidZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        NetworkObject netObj = other.GetComponentInParent<NetworkObject>();
        if (netObj == null || !netObj.HasInputAuthority) return;

        Transform ponto = BasicSpawner.PontoDeSpawn(BasicSpawner.PersonagemLocal);
        if (ponto == null) return;

        NetworkCharacterController cc = other.GetComponentInParent<NetworkCharacterController>();
        if (cc != null)
            cc.Teleport(ponto.position, ponto.rotation);
        else
            other.transform.root.position = ponto.position;

        Debug.Log("[VoidZone] " + BasicSpawner.PersonagemLocal + " caiu no void — respawnando.");
    }
}
