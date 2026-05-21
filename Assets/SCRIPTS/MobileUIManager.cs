using UnityEngine.UI;
using UnityEngine;

public class MobileUIManager : MonoBehaviour
{
    public static MobileUIManager Instance { get; private set; }

    [Header("Botões de Interação")]
    [SerializeField] private Button pickUpButton;
    [SerializeField] private Button dropButton;

    private PlayerInteractor localPlayer;

    private void Awake()
    {
        // Singleton simples para facilitar o acesso do player local
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Esconde ambos os botões no início
        pickUpButton.gameObject.SetActive(false);
        dropButton.gameObject.SetActive(false);
    }

    // Chamado pelo PlayerInteractor quando o player local nasce (Spawned)
    public void RegisterLocalPlayer(PlayerInteractor player)
    {
        localPlayer = player;

        // Configura os eventos de clique dos botões mobile
        pickUpButton.onClick.RemoveAllListeners();
        pickUpButton.onClick.AddListener(() => localPlayer.OnPickUpButtonPressed());

        dropButton.onClick.RemoveAllListeners();
        dropButton.onClick.AddListener(() => localPlayer.OnDropButtonPressed());
    }

    public void ShowPickUpButton(bool show)
    {
        pickUpButton.gameObject.SetActive(show);
    }

    // Alterna entre o estado de "Pegar" e "Largar"
    public void ToggleHoldState(bool isHolding)
    {
        pickUpButton.gameObject.SetActive(!isHolding);
        dropButton.gameObject.SetActive(isHolding);
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
