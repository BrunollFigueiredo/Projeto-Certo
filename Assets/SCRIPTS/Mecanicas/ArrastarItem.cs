using UnityEngine;
using UnityEngine.EventSystems;

public class ArrastarItem : MonoBehaviour
{
    [SerializeField] private Transform jogador;
    [SerializeField] private float alcanceMaximo = 3f;

    private bool arrastando = false;
    private int fingerIdArrastando = -1;
    private float alturaObjeto;
    private Rigidbody rb;
    private float tempoCooldown = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (tempoCooldown > 0f)
            tempoCooldown -= Time.deltaTime;

        if (arrastando)
        {
            bool encontrou = false;

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.fingerId != fingerIdArrastando) continue;

                encontrou = true;

                if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
                    MoverObjeto(t.position);

                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                    SoltarObjeto();

                break;
            }

            if (!encontrou)
                SoltarObjeto();

            return;
        }

        if (tempoCooldown > 0f) return;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            if (t.phase != TouchPhase.Began) continue;
            if (EventSystem.current.IsPointerOverGameObject(t.fingerId)) continue;

            Ray ray = Camera.main.ScreenPointToRay(t.position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
            {
                arrastando = true;
                fingerIdArrastando = t.fingerId;
                alturaObjeto = transform.position.y;

                if (rb != null)
                    rb.isKinematic = true;

                break;
            }
        }
    }

    void MoverObjeto(Vector2 posicaoTela)
    {
        Ray ray = Camera.main.ScreenPointToRay(posicaoTela);
        Plane plano = new Plane(Vector3.up, new Vector3(0f, alturaObjeto, 0f));
        float distancia;

        if (plano.Raycast(ray, out distancia))
        {
            Vector3 novaPosicao = ray.GetPoint(distancia);

            if (jogador != null)
            {
                Vector3 direcao = novaPosicao - jogador.position;
                direcao.y = 0f;

                if (direcao.magnitude > alcanceMaximo)
                {
                    direcao = direcao.normalized * alcanceMaximo;
                    novaPosicao = jogador.position + direcao;
                    novaPosicao.y = alturaObjeto;
                }
            }

            transform.position = novaPosicao;
        }
    }

    void SoltarObjeto()
    {
        arrastando = false;
        fingerIdArrastando = -1;
        tempoCooldown = 0.3f;

        if (rb != null)
            rb.isKinematic = false;
    }
}
