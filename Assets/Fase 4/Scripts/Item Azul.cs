using UnityEngine;

public class ItemAzul : MonoBehaviour
{
    Renderer cor;
    public GameObject Portão2;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Azul"))
        {
            cor.material.color = Color.green;
            Portão2.transform.Translate(0, 5, 0);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Azul"))
        {
            cor.material.color = Color.blue;
            Portão2.transform.Translate(0, -5, 0);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       cor =(Renderer)GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
