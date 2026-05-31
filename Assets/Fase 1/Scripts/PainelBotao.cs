using UnityEngine;
using System.Collections;

// Controla um botão do painel (descer, subir e mudar cor)
public class PainelBotao : MonoBehaviour
{
    public int idBotao;              // Número que identifica o botão na sequência
    public float velocidade = 15f;   // Velocidade da animação de subir/descer
    public float deslocamentoY = 3.5f; // Quanto o botão desce no eixo Y

    private Vector3 posOriginal;     // Posição inicial do botão
    private bool estaAbaixado;       // Se o botão está pressionado
    private MeshRenderer meshRenderer; // Renderer usado para mudar a cor
    private Color corOriginal;       // Cor original do botão

    void Start()
    {
        // Guarda a posição inicial e a cor original do botão
        posOriginal = transform.localPosition;
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
            corOriginal = meshRenderer.material.color;
        }
    }

    // Chamado quando o jogador clica no botão
    void OnMouseDown()
    {
        // Só o Kofi pode pressionar os botões do painel
        if (BasicSpawner.PersonagemLocal != Personagem.Kofi)
        {
            FeedbackUI.Mostrar("Você não tem força suficiente para pressionar isso.");
            return;
        }

        // Procura o puzzle na cena e avisa qual botão foi clicado
        Puzzlebotoes puzzle = FindObjectOfType<Puzzlebotoes>();

        if (puzzle != null)
        {
            puzzle.TenteiPressionar(idBotao);
        }
    }

    // Inicia a animação de descer o botão
    public void Descer()
    {
        if (estaAbaixado) return;
        StopAllCoroutines();
        StartCoroutine(AnimarDescer());
    }

    // Inicia a animação de subir e troca a cor (verde se acertou, vermelho se errou)
    public void SubirComCor(Color cor, bool resetarCorDepois)
    {
        if (!estaAbaixado) return;
        StopAllCoroutines();
        StartCoroutine(AnimarSubir(cor, resetarCorDepois));
    }

    // Anima o botão descendo até a posição alvo
    IEnumerator AnimarDescer()
    {
        estaAbaixado = true;
        Vector3 destino = new Vector3(posOriginal.x, posOriginal.y - deslocamentoY, posOriginal.z);

        // Move um pouco por frame até chegar no destino
        while (Vector3.Distance(transform.localPosition, destino) > 0.01f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, destino, velocidade * Time.deltaTime);
            yield return null;
        }

        transform.localPosition = destino;
    }

    // Anima o botão subindo e troca a cor para mostrar acerto/erro
    IEnumerator AnimarSubir(Color cor, bool resetarCorDepois)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = cor;
        }

        // Move um pouco por frame até voltar para a posição inicial
        while (Vector3.Distance(transform.localPosition, posOriginal) > 0.01f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, posOriginal, velocidade * Time.deltaTime);
            yield return null;
        }

        transform.localPosition = posOriginal;
        estaAbaixado = false;

        // Se errou, espera 1 segundo e volta para a cor original
        if (resetarCorDepois && meshRenderer != null)
        {
            yield return new WaitForSeconds(1f);
            meshRenderer.material.color = corOriginal;
        }
    }
}
