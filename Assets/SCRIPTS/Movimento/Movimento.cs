using UnityEngine;
using TMPro;

public class Movimento : MonoBehaviour
{
    public VariableJoystick joystick;
    float tempo = 120f;

    [SerializeField] private float sensibilidade = 0.15f;

    [SerializeField] private GameObject painelJoystick;
    [SerializeField] private GameObject botaoPulo;
    [SerializeField] private TMP_Text textoTempo;

    private bool uiAtivada = false;

    void Start()
    {
        if (painelJoystick != null) painelJoystick.SetActive(false);
        if (botaoPulo != null) botaoPulo.SetActive(false);
        if (textoTempo != null) textoTempo.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!Player.LocalSpawnou) return;

        if (!uiAtivada)
        {
            uiAtivada = true;
            if (painelJoystick != null) painelJoystick.SetActive(true);
            if (botaoPulo != null) botaoPulo.SetActive(true);
            if (textoTempo != null) textoTempo.gameObject.SetActive(true);
        }

        if (joystick != null)
        {
            Vector2 direction = new Vector2(joystick.Horizontal, joystick.Vertical);
            BasicSpawner.TouchMoveInput = direction;
        }

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

        tempo -= Time.deltaTime;
        if (textoTempo != null) textoTempo.text = Mathf.CeilToInt(tempo).ToString();
        if (tempo <= 0)
        {
            Debug.Log("Você Perdeu!");
        }
    }

    public void Pulo()
    {
        BasicSpawner.JumpPressed = true;
    }
}
