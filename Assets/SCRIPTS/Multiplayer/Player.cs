using System.Collections.Generic;
using Fusion;
using UnityEngine;
public class Player : NetworkBehaviour
{
    public static bool LocalSpawnou { get; private set; } = false;
    public static Transform LocalTransform { get; private set; }
    public static Transform LocalPontoMao { get; private set; }
    public static Camera LocalCamera { get; private set; }

    // Referencia ao Player do jogador local (usado pra carregar objetos em rede).
    public static Player Local { get; private set; }

    // Ponto da mao exposto pra rede: o host le a mao do dono pra mover o objeto.
    public Transform PontoMao => pontoMao;

    public static event System.Action SolicitarAtivarCamera;

    // Personagem (Kofi/Aldric) sincronizado na rede. Escrito apenas pelo dono do objeto.
    [Networked] public Personagem PersonagemAtual { get; set; }

    // Registro local de todos os Players conhecidos nesta máquina (local + proxies).
    // Usado para resolver conflito de personagem entre os jogadores.
    private static readonly List<Player> _todosJogadores = new List<Player>();

    public static void AtivarCamerasJogadores()
    {
        SolicitarAtivarCamera?.Invoke();
    }

    private NetworkCharacterController _cc;

    [SerializeField] private Personagem personagemDoPrefab = Personagem.Kofi;
    [SerializeField] private float speed = 15f;
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private Transform pontoMao;

    [Networked] private NetworkButtons PreviousButtons { get; set; }

    // Personagem com que este objeto foi posicionado. Se a rede resolver um
    // conflito e trocar o personagem, reposicionamos no ponto de spawn certo.
    private Personagem _personagemPosicionado;
    private bool _posicaoInicializada = false;

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();
    }

    public override void Spawned()
    {
        _todosJogadores.Add(this);

        // No modo Shared, quem dá Spawn é dono (State + Input Authority) do próprio
        // objeto, então define o personagem inicial a partir da escolha salva.
        if (HasStateAuthority)
        {
            PersonagemAtual = personagemDoPrefab;
            _personagemPosicionado = PersonagemAtual;
            _posicaoInicializada = true;
        }

        int localLayer   = LayerMask.NameToLayer("LocalPlayer");
        int defaultLayer = LayerMask.NameToLayer("Default");

        if (HasInputAuthority)
        {
            BasicSpawner.PersonagemLocal = PersonagemAtual;
            LocalTransform = transform;
            LocalPontoMao = pontoMao;
            LocalCamera = cameraHolder != null ? cameraHolder.GetComponentInChildren<Camera>() : Camera.main;
            LocalSpawnou = true;
            Local = this;

            // Próprio corpo → LocalPlayer: câmera local não renderiza
            if (localLayer >= 0)
            {
                foreach (var r in GetComponentsInChildren<Renderer>())
                    r.gameObject.layer = localLayer;

                Camera cam = cameraHolder != null ? cameraHolder.GetComponentInChildren<Camera>() : null;
                if (cam != null)
                    cam.cullingMask &= ~(1 << localLayer);
            }

            if (!CutsceneFase1.Ativa)
            {
                AtivarCamera();
            }
            else
            {
                if (cameraHolder != null) cameraHolder.gameObject.SetActive(false);
                SolicitarAtivarCamera += AtivarCamera;
            }
        }
        else
        {
            // Proxy → força Default: garante visibilidade mesmo que o prefab
            // tenha ficado com LocalPlayer salvo de um teste anterior
            if (defaultLayer >= 0)
                foreach (var r in GetComponentsInChildren<Renderer>())
                    r.gameObject.layer = defaultLayer;

            if (cameraHolder != null)
                cameraHolder.gameObject.SetActive(false);
        }
    }

    private void AtivarCamera()
    {
        SolicitarAtivarCamera -= AtivarCamera;

        if (cameraHolder == null) return;

        Camera sceneCam = Camera.main;
        if (sceneCam != null && !sceneCam.transform.IsChildOf(transform))
        {
            AudioListener al = sceneCam.GetComponent<AudioListener>();
            if (al != null) al.enabled = false;
            sceneCam.gameObject.SetActive(false);
        }

        cameraHolder.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        _todosJogadores.Remove(this);

        if (HasInputAuthority)
        {
            SolicitarAtivarCamera -= AtivarCamera;
            LocalSpawnou = false;
            LocalTransform = null;
            LocalPontoMao = null;
            LocalCamera = null;
            Local = null;
        }
    }

    // Move o jogador para o ponto de spawn do personagem resolvido.
    private void Reposicionar(Personagem p)
    {
        Transform ponto = BasicSpawner.PontoDeSpawn(p);
        if (ponto == null) return;

        if (_cc != null)
            _cc.Teleport(ponto.position, ponto.rotation);
        else
            transform.SetPositionAndRotation(ponto.position, ponto.rotation);
    }

    // Lê a escolha feita na tela de seleção. Default: Kofi.
    private static Personagem LerPreferenciaPersonagem()
    {
        return PlayerPrefs.GetString("PapelEscolhido", "") == "Aldric"
            ? Personagem.Aldric
            : Personagem.Kofi;
    }

    // Garante que os dois jogadores não fiquem com o mesmo personagem.
    // Regra determinística: em caso de empate, quem tem o PlayerId menor mantém
    // a preferência e o outro recebe o personagem restante. Converge em um tick.
    private void ResolverConflitoPersonagem()
    {
        foreach (var outro in _todosJogadores)
        {
            if (outro == this || outro == null || !outro.Object) continue;

            if (outro.PersonagemAtual == PersonagemAtual &&
                outro.Object.InputAuthority.PlayerId < Object.InputAuthority.PlayerId)
            {
                PersonagemAtual = PersonagemAtual == Personagem.Kofi
                    ? Personagem.Aldric
                    : Personagem.Kofi;
                break;
            }
        }
    }

    public override void Render()
    {
        // Mantém o estático local em sincronia com o personagem resolvido na rede.
        if (HasInputAuthority)
            BasicSpawner.PersonagemLocal = PersonagemAtual;
    }

    public override void FixedUpdateNetwork()
    {
        // Só o dono do objeto escreve o próprio personagem; roda sempre,
        // inclusive durante a cutscene, para já entrar na fase sem conflito.
        if (HasStateAuthority)
        {
            ResolverConflitoPersonagem();

            // Se o conflito trocou o personagem, move para o ponto de spawn
            // correto para a posição não ficar errada.
            if (_posicaoInicializada && PersonagemAtual != _personagemPosicionado)
            {
                Reposicionar(PersonagemAtual);
                _personagemPosicionado = PersonagemAtual;
            }
        }

        if (_cc == null) return;
        if (CutsceneFase1.Ativa) return;

        if (GetInput(out NetworkInputData data))
        {
            transform.rotation = Quaternion.Euler(0, data.lookYaw, 0);

            if (cameraHolder != null)
            {
                cameraHolder.localRotation = Quaternion.Euler(data.lookPitch, 0, 0);
            }

            Vector3 moveDirection = transform.forward * data.direction.z + transform.right * data.direction.x;
            _cc.Move(speed * moveDirection * Runner.DeltaTime);

            if (data.buttons.WasPressed(PreviousButtons, InputButtons.Jump))
            {
                _cc.Jump();
            }

            PreviousButtons = data.buttons;
        }
    }
    private void Update()
    {

    }
}