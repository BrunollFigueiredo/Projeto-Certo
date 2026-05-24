using System.Collections;
using UnityEngine;

public class Prensa : MonoBehaviour
{
    [SerializeField] private float distanciaDescida = 2f;
    [SerializeField] private float velocidade = 3f;
    [SerializeField] private bool voltarAposDescer = false;
    [SerializeField] private float tempoParaVoltar = 1.5f;
    [SerializeField] private PontoDeEncaixe pontoDoObjeto;
    public PontoDeEncaixe PontoDoObjeto => pontoDoObjeto;

    private Vector3 posicaoInicial;
    private Vector3 posicaoFinal;
    private bool ativada = false;
    private bool emMovimento = false;

    void Start()
    {
        posicaoInicial = transform.position;
        posicaoFinal = posicaoInicial - new Vector3(0f, distanciaDescida, 0f);
    }

    public void Ativar(Vector3 escalaAlvo)
    {
        if (ativada || emMovimento) return;

        if (pontoDoObjeto == null || !pontoDoObjeto.Ocupado)
        {
            Debug.LogWarning("[Prensa] Nenhum objeto no ponto de encaixe.");
            return;
        }

        StartCoroutine(CicloPrensagem(escalaAlvo));
    }

    IEnumerator CicloPrensagem(Vector3 escalaAlvo)
    {
        emMovimento = true;

        // Desce e escala o objeto ao mesmo tempo
        yield return MoverEEscalar(posicaoFinal, escalaAlvo);

        if (voltarAposDescer)
        {
            yield return new WaitForSeconds(tempoParaVoltar);
            yield return Mover(posicaoInicial);
            emMovimento = false;
        }
        else
        {
            ativada = true;
            emMovimento = false;
        }
    }

    IEnumerator MoverEEscalar(Vector3 destino, Vector3 escalaAlvo)
    {
        Transform objeto = pontoDoObjeto.ObjetoEncaixado;
        Vector3 escalaInicial = objeto != null ? objeto.localScale : Vector3.one;

        float duracao = distanciaDescida / velocidade;
        float tempo = 0f;
        Vector3 origemPrensagem = transform.position;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / duracao);

            transform.position = Vector3.Lerp(origemPrensagem, destino, t);

            if (objeto != null)
                objeto.localScale = Vector3.Lerp(escalaInicial, escalaAlvo, t);

            yield return null;
        }

        transform.position = destino;
        if (objeto != null)
            objeto.localScale = escalaAlvo;
    }

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
