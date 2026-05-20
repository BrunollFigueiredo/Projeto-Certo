using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicial : MonoBehaviour
{
    public void Jogar()
    {
        SceneManager.LoadScene("Escolha Jogador");
    }
    public void Sair()
    {
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }
    public void Creditos()
    {
        SceneManager.LoadScene("Créditos");
    }
    public void Voltar()
    {
        SceneManager.LoadScene("Tela Inicial");
    }
}
