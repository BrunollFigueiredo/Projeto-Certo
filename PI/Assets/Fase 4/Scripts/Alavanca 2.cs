using Fusion;
using UnityEngine;

public class Alavanca2 : NetworkBehaviour
{
    [SerializeField] private Transform plataforma;   // Plataforma do OUTRO jogador
    [SerializeField] private Transform baseOrigem;   // Posição inicial da plataforma
    [SerializeField] private Transform baseDestino;  // Posição quando a alavanca está sendo segurada
    [SerializeField] private float velocidade = 3f;

    [Networked] private NetworkBool Segurando { get; set; }

    private int _dedoId = -1;
    private bool _estavaSegurandoLocal = false;

    public override void FixedUpdateNetwork()
    {
        if (plataforma == null) return;

        Vector3 alvo = Segurando ? baseDestino.position : baseOrigem.position;
        plataforma.position = Vector3.MoveTowards(
            plataforma.position,
            alvo,
            velocidade * Runner.DeltaTime
        );
    }

    private void Update()
    {
        Camera cam = Player.LocalCamera != null ? Player.LocalCamera : Camera.main;
        if (cam == null) return;

        bool segurando = false;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);

            if (t.phase == TouchPhase.Began)
            {
                Ray ray = cam.ScreenPointToRay(t.position);
                if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
                    _dedoId = t.fingerId;
            }

            if (t.fingerId == _dedoId)
            {
                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                    _dedoId = -1;
                else
                    segurando = true;
            }
        }

#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
                segurando = true;
        }
#endif

        if (segurando != _estavaSegurandoLocal)
        {
            _estavaSegurandoLocal = segurando;
            RPC_SetSegurando(segurando);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_SetSegurando(NetworkBool segurando)
    {
        Segurando = segurando;
    }
}
