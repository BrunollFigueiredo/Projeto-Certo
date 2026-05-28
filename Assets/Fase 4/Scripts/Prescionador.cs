using UnityEngine;

public class Prescionador : MonoBehaviour
{
    public GameObject portão2;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Box"))
        {
            portão2.transform.Translate(0, 5, 0);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Box"))
        {
            portão2.transform.Translate(0, -5, 0);
        }
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
