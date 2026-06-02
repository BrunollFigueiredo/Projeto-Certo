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
        // Tempo real (não trava se o jogo pausar com Time.timeScale = 0)
        yield return new WaitForSecondsRealtime(delay);

        // Encerra a rede e ESPERA o shutdown terminar antes de recarregar.
        // Sem essa espera, a cena nova tenta abrir a rede com a sessão antiga
        // ainda aberta (mesmo nome de sessão) e dá conflito / erro de conexão.
        if (runner != null && runner.IsRunning)
        {
            var tarefa = runner.Shutdown();
            while (tarefa != null && !tarefa.IsCompleted)
            {
                yield return null;
            }
        }

        // Uma folga de um frame pra rede liberar de vez
        yield return null;

        SceneManager.LoadScene(cena);
        Destroy(gameObject);
    }
}
