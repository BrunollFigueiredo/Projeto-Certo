using UnityEngine.UI;
using UnityEngine;

public class MobileUIManager : MonoBehaviour
{
    public static MobileUIManager Instance { get; private set; }

    [Header("Bot�es de Intera��o")]
    [SerializeField] private Button pickUpButton;
    [SerializeField] private Button dropButton;

    private PlayerInteractor localPlayer;

    private void Awake()
    {
        // Singleton simples para facilitar o acesso do player local
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (pickUpButton != null) pickUpButton.gameObject.SetActive(false);
        if (dropButton != null) dropButton.gameObject.SetActive(false);
    }

    // Chamado pelo PlayerInteractor quando o player local nasce (Spawned)
    public void RegisterLocalPlayer(PlayerInteractor player)
    {
        localPlayer = player;

        // Configura os eventos de clique dos bot�es mobile
        pickUpButton.onClick.RemoveAllListeners();
        pickUpButton.onClick.AddListener(() => localPlayer.OnPickUpButtonPressed());

        dropButton.onClick.RemoveAllListeners();
        dropButton.onClick.AddListener(() => localPlayer.OnDropButtonPressed());
    }

    public void ShowPickUpButton(bool show)
    {
        if (pickUpButton != null) pickUpButton.gameObject.SetActive(show);
    }

    public void ToggleHoldState(bool isHolding)
    {
        if (pickUpButton != null) pickUpButton.gameObject.SetActive(!isHolding);
        if (dropButton != null) dropButton.gameObject.SetActive(isHolding);
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
