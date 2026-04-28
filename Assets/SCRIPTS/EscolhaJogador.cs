using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EscolhaJogador : MonoBehaviour
{
    [SerializeField] private Button botaoJogador1;
    [SerializeField] private Button botaoJogador2;
    [SerializeField] private GameObject marcadorJogador1;
    [SerializeField] private GameObject marcadorJogador2;
    [SerializeField] private string cenaJogo = "Fase1";

    private bool escolheu = false;

    void Start()
    {
        if (marcadorJogador1 != null) marcadorJogador1.SetActive(false);
        if (marcadorJogador2 != null) marcadorJogador2.SetActive(false);
    }

    public void EscolherJogador1()
    {
        if (escolheu) return;
        Confirmar("Forca", marcadorJogador1);
    }

    public void EscolherJogador2()
    {
        if (escolheu) return;
        Confirmar("Inteligencia", marcadorJogador2);
    }

    void Confirmar(string papel, GameObject marcador)
    {
        escolheu = true;

        PlayerPrefs.SetString("PapelEscolhido", papel);
        PlayerPrefs.Save();

        botaoJogador1.interactable = false;
        botaoJogador2.interactable = false;

        if (marcador != null) marcador.SetActive(true);

        Invoke(nameof(IrParaJogo), 0.5f);
    }

    void IrParaJogo()
    {
        SceneManager.LoadScene(cenaJogo);
    }
}
