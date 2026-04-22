using UnityEngine;

public class Movimento : MonoBehaviour
{
    public VariableJoystick joystick;

    [Header("Configurações de Câmera")]
    [SerializeField] private float sensibilidade = 0.15f;

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
    }

    public void Pulo()
    {
        BasicSpawner.JumpPressed = true;
    }
}
