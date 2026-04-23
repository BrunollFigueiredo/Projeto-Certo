using UnityEngine;
using TMPro;
public class Movimento : MonoBehaviour
{
    public VariableJoystick joystick;
    float tempo = 60;
    [Header("Configurações de Câmera")]
    [SerializeField] private float sensibilidade = 0.15f;
    public TMP_Text segundos;
    void Update()
    {
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
        segundos.text = tempo.ToString();
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
