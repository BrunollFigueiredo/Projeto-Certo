using System.Collections;
using UnityEngine;

// Prensa que desce e escala o objeto encaixado no ponto
public class Prensa : MonoBehaviour
{
    [SerializeField] private float distanciaDescida = 2f;      // Distância que a prensa desce
    [SerializeField] private float velocidade = 3f;            // Velocidade do movimento
    [SerializeField] private bool voltarAposDescer = false;    // Se a prensa deve subir sozinha depois
    [SerializeField] private float tempoParaVoltar = 1.5f;     // Quanto tempo espera antes de subir
    [SerializeField] private PontoDeEncaixe pontoDoObjeto;     // Onde o objeto fica preso

    // Propriedade pública para o painel acessar o ponto de encaixe
    public PontoDeEncaixe PontoDoObjeto
    {
        get { return pontoDoObjeto; }
    }

    private Vector3 posicaoInicial;     // Posição inicial (em cima)
    private Vector3 posicaoFinal;       // Posição final (embaixo)
    private bool ativada = false;       // Se já prensou e ficou abaixada
    private bool emMovimento = false;   // Se está se movendo agora

    void Start()
    {
        // Guarda as posições inicial e final calculadas a partir da posição atual
        posicaoInicial = transform.position;
        posicaoFinal = posicaoInicial - new Vector3(0f, distanciaDescida, 0f);
    }

    // Levanta a prensa de volta para a posição inicial
    public IEnumerator Levantar()
    {
        // Espera caso ainda esteja se movendo
        while (emMovimento)
        {
            yield return null;
        }

        // Sobe até a posição inicial se não estiver lá
        if (Vector3.Distance(transform.position, posicaoInicial) > 0.01f)
        {
            yield return Mover(posicaoInicial);
        }

        ativada = false;
        emMovimento = false;
    }

    // Ativa a prensa com a escala alvo (chamado pelo painel)
    public void Ativar(Vector3 escalaAlvo)
    {
        // Ignora se já está prensada ou se movendo
        if (ativada) return;
        if (emMovimento) return;

        // Precisa ter um objeto encaixado para prensar
        if (pontoDoObjeto == null || !pontoDoObjeto.Ocupado)
        {
            Debug.LogWarning("[Prensa] Nenhum objeto no ponto de encaixe.");
            return;
        }

        StartCoroutine(CicloPrensagem(escalaAlvo));
    }

    // Faz o ciclo completo: desce, prensa, opcionalmente sobe
    IEnumerator CicloPrensagem(Vector3 escalaAlvo)
    {
        emMovimento = true;

        // Desce e ao mesmo tempo redimensiona o objeto
        yield return MoverEEscalar(posicaoFinal, escalaAlvo);

        if (voltarAposDescer)
        {
            // Espera um pouco e sobe sozinha
            yield return new WaitForSeconds(tempoParaVoltar);
            yield return Mover(posicaoInicial);
            emMovimento = false;
        }
        else
        {
            // Fica abaixada até alguém mandar levantar
            ativada = true;
            emMovimento = false;
        }
    }

    // Movimento de descida que também muda a escala do objeto
    IEnumerator MoverEEscalar(Vector3 destino, Vector3 escalaAlvo)
    {
        Transform objeto = pontoDoObjeto.ObjetoEncaixado;

        // Pega a escala inicial do objeto para fazer a interpolação
        Vector3 escalaInicial = Vector3.one;
        if (objeto != null)
        {
            escalaInicial = objeto.localScale;
        }

        // Calcula o tempo total baseado em distância e velocidade
        float duracao = distanciaDescida / velocidade;
        float tempo = 0f;
        Vector3 origemPrensagem = transform.position;

        // Interpola posição da prensa e escala do objeto ao mesmo tempo
        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / duracao);

            transform.position = Vector3.Lerp(origemPrensagem, destino, t);

            if (objeto != null)
            {
                objeto.localScale = Vector3.Lerp(escalaInicial, escalaAlvo, t);
            }

            yield return null;
        }

        // Garante valores finais exatos
        transform.position = destino;

        if (objeto != null)
        {
            objeto.localScale = escalaAlvo;
        }
    }

    // Movimento simples (usado para subir de volta)
    IEnumerator Mover(Vector3 destino)
    {
        while (Vector3.Distance(transform.position, destino) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destino, velocidade * Time.deltaTime);
            yield return null;
        }

        transform.position = destino;
    }
}
