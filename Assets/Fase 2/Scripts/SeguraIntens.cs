using UnityEngine;

public class SeguraIntens : MonoBehaviour
{
    private Vector2 offset;

    private bool isDragging;

    private Rigidbody rb;

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

            Vector3 touchPositionPixels = new Vector3(

            touch.position.x,

            touch.position.y,

            Camera.main.WorldToScreenPoint(rb.position).z

            );

            touchPosition = Camera.main.ScreenToWorldPoint(touchPositionPixels);

            Ray ray = Camera.main.ScreenPointToRay(touch.position);

            RaycastHit hit;
            switch (touch.phase)
            {

                case TouchPhase.Began:

                    if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
                    {

                        isDragging = true;

                        offset.x = rb.position.x - touchPosition.x;

                        offset.y = rb.position.y - touchPosition.y;
                        rb.useGravity = false;
                    }
                    break;

                case TouchPhase.Moved:

                    if (isDragging)
                    {

                        Vector3 targetPosition = touchPosition + new Vector3(0f, offset.y, 0f);

                        rb.MovePosition(targetPosition);

                    }
                    break;
                case TouchPhase.Ended:

                case TouchPhase.Canceled:

                    isDragging = false;
                    rb.useGravity = true;
                    break;

            }

        }
    }
}
