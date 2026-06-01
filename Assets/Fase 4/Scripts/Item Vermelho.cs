using Fusion;
using UnityEngine;

public class ItemVermelho : NetworkBehaviour
{
    [Networked] private NetworkBool ItemColocado { get; set; }

    private Renderer _renderer;

    public override void Spawned()
    {
        _renderer = GetComponent<Renderer>();
        AtualizarCor();
    }

    public override void Render()
    {
        AtualizarCor();
    }

    private void AtualizarCor()
    {
        if (_renderer == null) return;
        _renderer.material.color = ItemColocado ? Color.green : Color.red;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return;
        if (!other.CompareTag("Vermelho")) return;
        if (ItemColocado) return;

        ItemColocado = true;
        GerenciadorFase4.Instance.RPC_MudarItens(1);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!HasStateAuthority) return;
        if (!other.CompareTag("Vermelho")) return;
        if (!ItemColocado) return;

        ItemColocado = false;
        GerenciadorFase4.Instance.RPC_MudarItens(-1);
    }
}
