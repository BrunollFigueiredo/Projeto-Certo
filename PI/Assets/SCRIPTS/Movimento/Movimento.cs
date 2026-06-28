using UnityEngine;

// Controla a entrada do jogador no celular: joystick pra andar, arrastar pra
// olhar e botão de pulo. O cronômetro da fase fica por conta do PhaseTimer.
public class Movimento : MonoBehaviour
{
    public VariableJoystick joystick;

    [SerializeField] private float sensibilidade = 0.15f;

    [SerializeField] private GameObject painelJoystick;
    [SerializeField] private GameObject botaoPulo;

    private bool uiAtivada = false;

    void Start()
    {
        // Esconde os controles até o jogador nascer
        if (painelJoystick != null) painelJoystick.SetActive(false);
        if (botaoPulo != null) botaoPulo.SetActive(false);
    }

    void Update()
    {
        if (!Player.LocalSpawnou) return;

        // Mostra os controles na primeira vez que o jogador existe
        if (!uiAtivada)
        {
            uiAtivada = true;
            if (painelJoystick != null) painelJoystick.SetActive(true);
            if (botaoPulo != null) botaoPulo.SetActive(true);
        }

        // Joystick -> direção de andar
        if (joystick != null)
        {
            Vector2 direction = new Vector2(joystick.Horizontal, joystick.Vertical);
            BasicSpawner.TouchMoveInput = direction;
        }

        // Arrastar no lado direito da tela -> girar a câmera
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.position.x > Screen.width / 2)
                {
                    if (touch.phase == TouchPhase.Moved)
                    {
                        BasicSpawner.YawInput += touch.deltaPosition.x * sensibilidade;
                        BasicSpawner.PitchInput -= touch.deltaPosition.y * sensibilidade;
                        BasicSpawner.PitchInput = Mathf.Clamp(BasicSpawner.PitchInput, -80f, 80f);
                    }
                }
            }
        }
    }

    public void Pulo()
    {
        BasicSpawner.JumpPressed = true;
    }
}
