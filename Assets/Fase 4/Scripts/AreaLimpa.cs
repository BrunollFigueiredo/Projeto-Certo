using UnityEngine;

public class AreaLimpa : MonoBehaviour
{
    public GameObject Portão1;
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Box"))
        {
            
          Portão1.transform.Translate(0, 5, 0);
            
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Box"))
        {
            Portão1.transform.Translate(0, -5, 0);
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
