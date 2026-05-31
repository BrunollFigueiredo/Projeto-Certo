using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum InputButtons
{
    Jump = 0
}

public struct NetworkInputData : INetworkInput
{
    public Vector3 direction;
    public float lookYaw;
    public float lookPitch;
    public NetworkButtons buttons;
}

public enum Personagem { Kofi, Aldric }

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    [SerializeField] private Transform pontoDeSpawn;

    public static Vector2 TouchMoveInput;
    public static bool JumpPressed;
    public static float YawInput;
    public static float PitchInput;

    // Personagem do jogador local. Definido pelo Player local quando ele nasce
    // (e ajustado se houver conflito), por isso o setter é público.
    public static Personagem PersonagemLocal { get; set; } = Personagem.Kofi;
    public static int JogadoresConectados { get; private set; } = 0;

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
    private NetworkRunner _runner;

    private void Start()
    {
        // Zera o estado estático para que ele não vaze entre cenas/sessões.
        // (statics sobrevivem ao recarregar a cena; sem isso o contador e o
        // olhar acumulam valores antigos quando uma nova fase começa.)
        ResetarEstadoEstatico();

        if (_runner == null)
            StartGame(GameMode.Shared);
    }

    private static void ResetarEstadoEstatico()
    {
        JogadoresConectados = 0;
        TouchMoveInput = Vector2.zero;
        JumpPressed = false;
        YawInput = 0f;
        PitchInput = 0f;
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        Vector3 dir = new Vector3(TouchMoveInput.x, 0f, TouchMoveInput.y);
        if (dir.sqrMagnitude > 1f) dir.Normalize();
        data.direction = dir;

        data.buttons.Set(InputButtons.Jump, JumpPressed);
        data.lookYaw = YawInput;
        data.lookPitch = PitchInput;

        input.Set(data);
        JumpPressed = false;
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        JogadoresConectados++;

        if (player == runner.LocalPlayer)
        {
            // O personagem (Kofi/Aldric) é resolvido pelo próprio Player ao nascer,
            // já com checagem de unicidade na rede. Ver Player.cs.
            Vector3 posicao = pontoDeSpawn != null ? pontoDeSpawn.position : transform.position;
            Quaternion rotacao = pontoDeSpawn != null ? pontoDeSpawn.rotation : transform.rotation;
            runner.Spawn(_playerPrefab, posicao, rotacao, player);
        }
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        JogadoresConectados--;
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    async void StartGame(GameMode mode)
    {
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            PlayerCount = 10
        });
    }
}
