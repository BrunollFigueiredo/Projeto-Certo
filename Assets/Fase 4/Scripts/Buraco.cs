using Fusion;
using UnityEngine;

public class Buraco : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj == null || !netObj.HasInputAuthority) return;

        Transform spawn = null;

        if (GerenciadorFase4.Instance != null && GerenciadorFase4.Instance.SpawnInicio != null)
            spawn = GerenciadorFase4.Instance.SpawnInicio;
        else
            spawn = BasicSpawner.PontoDeSpawn(BasicSpawner.PersonagemLocal);

        if (spawn == null) return;

        NetworkCharacterController ncc = other.GetComponent<NetworkCharacterController>();
        if (ncc != null)
            ncc.Teleport(spawn.position, spawn.rotation);
        else if (Player.LocalTransform != null)
            Player.LocalTransform.position = spawn.position;
    }
}
