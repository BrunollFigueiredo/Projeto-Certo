using UnityEngine;

public class PegarObjeto : MonoBehaviour
{
    public GameObject visão1, visão2;
    public Transform objetoT, cameraT;
    public bool interagivel, pegar;
    public Rigidbody objetoRigidbody;
    float cont = 0f;
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Camera"))
        {
            visão1.SetActive(false);
            visão2.SetActive(true);
            interagivel = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Camera"))
        {
            if (pegar == false)
            {
                visão1.SetActive(true);
                visão2.SetActive(false);
                interagivel = false;
            }
        }
    }
    public void Segurar()
    {
        if (interagivel == true)
        {
            cont++;
            if (cont == 1)
            {
                objetoT.parent = cameraT;
                objetoRigidbody.useGravity = false;
                pegar = true;
            }
            if (cont == 2)
            {
                objetoT.parent = null;
                objetoRigidbody.useGravity = true;
                pegar = false;
                cont = 0;
            }
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
