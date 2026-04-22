using UnityEngine;
using UnityEngine.UI;

public class instante : MonoBehaviour
{
    public GameObject painelLivroUI;
    public Image imagemNoCanvas;
    public Sprite imagemDoLivro;

    private void OnMouseDown()
    {
        Interagir();
    }

    public void Interagir()
    {
        if (painelLivroUI == null) return;

        bool estaAberto = painelLivroUI.activeSelf;

        if (estaAberto)
        {
            Fechar();
        }
        else
        {
            Abrir();
        }
    }

    public void Abrir()
    {
        if (painelLivroUI == null) return;

        if (imagemNoCanvas != null && imagemDoLivro != null)
        {
            imagemNoCanvas.sprite = imagemDoLivro;
        }

        painelLivroUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Fechar()
    {
        if (painelLivroUI == null) return;

        painelLivroUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
