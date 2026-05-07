using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Fusion;

public class TransicaoFase : MonoBehaviour
{
    public static void Ir(NetworkRunner runner, string cena, float delay = 2f)
    {
        GameObject obj = new GameObject("_TransicaoFase");
        DontDestroyOnLoad(obj);
        TransicaoFase transicao = obj.AddComponent<TransicaoFase>();
        transicao.StartCoroutine(transicao.Executar(runner, cena, delay));
    }

    private IEnumerator Executar(NetworkRunner runner, string cena, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (runner != null && runner.IsRunning)
        {
            runner.Shutdown();
        }

        SceneManager.LoadScene(cena);
        Destroy(gameObject);
    }
}
