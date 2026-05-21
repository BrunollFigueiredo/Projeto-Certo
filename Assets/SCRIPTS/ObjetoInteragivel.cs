using Fusion;
using UnityEngine;

public class ObjetoInteragivel : NetworkBehaviour
{
    [Header("Referências Visuais")]
    [SerializeField] private GameObject floatingBalloonUI;
    [SerializeField] private Collider itemCollider;
    [SerializeField] private Rigidbody rb;

    // Variável sincronizada na rede para saber quem está segurando o objeto
    [Networked] public PlayerInteractor CurrentHolder { get; set; }

    public override void Spawned()
    {
        floatingBalloonUI.SetActive(false);
    }

    // Método chamado apenas na máquina do player que se aproximou
    public void ToggleFloatingBalloon(bool show)
    {
        floatingBalloonUI.SetActive(show);
    }

    public override void FixedUpdateNetwork()
    {
        // Se alguém estiver segurando o objeto, ele acompanha o player
        if (CurrentHolder != null)
        {
            rb.isKinematic = true; // Desativa a física para não brigar com o player
            itemCollider.enabled = false; // Evita colisões estranhas com o CharacterController

            // Move suavemente para a posição "acima da cabeça" do player
            transform.position = Vector3.Lerp(transform.position, CurrentHolder.HoldPoint.position, Runner.DeltaTime * 15f);
            transform.rotation = Quaternion.Lerp(transform.rotation, CurrentHolder.HoldPoint.rotation, Runner.DeltaTime * 15f);
        }
        else
        {
            // Se foi solto, a física volta a atuar e ele cai no chão
            rb.isKinematic = false;
            itemCollider.enabled = true;
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
