using UnityEngine;
using System.Collections;

public class PainelBotao : MonoBehaviour
{
    public int idBotao;
    public float velocidade = 15f;
    public float deslocamentoY = 3.5f;

    private Vector3 posOriginal;
    private bool estaAbaixado = false;
    private MeshRenderer meshRenderer;
    private Color corOriginal;

    void Start()
    {
        posOriginal = transform.localPosition;
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
            corOriginal = meshRenderer.material.color;
        }
    }

    private void OnMouseDown()
    {
        if (BasicSpawner.PapelLocal != PapelJogador.Forca)
        {
            FeedbackUI.Mostrar("Você não tem força suficiente para pressionar isso.");
            return;
        }

        Puzzlebotoes puzzle = Object.FindObjectOfType<Puzzlebotoes>();

        if (puzzle != null)
        {
            puzzle.TenteiPressionar(idBotao);
        }
    }

    public void Descer()
    {
        if (estaAbaixado) return;
        StopAllCoroutines();
        StartCoroutine(AnimarDescer());
    }

    public void SubirComCor(Color cor, bool resetarCorDepois)
    {
        if (!estaAbaixado) return;
        StopAllCoroutines();
        StartCoroutine(AnimarSubir(cor, resetarCorDepois));
    }

    IEnumerator AnimarDescer()
    {
        estaAbaixado = true;
        Vector3 destino = posOriginal + new Vector3(0f, -deslocamentoY, 0f);

        while (Vector3.Distance(transform.localPosition, destino) > 0.01f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, destino, velocidade * Time.deltaTime);
            yield return null;
        }

        transform.localPosition = destino;
    }

    IEnumerator AnimarSubir(Color cor, bool resetarCorDepois)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = cor;
        }

        while (Vector3.Distance(transform.localPosition, posOriginal) > 0.01f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, posOriginal, velocidade * Time.deltaTime);
            yield return null;
        }

        transform.localPosition = posOriginal;
        estaAbaixado = false;

        if (resetarCorDepois && meshRenderer != null)
        {
            yield return new WaitForSeconds(1f);
            meshRenderer.material.color = corOriginal;
        }
    }
}
