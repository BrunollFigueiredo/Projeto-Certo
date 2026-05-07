using UnityEngine;
using System.Collections;

public class PortaAutomatica : MonoBehaviour
{
    public Transform portaDireita;
    public Transform portaEsquerda;

    public float distanciaAbertura = 8f;
    public float velocidade = 3f;

    private Vector3 posOriginalDireita;
    private Vector3 posOriginalEsquerda;
    private Vector3 posAbertaDireita;
    private Vector3 posAbertaEsquerda;

    private bool aberta = false;
    private bool animando = false;
    private bool jogadorDentro = false;

    void Start()
    {
        posOriginalDireita = portaDireita.localPosition;
        posOriginalEsquerda = portaEsquerda.localPosition;

        posAbertaDireita = posOriginalDireita + new Vector3(-distanciaAbertura, 0f, 0f);
        posAbertaEsquerda = posOriginalEsquerda + new Vector3(distanciaAbertura, 0f, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        jogadorDentro = true;

        if (!aberta && !animando)
        {
            StartCoroutine(AnimarPorta(true));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        jogadorDentro = false;

        if (aberta && !animando)
        {
            StartCoroutine(AnimarPorta(false));
        }
    }

    IEnumerator AnimarPorta(bool abrindo)
    {
        animando = true;

        Vector3 destDireita;
        Vector3 destEsquerda;

        if (abrindo)
        {
            destDireita = posAbertaDireita;
            destEsquerda = posAbertaEsquerda;
        }
        else
        {
            destDireita = posOriginalDireita;
            destEsquerda = posOriginalEsquerda;
        }

        while (Vector3.Distance(portaDireita.localPosition, destDireita) > 0.01f)
        {
            portaDireita.localPosition = Vector3.MoveTowards(portaDireita.localPosition, destDireita, velocidade * Time.deltaTime);
            portaEsquerda.localPosition = Vector3.MoveTowards(portaEsquerda.localPosition, destEsquerda, velocidade * Time.deltaTime);
            yield return null;
        }

        portaDireita.localPosition = destDireita;
        portaEsquerda.localPosition = destEsquerda;
        aberta = abrindo;
        animando = false;

        if (aberta && !jogadorDentro)
        {
            StartCoroutine(AnimarPorta(false));
        }
    }
}
