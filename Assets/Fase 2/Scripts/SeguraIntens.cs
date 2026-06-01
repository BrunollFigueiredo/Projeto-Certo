using Fusion;
using UnityEngine;

// Permite arrastar o objeto pela tela usando o toque do mobile
public class SeguraIntens : MonoBehaviour
{
    private Vector2 offset;            // Diferença entre toque e centro do objeto
    private bool isDragging;           // Se o objeto está sendo arrastado
    private Rigidbody rb;              // Rigidbody do objeto
    private Vector3 touchPosition;     // Posição do toque convertida para o mundo
    private NetworkObject netObj;      // NetworkObject do item (se for em rede)

    void Start()
    {
        // Pega o Rigidbody do objeto
        rb = GetComponent<Rigidbody>();
        netObj = GetComponent<NetworkObject>();
    }

    void Update()
    {
        // Só faz algo se tem pelo menos um dedo na tela
        if (Input.touchCount <= 0) return;

        Camera cam = Player.LocalCamera != null ? Player.LocalCamera : Camera.main;
        if (cam == null) return;

        Touch touch = Input.GetTouch(0);

        // Converte a posição do toque (pixels) para coordenadas do mundo
        Vector3 touchPositionPixels = new Vector3(
            touch.position.x,
            touch.position.y,
            cam.WorldToScreenPoint(rb.position).z
        );

        touchPosition = cam.ScreenToWorldPoint(touchPositionPixels);

        // Cria um raio do dedo para o mundo (usado no início do toque)
        Ray ray = cam.ScreenPointToRay(touch.position);
        RaycastHit hit;

        // Quando o toque começa: verifica se acertou o objeto
        if (touch.phase == TouchPhase.Began)
        {
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
            {
                isDragging = true;
                // Salva a diferença entre o objeto e o toque
                offset.x = rb.position.x - touchPosition.x;
                offset.y = rb.position.y - touchPosition.y;
                rb.useGravity = false;

                // Pede autoridade na rede para o arrasto sincronizar
                if (netObj != null && !netObj.HasStateAuthority)
                {
                    netObj.RequestStateAuthority();
                }
            }
        }
        // Enquanto o dedo se move: arrasta o objeto
        else if (touch.phase == TouchPhase.Moved)
        {
            if (isDragging)
            {
                Vector3 targetPosition = touchPosition + new Vector3(0f, offset.y, 0f);
                rb.MovePosition(targetPosition);
            }
        }
        // Quando o toque acaba: solta o objeto e religa a gravidade
        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            isDragging = false;
            rb.useGravity = true;
        }
    }
}
