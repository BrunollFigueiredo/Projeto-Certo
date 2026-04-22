using UnityEngine;

public class BotaoInteragir : MonoBehaviour
{
    public float alcance = 3f;

    public void Interagir()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, alcance))
        {
            instante livro = hit.collider.GetComponent<instante>();
            if (livro != null)
            {
                livro.Interagir();
            }
        }
    }
}
