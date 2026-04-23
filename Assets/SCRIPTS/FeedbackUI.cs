using UnityEngine;

public class FeedbackUI : MonoBehaviour
{
    public static FeedbackUI Instance;

    private string _mensagem = "";
    private float _tempoRestante = 0f;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (_tempoRestante > 0f)
            _tempoRestante -= Time.deltaTime;
    }

    void OnGUI()
    {
        if (_tempoRestante > 0f && !string.IsNullOrEmpty(_mensagem))
        {
            GUIStyle estilo = new GUIStyle(GUI.skin.box);
            estilo.fontSize = 28;
            estilo.alignment = TextAnchor.MiddleCenter;
            float w = 520, h = 80;
            float x = (Screen.width - w) / 2f;
            float y = Screen.height * 0.75f;
            GUI.Box(new Rect(x, y, w, h), _mensagem, estilo);
        }
    }

    public static void Mostrar(string mensagem, float duracao = 2.5f)
    {
        if (Instance == null)
        {
            var go = new GameObject("FeedbackUI");
            Instance = go.AddComponent<FeedbackUI>();
            DontDestroyOnLoad(go);
        }
        Instance._mensagem = mensagem;
        Instance._tempoRestante = duracao;
    }
}
