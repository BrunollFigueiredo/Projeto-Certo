using UnityEngine;

public class PortaFinal : MonoBehaviour
{
    public GameObject Portão3;
    float cont = 0;
    public float contF = 2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cont++;
            if (cont == contF)
            {
                Portão3.transform.Translate(0, 40, 0);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cont--;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
