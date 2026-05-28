using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicial : MonoBehaviour
{
    public void Jogar()
    {
        SceneManager.LoadScene("CutsceneInicio");
    }
    public void Sair()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
Application.Quit();
#endif
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
