using UnityEngine;

public class Alavanca2 : MonoBehaviour
{
    public GameObject plataforma2;
    float cont = 0;
    public GameObject baseA;
    public GameObject baseB;
    Rigidbody rb;
    private Vector3 touchPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchPositionPixels = new Vector3(touch.position.x, touch.position.y, Camera.main.WorldToScreenPoint(rb.position).z

 );
            touchPosition = Camera.main.ScreenToWorldPoint(touchPositionPixels);

            Ray ray = Camera.main.ScreenPointToRay(touch.position);

            RaycastHit hit;
            switch (touch.phase)
            {
                case TouchPhase.Began:

                    if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
                    {
                        cont++;
                    }
                    break;
            }
            if (cont > 0 && cont < 2)
            {
                if (plataforma2.transform.position.z > baseA.transform.position.z)
                {
                    plataforma2.transform.Translate(0, 0, -2);
                }
            }
            if (cont >= 2)
            {
                if (plataforma2.transform.position.z < baseB.transform.position.z)
                {
                    plataforma2.transform.Translate(0, 0, 2);
                }
                else
                {
                    cont = 0;
                }
            }
        }
    }
}
