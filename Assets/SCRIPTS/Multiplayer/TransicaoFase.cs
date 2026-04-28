using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;

public class TransicaoFase : MonoBehaviour
{
    public static void Ir(NetworkRunner runner, string cena, float delay = 2f)
    {
        var go = new GameObject("_TransicaoFase");
        DontDestroyOnLoad(go);
        go.AddComponent<TransicaoFase>().Executar(runner, cena, delay);
    }

    private async void Executar(NetworkRunner runner, string cena, float delay)
    {
        await System.Threading.Tasks.Task.Delay((int)(delay * 1000));

        if (runner != null && runner.IsRunning)
            await runner.Shutdown();

        SceneManager.LoadScene(cena);
        Destroy(gameObject);
    }
}
